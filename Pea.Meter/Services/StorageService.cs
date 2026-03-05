using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;
using Pea.Meter.Extension;

namespace Pea.Meter.Services;

[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class StorageService : ObservableObject
{
    [ObservableProperty] private ObservableCollection<PeaMeterReading> allMeterReadingsAsync = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> hourlyAggregated = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> dailyAggregated = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> weeklyAggregated = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> dailyReadings = [];
    private readonly PeaDbContextFactory dbContextFactory;
    private readonly PeaAdapter peaAdapter;
    private CancellationTokenSource? cancellationTokenSource;
    private Timer? backgroundTimer;
    private DateTime currentDay = DateTime.Now.Date;

    public bool IsAuthenticated { get; set; }

    public StorageService(PeaDbContextFactory dbContextFactory, PeaAdapter peaAdapter)
    {
        this.dbContextFactory = dbContextFactory;
        this.peaAdapter = peaAdapter;

        WeakReferenceMessenger.Default.Register<DateChangedMessage>(this, async (r, m) =>
        {
            await InitNewDay();
        });
    }


    public async Task Init()
    {
        await InitNewDay();

        StartBackgroundTask();
    }

    private async Task InitNewDay()
    {
        StopBackgroundTask();
        
        var context = dbContextFactory.CreateDbContext();
        var meterReadingRepository = new MeterReadingRepository(context);
        
        var readingsFromDb = await meterReadingRepository.GetAllMeterReadingsAsync();
        AllMeterReadingsAsync.AddRange(readingsFromDb);

        // Start aggregations in background without blocking
        await Task.Run(async () =>
        {
                await FetchAndFilterDailyReadings();
                ProcessAggregations();
        });

        StartBackgroundTask();
    }

    private async Task FetchAndFilterDailyReadings()
    {
        var readingsFromPea = await peaAdapter.ShowDailyReadings(DateTime.Today);

        var newReadings = GetDiffBetweenDailyAndAllReadings(readingsFromPea.ToList());

//        await MainThread.InvokeOnMainThreadAsync(() =>
//        {
            var newReadingsFiltered = newReadings.Where(r => r.Total > 0).ToList();
            
            DailyReadings.AddRange(newReadingsFiltered);
            AllMeterReadingsAsync.AddRange(newReadingsFiltered);
//        });
    }

    private void StartBackgroundTask()
    {
        cancellationTokenSource = new CancellationTokenSource();

        // Run aggregations every 15 minutes
        backgroundTimer = new Timer(async _ =>
        {
            if (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                CheckAndSendDateChangeMessage();

                await Task.Run(async () =>
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await FetchAndFilterDailyReadings();
                        ProcessAggregations();
                    });
                }, cancellationTokenSource.Token);
            }
        }, null, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));
    }

    private void CheckAndSendDateChangeMessage()
    {
        // Check if date has changed
        if (DateTime.Now.Date > currentDay)
        {
            var oldDate = currentDay;
            var newDate = DateTime.Now.Date;
            currentDay = newDate;

            WeakReferenceMessenger.Default.Send(new DateChangedMessage(oldDate, newDate));
        }
    }

    private void StopBackgroundTask()
    {
        cancellationTokenSource?.Cancel();
        backgroundTimer?.Dispose();
        cancellationTokenSource?.Dispose();
    }

    private void ProcessAggregations()
    {
        HourlyAggregated.Clear();
        DailyAggregated.Clear();
        WeeklyAggregated.Clear();
        
        // Aggregate by hour
        var hourlyList = AllMeterReadingsAsync
            .GroupBy(r =>
                new DateTime(r.PeriodStart.Year, r.PeriodStart.Month, r.PeriodStart.Day, r.PeriodStart.Hour, 0, 0))
            .Select(g => new PeaMeterReading(
                g.Key,
                g.Sum(r => r.RateA),
                g.Sum(r => r.RateB),
                g.Sum(r => r.RateC),
                60
            ))
            .OrderBy(r => r.PeriodStart);
        
        HourlyAggregated.AddRange(hourlyList);

        WeakReferenceMessenger.Default.Send(new HourlyAggregationCompletedMessage(HourlyAggregated));

        
        // Aggregate by day
        var dailyList = AllMeterReadingsAsync
            .GroupBy(r => r.PeriodStart.Date)
            .Select(g => new PeaMeterReading(
                g.Key,
                g.Sum(r => r.RateA),
                g.Sum(r => r.RateB),
                g.Sum(r => r.RateC),
                60 * 24
            ))
            .OrderBy(r => r.PeriodStart);

        DailyAggregated.AddRange(dailyList);
        
        WeakReferenceMessenger.Default.Send(new DailyAggregationCompletedMessage(DailyAggregated));

        
        // Aggregate by week
        var weeklyList = AllMeterReadingsAsync
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

        WeeklyAggregated.AddRange(weeklyList);
        
        WeakReferenceMessenger.Default.Send(new WeeklyAggregationCompletedMessage(WeeklyAggregated));
    }


    // public ObservableCollection<PeaMeterReading> FetchDailyAggregatedForPeriodAsync(DateTime startDate,
    //     DateTime endDate)
    // {
    //     var readings = DailyReadings
    //         .Where(m => m.PeriodStart >= startDate && m.PeriodStart < endDate)
    //         .ToList();
    //
    //     return new ObservableCollection<PeaMeterReading>(readings);
    // }

    // public ObservableCollection<PeaMeterReading> GetQuarterlyAggregated() => allMeterReadingsAsync;
    // public ObservableCollection<PeaMeterReading> GetHourlyAggregated() => hourlyAggregated;
    // public ObservableCollection<PeaMeterReading> GetDailyAggregated() => dailyAggregated;
    // public ObservableCollection<PeaMeterReading> GetWeeklyAggregated() => weeklyAggregated;
    // public ObservableCollection<PeaMeterReading> GetCurrentDayMeterReadings() => dailyReadings;

    /// <summary>
    /// Creates a diff list between all database readings and current day's live readings.
    /// Returns readings that exist in dailyReadings but not yet in the database (allMeterReadingsAsync).
    /// </summary>
    private List<PeaMeterReading> GetDiffBetweenDailyAndAllReadings(List<PeaMeterReading> readings)
    {
        // Get readings from dailyReadings that don't exist in allMeterReadingsAsync
        // Compare by PeriodStart as unique identifier
        var allReadingTimes = new HashSet<DateTime>(allMeterReadingsAsync.Select(r => r.PeriodStart));

        var newReadings = readings
            .Where(dr => !allReadingTimes.Contains(dr.PeriodStart))
            .ToList();

        return newReadings;
    }
}

public record HourlyAggregationCompletedMessage(ObservableCollection<PeaMeterReading> Data);

public record DailyAggregationCompletedMessage(ObservableCollection<PeaMeterReading> Data);

public record WeeklyAggregationCompletedMessage(ObservableCollection<PeaMeterReading> Data);

public record AllAggregationsCompletedMessage();

public record DateChangedMessage(DateTime OldDate, DateTime NewDate);