using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Messaging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;

namespace Pea.Meter.Services;

public class StorageService
{
    private ObservableCollection<PeaMeterReading> allMeterReadingsAsync;
    private ObservableCollection<PeaMeterReading> hourlyAggregated;
    private ObservableCollection<PeaMeterReading> dailyAggregated;
    private ObservableCollection<PeaMeterReading> weeklyAggregated;
    private ObservableCollection<PeaMeterReading> dailyReadings = [];
    private readonly PeaDbContextFactory dbContextFactory;
    private readonly PeaAdapter peaAdapter;
    private CancellationTokenSource? cancellationTokenSource;
    private Timer? backgroundTimer;
    private MeterReadingRepository meterReadingRepository;

    public StorageService(PeaDbContextFactory dbContextFactory, PeaAdapter peaAdapter)
    {
        this.dbContextFactory = dbContextFactory;
        this.peaAdapter = peaAdapter;

        // WeakReferenceMessenger.Default.Register<DataImportedMessage>(this, async (r, m) =>
        // {
        //     ProcessAggregations();
        // });
    }

    public bool IsAuthenticated { get; set; }

    public async Task Init()
    {
        var context = dbContextFactory.CreateDbContext();
        meterReadingRepository = new MeterReadingRepository(context);
        var allMeterReadingsList = await meterReadingRepository.GetAllMeterReadingsAsync();
        allMeterReadingsAsync = allMeterReadingsList.ToObservableCollection();

        await FetchAndFilterDailyReadings();

        // Start aggregations in background without blocking
        await Task.Run(() =>
        {
            ProcessAggregations();
            WeakReferenceMessenger.Default.Send(new AllAggregationsCompletedMessage());
        });

        StartBackgroundTask();
    }

    private async Task FetchAndFilterDailyReadings()
    {
        var readings = await peaAdapter.ShowDailyReadings(DateTime.Today);

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            dailyReadings.Clear();
            foreach (var reading in readings.Where(r => r.Total > 0))
            {
                if (reading.PeriodStart < DateTime.Now)
                {
                    dailyReadings.Add(reading);
                }
            }
        });
    }

    private void StartBackgroundTask()
    {
        cancellationTokenSource = new CancellationTokenSource();

        // Run aggregations every 15 minutes
        backgroundTimer = new Timer(async _ =>
        {
            if (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                await Task.Run(async () =>
                {
                    await FetchAndFilterDailyReadings();

                    var lastDailyReadings = allMeterReadingsAsync.LastOrDefault()?.PeriodStart.Date;
                    var currentMeterReadingDay = dailyReadings.LastOrDefault()?.PeriodStart.Date;

                    if (lastDailyReadings is not null && currentMeterReadingDay is not null)
                    {
                        if (currentMeterReadingDay.Value.AddDays(-1).Date > lastDailyReadings)
                        {
                            var allMeterReadingsList = await meterReadingRepository.GetAllMeterReadingsAsync();
                            allMeterReadingsAsync = allMeterReadingsList.ToObservableCollection();

                            await MainThread.InvokeOnMainThreadAsync(ProcessAggregations);
                        }
                    }
                }, cancellationTokenSource.Token);
            }
        }, null, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));
    }

    public void StopBackgroundTask()
    {
        cancellationTokenSource?.Cancel();
        backgroundTimer?.Dispose();
        cancellationTokenSource?.Dispose();
    }

    private void ProcessAggregations()
    {
        // Aggregate by hour
        hourlyAggregated = allMeterReadingsAsync
            .GroupBy(r =>
                new DateTime(r.PeriodStart.Year, r.PeriodStart.Month, r.PeriodStart.Day, r.PeriodStart.Hour, 0, 0))
            .Select(g => new PeaMeterReading(
                g.Key,
                g.Sum(r => r.RateA),
                g.Sum(r => r.RateB),
                g.Sum(r => r.RateC),
                60
            ))
            .OrderBy(r => r.PeriodStart)
            .ToObservableCollection();
        
        WeakReferenceMessenger.Default.Send(new HourlyAggregationCompletedMessage(hourlyAggregated));

        // Aggregate by day
        dailyAggregated = allMeterReadingsAsync
            .GroupBy(r => r.PeriodStart.Date)
            .Select(g => new PeaMeterReading(
                g.Key,
                g.Sum(r => r.RateA),
                g.Sum(r => r.RateB),
                g.Sum(r => r.RateC),
                60 * 24
            ))
            .OrderBy(r => r.PeriodStart)
            .ToObservableCollection();
        
        WeakReferenceMessenger.Default.Send(new DailyAggregationCompletedMessage(dailyAggregated));

        // Aggregate by week
        weeklyAggregated = allMeterReadingsAsync
            .GroupBy(r =>
            {
                var weekStart = r.PeriodStart.Date.AddDays(-(int)r.PeriodStart.DayOfWeek);
                return weekStart;
            })
            .Select(g => new PeaMeterReading(
                g.Key,
                g.Sum(r => r.RateA),
                g.Sum(r => r.RateB),
                g.Sum(r => r.RateC),
                60 * 24 * 7
            ))
            .OrderBy(r => r.PeriodStart)
            .ToObservableCollection();
        
        WeakReferenceMessenger.Default.Send(new WeeklyAggregationCompletedMessage(weeklyAggregated));
    }

    
    
    public ObservableCollection<PeaMeterReading> FetchAverageQuarterlyReadingsForPeriodAsync(DateTime startDate, DateTime endDate)
    {
        var readings = GetQuarterlyAggregated()
            .Where(m => m.PeriodStart >= startDate && m.PeriodStart < endDate)
            .ToList();

        if (readings.Count == 0)
        {
            return new ObservableCollection<PeaMeterReading>();
        }

        // Group by time of day (hour and minute) and calculate average for each 15-minute slot
        var averageReadings = readings
            .GroupBy(r => new { r.PeriodStart.Hour, r.PeriodStart.Minute })
            .Select(g => new PeaMeterReading(
                new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, g.Key.Hour, g.Key.Minute,
                    0),
                g.Average(r => r.RateA),
                g.Average(r => r.RateB),
                g.Average(r => r.RateC)
            ))
            .OrderBy(r => r.PeriodStart)
            .ToList();

        return averageReadings.ToObservableCollection();
    }

    public ObservableCollection<PeaMeterReading> FetchAverageHourlyReadingsForPeriodAsync(DateTime startDate, DateTime endDate)
    {
        var readings = GetHourlyAggregated()
            .Where(m => m.PeriodStart >= startDate && m.PeriodStart < endDate)
            .ToList();

        if (readings.Count == 0)
        {
            return new ObservableCollection<PeaMeterReading>();
        }

        // Group by time of day (hour and minute) and calculate average for each 15-minute slot
        var averageReadings = readings
            .GroupBy(r => new { r.PeriodStart.Hour })
            .Select(g => new PeaMeterReading(
                new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, g.Key.Hour, 0, 0),
                g.Average(r => r.RateA),
                g.Average(r => r.RateB),
                g.Average(r => r.RateC)
            ))
            .OrderBy(r => r.PeriodStart)
            .ToList();

        return averageReadings.ToObservableCollection();
    }

    public ObservableCollection<PeaMeterReading> FetchDailyAggregatedForPeriodAsync(DateTime startDate, DateTime endDate)
    {
        var readings = GetDailyAggregated()
            .Where(m => m.PeriodStart >= startDate && m.PeriodStart < endDate)
            .ToList();

        return readings.ToObservableCollection();
    }

    public ObservableCollection<PeaMeterReading> GetQuarterlyAggregated() => allMeterReadingsAsync;
    public ObservableCollection<PeaMeterReading> GetHourlyAggregated() => hourlyAggregated;
    public ObservableCollection<PeaMeterReading> GetDailyAggregated() => dailyAggregated;
    public ObservableCollection<PeaMeterReading> GetWeeklyAggregated() => weeklyAggregated;
    public ObservableCollection<PeaMeterReading> GetCurrentDayMeterReadings() => dailyReadings;
}

public record HourlyAggregationCompletedMessage(ObservableCollection<PeaMeterReading> Data);

public record DailyAggregationCompletedMessage(ObservableCollection<PeaMeterReading> Data);

public record WeeklyAggregationCompletedMessage(ObservableCollection<PeaMeterReading> Data);

public record AllAggregationsCompletedMessage();