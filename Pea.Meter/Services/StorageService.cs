using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure;
using Pea.Infrastructure.Models;
using Pea.Meter.Extension;
using Pea.Meter.Models;

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
    [ObservableProperty] private ObservableCollection<PeaMeterReading> monthlyAggregated = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> dailyPeriodReadings = [];
    
    [ObservableProperty] private ConfigurationTariffModel configurationTariffModel;
    [ObservableProperty] private ConfigurationLanguageModel configurationLanguageModel;

    private readonly ILogger logger;
    private readonly PeaDbContextFactory dbContextFactory;
    private readonly PeaAdapter peaAdapter;
    private CancellationTokenSource? cancellationTokenSource;
    private Timer? backgroundTimer;
    private DateTime currentDay = DateTime.MinValue; // MinValue - Will trigger a new day on the first run
    private Timer? backgroundTimerNewDay;
    private CancellationTokenSource newDayCancellationTokenSource;
    public bool IsAuthenticated { get; set; }

    public StorageService(ILogger<StorageService> logger, PeaDbContextFactory dbContextFactory, PeaAdapter peaAdapter)
    {
        this.logger = logger;
        this.dbContextFactory = dbContextFactory;
        this.peaAdapter = peaAdapter;
        
        ConfigurationTariffModel = ConfigurationTariffModel.Load();
        ConfigurationLanguageModel = ConfigurationLanguageModel.Load();

        newDayCancellationTokenSource = new CancellationTokenSource();
        
        WeakReferenceMessenger.Default.Register<DataImportedMessage>(this, async void (r, m) =>
        {
            try
            {
                var context = dbContextFactory.CreateDbContext();
                var meterReadingRepository = new MeterReadingRepository(context);

                var readingsFromDb = await meterReadingRepository.GetAllMeterReadingsAsync();
                AllMeterReadingsAsync = readingsFromDb.ToObservableCollection();
                ProcessAggregations();

                await MainThread.InvokeOnMainThreadAsync(() => { ProcessAggregations(); });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in {Method}: {Message}", nameof(StorageService), e.Message);
            }
        });
    }


    public async Task Init()
    {
        await CheckForNewDayBackgroundTask();

        //StartBackgroundTask();
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

                    try
                    {
                        AllMeterReadingsAsync = readingsFromDb.ToObservableCollection();
                        ProcessAggregations();
                        WeakReferenceMessenger.Default.Send(new AllAggregationsCompletedMessage());
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(CheckForNewDayBackgroundTask),
                            e.Message);
                    }

                    await InitNewDay(oldDate, newDate);

                    StartBackgroundTask();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in background task ({backgroundTaskName}): {Message}",
                    nameof(CheckForNewDayBackgroundTask), e.Message);
            }
        }, null, TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(10));

        return Task.CompletedTask;
    }

    private async Task InitNewDay(DateTime oldDate, DateTime newDate)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                logger.LogInformation("InitNewDay: {OldDate}, {NewDate}", oldDate, newDate);


                var context = dbContextFactory.CreateDbContext();
                var meterReadingRepository = new MeterReadingRepository(context);

                var readingsFromDb = await meterReadingRepository.GetAllMeterReadingsAsync();

                AllMeterReadingsAsync.Clear();
                AllMeterReadingsAsync.AddRange(readingsFromDb);

                var readingsFromPea = await peaAdapter.ShowDailyReadings(newDate);
                var newReadings = GetDiffBetweenDailyAndAllReadings(readingsFromPea.ToList());
                var newReadingsFiltered = newReadings.Where(r => r.Total > 0).ToList();

                DailyPeriodReadings.Clear();
                DailyPeriodReadings.AddRange(newReadingsFiltered);
                AllMeterReadingsAsync.AddRange(newReadingsFiltered);

                ProcessAggregations();

                WeakReferenceMessenger.Default.Send(new DateChangedMessage(oldDate, newDate));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in {Method}: {Message}", nameof(InitNewDay),
                    e.Message);
            }
        });
    }

    private async Task FetchAndFilterDailyReadings()
    {
        logger.LogInformation("Fetching daily readings from Pea Adapter");

        await MainThread.InvokeOnMainThreadAsync(async () => { await ProcessNewPeriodReadings(); });
    }

    private async Task ProcessNewPeriodReadings()
    {
        var readingsFromPea = await peaAdapter.ShowDailyReadings(DateTime.Today);
        var newReadings = GetDiffBetweenDailyAndAllReadings(readingsFromPea.ToList());
        var newReadingsFiltered = newReadings.Where(r => r.Total > 0).ToList();

        logger.LogInformation($"Found {newReadingsFiltered.Count} new readings");

        if (newReadingsFiltered.Count > 0)
        {
            DailyPeriodReadings.AddRange(newReadingsFiltered);
            AllMeterReadingsAsync.AddRange(newReadingsFiltered);
            ProcessAggregations();
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
                    }, cancellationTokenSource.Token);
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
        MonthlyAggregated.Clear();

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

        // Aggregate by month
        var monthlyList = AllMeterReadingsAsync
            .GroupBy(r => new { Year = r.PeriodStart.Year, Month = r.PeriodStart.Month })
            .Select(g => new PeaMeterReading(
                // Use the first day of the month as the PeriodStart
                new DateTime(g.Key.Year, g.Key.Month, 1),
                g.Sum(r => r.RateA),
                g.Sum(r => r.RateB),
                g.Sum(r => r.RateC),
                // Minutes in the month (approximate: 60 * 24 * days)
                60 * 24 * DateTime.DaysInMonth(g.Key.Year, g.Key.Month)
            ))
            .OrderBy(r => r.PeriodStart)
            .ToObservableCollection();

        MonthlyAggregated.AddRange(monthlyList);

        WeakReferenceMessenger.Default.Send(new MonthlyAggregationCompletedMessage(MonthlyAggregated));
    }

    /// <summary>
    /// Creates a diff list between all database readings and current day's live readings.
    /// Returns readings that exist in dailyReadings but not yet in the database (allMeterReadingsAsync).
    /// </summary>
    private List<PeaMeterReading> GetDiffBetweenDailyAndAllReadings(List<PeaMeterReading> readings)
    {
        // Get readings from dailyReadings that don't exist in allMeterReadingsAsync
        // Compare by PeriodStart as unique identifier
        var allReadingTimes = new HashSet<DateTime>(AllMeterReadingsAsync.Select(r => r.PeriodStart));

        var newReadings = readings
            .Where(dr => !allReadingTimes.Contains(dr.PeriodStart))
            .ToList();

        return newReadings;
    }

    public List<MeterReadingMonthlySummary> GetMeterReadingMonthlySummaries(List<PeaMeterReading> allPeriods,
        decimal solarArraySize = 1, decimal batterySize = 0)
    {
        var listOfAllCostCompares = allPeriods.Select(s => new CostCompare(s, 3.9086m, 5.1135m, 2.6037m)).ToList();

        var costCompareMonthList = listOfAllCostCompares
            .GroupBy(costCompare => new
                { costCompare.MeterReading.PeriodStart.Year, costCompare.MeterReading.PeriodStart.Month })
            .Select(costCompares =>
            {
                var daysInMonthList = costCompares.Select(s => s.MeterReading.PeriodStart.Date).Distinct().ToList();

                var peekRecords = costCompares.Where(w => w.IsPeekPeriod).ToList();
                var offPeekRecords = costCompares.Where(w => !w.IsPeekPeriod).ToList();
                var totalKw = peekRecords.Sum(s => s.KwUsed) + offPeekRecords.Sum(s => s.KwUsed);

                var peekSum = peekRecords.Sum(s => s.KwUsed);
                var offPeekSum = offPeekRecords.Sum(s => s.KwUsed);

                var costSummaryFlatRate = costCompares.Sum(s => s.FlatRateCost);
                var costSummaryTouPeek = costCompares.Sum(s => s.KwCostAtPeek);
                var costSummaryTouOffPeek = costCompares.Sum(s => s.KwCostAtOffPeek);
                var costTouTotal = costSummaryTouPeek + costSummaryTouOffPeek;


                var sumKwUsedBetween08To17 = costCompares
                    .Where(w => w is { IsPeekPeriod: true, MeterReading.PeriodStart.Hour: >= 8 and < 17 })
                    .Sum(s => s.KwUsed);

                // distinct days in this month for normalization
                var daysInMonth = DateTime.DaysInMonth(costCompares.Key.Year, costCompares.Key.Month);
                //var peekDays = peekRecords.Select(s => s.MeterReading.PeriodStart.Date).Distinct().Count();
                //var offPeekDays = offPeekRecords.Select(s => s.MeterReading.PeriodStart.Date).Distinct().Count();

                var dateTime = new DateTime(costCompares.Key.Year, costCompares.Key.Month, 1);

                var calculateKwGeneratedMonthly = (decimal)PvCalculatorService
                    .CalculateKwMonthly(dateTime, (double)solarArraySize, 3, 180);

                var calculateKwGeneratedDaily = calculateKwGeneratedMonthly / daysInMonth;

                var firstCostCompare = costCompares.First();

                var offPeekPrice = firstCostCompare.OffPeekPrice;
                var peekPrice = firstCostCompare.PeekPrice;
                var flatRatePrice = firstCostCompare.FlatRatePrice;

                var peekTouSaving = 0m;
                var offPeekTouSaving = 0m;
                var flatRateSaving = 0m;
                foreach (var day in daysInMonthList)
                {
                    if (day.DayOfWeek == DayOfWeek.Saturday |
                        day.DayOfWeek == DayOfWeek.Sunday)
                    {
                        offPeekTouSaving += calculateKwGeneratedDaily * offPeekPrice;
                    }
                    else
                    {
                        peekTouSaving += calculateKwGeneratedDaily * peekPrice;
                    }

                    flatRateSaving += calculateKwGeneratedDaily * flatRatePrice;
                }

                if (peekTouSaving > costSummaryTouPeek)
                {
                    var rest = peekTouSaving - costSummaryTouPeek;

                    peekTouSaving = costSummaryTouPeek;
                    offPeekTouSaving += rest;
                }

                return new MeterReadingMonthlySummary
                {
                    Date = dateTime,
                    KwUsedAtPeek = peekSum,
                    KwUsedAtOffPeek = offPeekSum,
                    KwUsedTotal = totalKw,

                    CostSummaryFlatRate = costSummaryFlatRate,
                    CostSummaryTouPeek = costSummaryTouPeek,
                    CostSummaryTouOffPeek = costSummaryTouOffPeek,
                    CostSummaryTouTotal = costTouTotal,

                    PeekTouDiscounted = costSummaryTouPeek - peekTouSaving,
                    OffPeekTouDiscounted = costSummaryTouOffPeek - offPeekTouSaving,
                    FlatRateDiscounted = costSummaryFlatRate - flatRateSaving,

                    PeekTouSaving = peekTouSaving,
                    OffPeekTouSaving = offPeekTouSaving,
                    FlatRateSaving = flatRateSaving,

                    // average per record
                    AverageKwUsedAtPeekPerRecord = peekRecords.Any() ? peekRecords.Average(s => s.KwUsed) : 0,
                    AverageKwUsedAtOffPeekPerRecord = offPeekRecords.Any() ? offPeekRecords.Average(s => s.KwUsed) : 0,
                    AverageKwUsedBetween08To17Monthly = sumKwUsedBetween08To17 / daysInMonth,

                    // average per day
                    AverageKwUsedAtPeekPerDay = daysInMonth > 0 ? peekSum / daysInMonth : 0,
                    AverageKwUsedAtOffPeekPerDay = daysInMonth > 0 ? offPeekSum / daysInMonth : 0,

                    CalculateProducedSolarKwMonthly = calculateKwGeneratedMonthly,
                    CalculateProducedSolarKwDaily = calculateKwGeneratedMonthly / daysInMonth,
                    BatteryKwProducedMonthly = batterySize * daysInMonth,

                    SolarArraySize = solarArraySize,
                    BatterySize = batterySize
                };
            })
            .ToList();
        return costCompareMonthList;
    }
}

public record HourlyAggregationCompletedMessage(ObservableCollection<PeaMeterReading> Data);

public record DailyAggregationCompletedMessage(ObservableCollection<PeaMeterReading> Data);

public record WeeklyAggregationCompletedMessage(ObservableCollection<PeaMeterReading> Data);

public record MonthlyAggregationCompletedMessage(ObservableCollection<PeaMeterReading> Data);

public record AllAggregationsCompletedMessage();

public record DateChangedMessage(DateTime OldDate, DateTime NewDate);