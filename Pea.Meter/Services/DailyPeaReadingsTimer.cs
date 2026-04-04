using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;

namespace Pea.Meter.Services;

public class DailyPeaReadingsTimer
{
    private Timer? dailyTimer;
    private IList<PeaMeterReading>? readingsFromPea;
    private readonly ILogger<DailyPeaReadingsTimer> logger;
    private readonly PeaAdapter peaAdapter;
    private readonly StorageService storageService;
    private readonly PeaDbContextFactory dbContextFactory;

    public DailyPeaReadingsTimer(ILogger<DailyPeaReadingsTimer> logger,
        PeaAdapter peaAdapter,
        StorageService storageService,
        PeaDbContextFactory dbContextFactory)
    {
        this.logger = logger;
        this.peaAdapter = peaAdapter;
        this.storageService = storageService;
        this.dbContextFactory = dbContextFactory;
    }

    public IList<PeaMeterReading> LatestReadingsFromPea => readingsFromPea ??= new List<PeaMeterReading>();

    public void Stop()
    {
        dailyTimer?.Dispose();
        dailyTimer = null;
    }

    public void Start()
    {
        if (dailyTimer != null)
        {
            logger.LogWarning("DailyPeaReadingsTimer is already running");
            return;
        }

        ReadingsTimer();
    }

    private void ReadingsTimer()
    {
        // Run aggregations every 15 minutes
        dailyTimer = new Timer(async void (_) =>
        {
            try
            {
                var meterReadingRepository = new MeterReadingRepository(dbContextFactory);
                
                await FetchAndFilterDailyReadings(meterReadingRepository);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in background task: {Message}", e.Message);
            }
        }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(15));
    }

    private async Task FetchAndFilterDailyReadings(MeterReadingRepository meterReadingRepository)
    {
        logger.LogInformation("Fetching daily readings from Pea Adapter");

        var readingsFromDb = await meterReadingRepository.GetAllMeterReadingsAsync();
        readingsFromPea = await peaAdapter.ShowDailyReadings(DateTime.Today);

        if (readingsFromPea == null)
        {
            logger.LogWarning("Failed to fetch daily readings from Pea Adapter");
            return;
        }

        var newReadingsFiltered = LatestReadingsFromPea
            .Where(r => r.Total > 0)
            .ToList();

        logger.LogInformation($"Found {newReadingsFiltered.Count} new readings");

        if (newReadingsFiltered.Count > 0)
        {
            await storageService.UpdatePeriodDataAndProcessAggregations(newReadingsFiltered.ToList(),
                readingsFromDb.ToList());
        }
    }
}