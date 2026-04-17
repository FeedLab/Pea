using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;
using Pea.Meter.Models;

namespace Pea.Meter.Services;

public class DailyPeaReadingsTimer
{
    private Timer? dailyTimer;
    private IList<PeaMeterReading>? readingsFromPea;
    private DateTime selectedDate = DateTime.MinValue;
    private readonly ILogger<DailyPeaReadingsTimer> logger;
    private readonly IPeaAdapter peaAdapter;
    private readonly StorageService storageService;
    private readonly PeaDbContextFactory dbContextFactory;
    private bool isTimerRunning;

    public DailyPeaReadingsTimer(ILogger<DailyPeaReadingsTimer> logger,
        IPeaAdapter peaAdapter,
        StorageService storageService,
        PeaDbContextFactory dbContextFactory)
    {
        this.logger = logger;
        this.peaAdapter = peaAdapter;
        this.storageService = storageService;
        this.dbContextFactory = dbContextFactory;

        WeakReferenceMessenger.Default.Register<UserAccountRemovedMessage>(this,
            (r, m) => { MainThread.InvokeOnMainThreadAsync(async () => { Stop(); }); });

        WeakReferenceMessenger.Default.Register<DateChangedMessage>(this, async void (r, m) =>
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        Stop();
                        await Start(m.NewDate);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(DateChangedMessage), e.Message);
                    }
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in {Method}: {Message}", nameof(DateChangedMessage), e.Message);
            }
        });
    }

    public IList<PeaMeterReading> LatestReadingsFromPea => readingsFromPea ??= new List<PeaMeterReading>();

    public DateTime SelectedDate => selectedDate;

    public void Stop()
    {
        dailyTimer?.Dispose();
        dailyTimer = null;
    }

    private async Task Start(DateTime date)
    {
        if (dailyTimer != null)
        {
            logger.LogWarning("DailyPeaReadingsTimer is already running");
            return;
        }

        selectedDate = date;
        await ReadingsTimer();
    }

    public async Task Start()
    {
        if (dailyTimer != null)
        {
            logger.LogWarning("DailyPeaReadingsTimer is already running");
            return;
        }

        selectedDate = DateTime.Today;
        await ReadingsTimer();
    }

    private async Task ReadingsTimer()
    {
        await Task.Delay(1000);

        // Run aggregations every 15 minutes
        dailyTimer = new Timer(async void (_) =>
        {
            try
            {
                isTimerRunning = true;

                var loggerRepository = AppService.GetRequiredService<ILogger<MeterReadingRepository>>();
                var meterReadingRepository = new MeterReadingRepository(loggerRepository, dbContextFactory);

                await FetchAndFilterDailyReadings(meterReadingRepository);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in background task: {Message}", e.Message);
            }
            finally
            {
                isTimerRunning = false;
            }
        }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(15));
    }

    private async Task FetchAndFilterDailyReadings(MeterReadingRepository meterReadingRepository)
    {
        logger.LogInformation("Fetching daily readings from Pea Adapter");
        var meterNumber = peaAdapter.MeterNumber ?? "N/A";

        var readingsFromDb = await meterReadingRepository.GetAllMeterReadingsAsync(meterNumber);
        readingsFromPea = await peaAdapter.ShowDailyReadings(selectedDate);

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