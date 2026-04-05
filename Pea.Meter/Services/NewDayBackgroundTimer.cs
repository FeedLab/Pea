using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;

namespace Pea.Meter.Services;

public class NewDayBackgroundTimer(
    ILogger<NewDayBackgroundTimer> logger,
    PeaDbContextFactory dbContextFactory,
    PeaAdapter peaAdapter,
    StorageService storageService,
    DailyPeaReadingsTimer dailyPeaReadingsTimer)
{
    private DateTime yesterday = DateTime.MinValue; // MinValue - Will trigger a new day on the first run
    private Timer? newDayTimer;

    public void  Start()
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
        var meterReadingRepository = new MeterReadingRepository(dbContextFactory);

        newDayTimer = new Timer(async void (_) =>
            {
                var today = DateTime.Now.Date;

                try
                {
                    logger.LogInformation("Checking for new day at {CurrentDate}", today);

                    if (today > yesterday)
                    {
                        var dailyReadings = await peaAdapter.ShowDailyReadings(today);
                        var readingsFromPeaToday = dailyReadings ??= new List<PeaMeterReading>();

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

                        logger.LogInformation("Found new day, Current day: {CurrentDay}, Now: {Now}",
                            yesterday,
                            today);

                        if (readingsFromPeaYesterday is not null)
                        {
                            await meterReadingRepository.AddRangeUpsertAsync(readingsFromPeaYesterday.ToList());
                        }

                        var allReadingsFromDb =
                            await RetrieveAndProcessAllMeterReadingsFromDb(meterReadingRepository);

                        await ProcessPeaReadingsAndNotify(readingsFromPeaToday, allReadingsFromDb, today);

                        yesterday = today;

                        // await ExportAllMeterReadingsToJsonAsync();
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
            TimeSpan.FromMinutes(15));
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
        var readingsFromDb = await meterReadingRepository.GetAllMeterReadingsAsync();

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