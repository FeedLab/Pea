using Microsoft.Extensions.Logging;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;
using Pea.Meter.Extension;

namespace Pea.Meter.Services;

public class DailyPeaReadingsTimer
{
    private readonly ILogger<DailyPeaReadingsTimer> logger;
    private readonly PeaAdapter peaAdapter;
    private readonly StorageService storageService;
    private readonly MeterReadingRepository meterReadingRepository;
    private Timer? dailyTimer;

    public DailyPeaReadingsTimer(ILogger<DailyPeaReadingsTimer> logger, PeaAdapter peaAdapter, StorageService storageService, MeterReadingRepository meterReadingRepository)
    {
        this.logger = logger;
        this.peaAdapter = peaAdapter;
        this.storageService = storageService;
        this.meterReadingRepository = meterReadingRepository;
    }

    private void ReadingsTimer()
    {
        // Run aggregations every 15 minutes
        dailyTimer = new Timer(async void (_) =>
        {
            try
            {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            await FetchAndFilterDailyReadings();
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "Error in background task: {Message}", e.Message);
                        }
                    });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in background task: {Message}", e.Message);
            }
        }, null, TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(15));
    }
    
    private async Task FetchAndFilterDailyReadings()
    {
        logger.LogInformation("Fetching daily readings from Pea Adapter");

        await MainThread.InvokeOnMainThreadAsync(async () => { await ProcessNewPeriodReadings(); });
    }
    
    private async Task ProcessNewPeriodReadings()
    {
        var readingsFromDb = await meterReadingRepository.GetAllMeterReadingsAsync();
        var readingsFromPea = await peaAdapter.ShowDailyReadings(DateTime.Today);

        
        if (readingsFromPea == null)
        {
            logger.LogWarning("Failed to fetch daily readings from Pea Adapter");
            return;
        }

        var newReadingsFiltered = readingsFromPea
            .Where(r => r.Total > 0)
            .ToList();

        logger.LogInformation($"Found {newReadingsFiltered.Count} new readings");

        if (newReadingsFiltered.Count > 0)
        {
            await storageService.UpdatePeriodDataAndProcessAggregations(newReadingsFiltered.ToList(), readingsFromDb.ToList());
        }
    }
    
    private void Stop()
    {
        dailyTimer?.Dispose();
        dailyTimer = null;
    }
}