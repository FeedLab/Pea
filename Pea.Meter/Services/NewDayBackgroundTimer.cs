using CommunityToolkit.Maui.Core.Extensions;
using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;

namespace Pea.Meter.Services;

public class NewDayBackgroundTimer(
    ILogger<NewDayBackgroundTimer> logger,
    PeaDbContextFactory dbContextFactory,
    PeaAdapter peaAdapter,
    StorageService storageService)
{
    private DateTime yesterday = DateTime.MinValue; // MinValue - Will trigger a new day on the first run
    private Timer? newDayTimer;

    public async Task Start()
    {
        if (newDayTimer != null)
        {
            logger.LogWarning("newDayTimer is already running");
            return; 
        }

        await CheckForNewDayTimer();
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

    private Task CheckForNewDayTimer()
    {
        const int startTimeDelay = 0;
        var context = dbContextFactory.CreateDbContext();
        var meterReadingRepository = new MeterReadingRepository(context);

        newDayTimer = new Timer(async void (_) =>
            {
                var today = DateTime.Now.Date;

                try
                {
                    logger.LogInformation("Stopping background task for checking for new day");
                    //StopBackgroundTask();

                    logger.LogInformation("Checking for new day at {CurrentDate}",
                        today);

                    if (today > yesterday)
                    {
                        var readingsFromPeaToday = await peaAdapter.ShowDailyReadings(today);

                        var readingsFromPeaYesterday =
                            yesterday != DateTime.MinValue
                                ? await peaAdapter.ShowDailyReadings(yesterday)
                                : null;

                        if (readingsFromPeaToday is null ||
                            readingsFromPeaToday.Count == 0 ||
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

                        var peaMeterReadingsInOrder =
                            await RetrieveAndProcessAllMeterReadingsFromDb(meterReadingRepository);
                        
                        await storageService.InitNewDay(yesterday,
                            today,
                            readingsFromPeaToday,
                            peaMeterReadingsInOrder);

                        logger.LogInformation("InitNewDay: Old date: {OldDate}, New date: {NewDate}",
                            yesterday,
                            today);

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
                finally
                {
                    yesterday = today;
                }
            },
            null,
            TimeSpan.FromSeconds(startTimeDelay),
            TimeSpan.FromMinutes(12));

        return Task.CompletedTask;
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