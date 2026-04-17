using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;
using Pea.Meter.Models;

namespace Pea.Meter.Services;

public class NewDayBackgroundTimer
{
    private DateTime yesterday = DateTime.MinValue; // MinValue - Will trigger a new day on the first run
    private Timer? newDayTimer;
    private readonly ILogger<NewDayBackgroundTimer> logger;
    private readonly PeaDbContextFactory dbContextFactory;
    private readonly IPeaAdapter peaAdapter;
    private readonly StorageService storageService;

    public NewDayBackgroundTimer(ILogger<NewDayBackgroundTimer> logger,
        PeaDbContextFactory dbContextFactory,
        IPeaAdapter peaAdapter,
        StorageService storageService,
        DailyPeaReadingsTimer dailyPeaReadingsTimer)
    {
        this.logger = logger;
        this.dbContextFactory = dbContextFactory;
        this.peaAdapter = peaAdapter;
        this.storageService = storageService;

        WeakReferenceMessenger.Default.Register<UserAccountRemovedMessage>(this,
            (r, m) => { MainThread.InvokeOnMainThreadAsync(async () => { Stop(); }); });
    }

    public void Start()
    {
        if (newDayTimer != null)
        {
            logger.LogWarning("newDayTimer is already running");
            return;
        }

        CheckForNewDayTimer();
    }

    public void Stop()
    {
        if (newDayTimer != null)
        {
            logger.LogInformation("Cancelling newDayTimer.");
            newDayTimer.Dispose();
            newDayTimer = null;
        }
    }

    private void CheckForNewDayTimer()
    {
        const int startTimeDelay = 2;
        yesterday = DateTime.Today.Date.AddDays(-1);
        var loggerRepository = AppService.GetRequiredService<ILogger<MeterReadingRepository>>();
        var meterReadingRepository = new MeterReadingRepository(loggerRepository, dbContextFactory);

        newDayTimer = new Timer(async void (_) =>
            {
                try
                {
                    var today = DateTime.Now.Date;
                    logger.LogInformation("Starting to check if we have a new Date at {CurrentDate}", today);

                    if (today > yesterday)
                    {
                        logger.LogInformation("YES, we have a new day...");
                        
                        var dailyReadings = await peaAdapter.ShowDailyReadings(today);
                        var readingsFromPeaToday = dailyReadings ?? new List<PeaMeterReading>();

                        var readingsFromPeaYesterday =
                            yesterday != DateTime.MinValue
                                ? await peaAdapter.ShowDailyReadings(yesterday)
                                : null;

                        if (readingsFromPeaToday.Count == 0 ||
                            readingsFromPeaToday.Last()
                                .PeriodStart.Date !=
                            today)
                        {
                            logger.LogWarning(
                                "We have a new day but no new readings for today, technically, we are not in a new day until the lagging data has catch up. So, we don't need to do anything");
                            return;
                        }

                        logger.LogInformation("Found new readings from PEA for TODAY, Yesterday: {Yesterday}, Today: {Today}",
                            yesterday,
                            today);

                        
                        if (readingsFromPeaYesterday is null || readingsFromPeaYesterday.Count == 0 )
                        {
                            logger.LogWarning("Could not find any PEA readings for yesterday, Yesterday: {Yesterday}, Today: {Today}",
                                yesterday,
                                today);
                            return;
                        }
                        else
                        {
                            logger.LogInformation("Found {Count} PEA readings for yesterday, Yesterday: {Yesterday}, Today: {Today}",
                                readingsFromPeaYesterday.Count,
                                yesterday,
                                today);
                            
                            var peaAdapterMeterNumber = peaAdapter.MeterNumber ?? "N/A";

                            await meterReadingRepository.AddRangeUpsertAsync(readingsFromPeaYesterday.ToList(),
                                peaAdapterMeterNumber);
                        }

                        var allReadingsFromDb =
                            await RetrieveAndProcessAllMeterReadingsFromDb(meterReadingRepository);

                        await ProcessPeaReadingsAndNotify(readingsFromPeaToday, allReadingsFromDb, today);

                        logger.LogInformation("Finished processing new day, Yesterday: {Yesterday}, Today: {Today}",
                            yesterday,
                            today);
                        
                        yesterday = today;
                    }
                    else
                    {
                        logger.LogInformation("No new day found: {Yesterday}, Today: {Today}", yesterday, today);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e,
                        "Error in background task ({backgroundTaskName}): {Message}",
                        nameof(CheckForNewDayTimer),
                        e.Message);
                }
            },
            null,
            TimeSpan.FromSeconds(startTimeDelay),
            TimeSpan.FromMinutes(5));
    }

    private async Task ProcessPeaReadingsAndNotify(IList<PeaMeterReading> readingsFromPeaToday,
        List<PeaMeterReading> allReadingsFromDb, DateTime today)
    {
        try
        {
            var peaReadingsFiltered = readingsFromPeaToday
                .Where(r => r.Total > 0)
                .ToList();

            await storageService.UpdatePeriodDataAndProcessAggregations(peaReadingsFiltered,
                allReadingsFromDb);

            WeakReferenceMessenger.Default.Send(new DateChangedMessage(yesterday, today));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error in {Method}: {Message}", nameof(CheckForNewDayTimer),
                exception.Message);
        }
    }

    private async Task<List<PeaMeterReading>> RetrieveAndProcessAllMeterReadingsFromDb(
        MeterReadingRepository meterReadingRepository)
    {
        var meterNumber = peaAdapter.MeterNumber ?? "N/A";
        var readingsFromDb = await meterReadingRepository.GetAllMeterReadingsAsync(meterNumber);

        if (readingsFromDb.Count == 0)
        {
            logger.LogWarning("No meter readings found in database");
            return [];
        }

        var peaMeterReadingsInOrder = readingsFromDb
            .OrderBy(r => r.PeriodStart)
            .ToList();

        return peaMeterReadingsInOrder;
    }
}