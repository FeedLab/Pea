using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;
using Pea.Meter.Extension;

namespace Pea.Meter.Services;

[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class StorageService : ObservableObject
{
    [ObservableProperty] private ObservableCollection<PeaMeterReading> allMeterReadingsAsync = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> hourlyAggregated = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> dailyAggregated = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> weeklyAggregated = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> dailyReadings = [];
    private readonly ILogger logger;
    private readonly PeaDbContextFactory dbContextFactory;
    private readonly PeaAdapter peaAdapter;
    private CancellationTokenSource? cancellationTokenSource;
    private Timer? backgroundTimer;
    private DateTime currentDay = DateTime.MinValue; // MinValue - Will trigger a new day on the first run
    private Timer backgroundTimerNewDay;
    private CancellationTokenSource newDayCancellationTokenSource;

    public bool IsAuthenticated { get; set; }

    public StorageService(ILogger<StorageService> logger, PeaDbContextFactory dbContextFactory, PeaAdapter peaAdapter)
    {
        this.logger = logger;
        this.dbContextFactory = dbContextFactory;
        this.peaAdapter = peaAdapter;
    }


    public async Task Init()
    {
        await CheckForNewDayBackgroundTask();

        StartBackgroundTask();
    }

    private Task CheckForNewDayBackgroundTask()
    {
        var context = dbContextFactory.CreateDbContext();
        var meterReadingRepository = new MeterReadingRepository(context);

        newDayCancellationTokenSource = new CancellationTokenSource();

        backgroundTimerNewDay = new Timer(async void (_) =>
        {
            try
            {
                if (DateTime.Now.Date > currentDay)
                {
                    var oldDate = currentDay;
                    var newDate = DateTime.Now.Date;
                    currentDay = newDate;

                    StopBackgroundTask();

                    var readingsFromDb = await meterReadingRepository.GetAllMeterReadingsAsync();
                    AllMeterReadingsAsync = readingsFromDb.ToObservableCollection();

                    await MainThread.InvokeOnMainThreadAsync(() => { ProcessAggregations(); });

                    await InitNewDay(oldDate, newDate);

                    StartBackgroundTask();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in background task ({backgroundTaskName}): {Message}",
                    nameof(CheckForNewDayBackgroundTask), e.Message);
            }
        }, null, TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(1));

        return Task.CompletedTask;
    }

    private async Task InitNewDay(DateTime oldDate, DateTime newDate)
    {
        logger.LogInformation("InitNewDay: {OldDate}, {NewDate}",oldDate, newDate);
        var context = dbContextFactory.CreateDbContext();
        var meterReadingRepository = new MeterReadingRepository(context);

        var readingsFromDb = await meterReadingRepository.GetAllMeterReadingsAsync();
        AllMeterReadingsAsync.AddRange(readingsFromDb);

        await FetchAndFilterDailyReadings();

        WeakReferenceMessenger.Default.Send(new DateChangedMessage(oldDate, newDate));
    }

    private async Task FetchAndFilterDailyReadings()
    {
        logger.LogInformation("Fetching daily readings from Pea Adapter");

        var readingsFromPea = await peaAdapter.ShowDailyReadings(DateTime.Today);
        var newReadings = GetDiffBetweenDailyAndAllReadings(readingsFromPea.ToList());
        var newReadingsFiltered = newReadings.Where(r => r.Total > 0).ToList();

        logger.LogInformation($"Found {newReadingsFiltered.Count} new readings");

        if (newReadingsFiltered.Count > 0)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                DailyReadings.AddRange(newReadingsFiltered);
                AllMeterReadingsAsync.AddRange(newReadingsFiltered);
            });
        }
    }

    private void StartBackgroundTask()
    {
        cancellationTokenSource = new CancellationTokenSource();

        // Run aggregations every 15 minutes
        backgroundTimer = new Timer(async void (_) =>
        {
            try
            {
                if (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Run(async () => { await FetchAndFilterDailyReadings(); }, cancellationTokenSource.Token);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in background task: {Message}", e.Message);
            }
        }, null, TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(15));
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
        WeakReferenceMessenger.Default.Send(new AllAggregationsCompletedMessage());
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