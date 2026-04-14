using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Infrastructure;
using Pea.Infrastructure.Extensions;
using Pea.Infrastructure.Helpers;
using Pea.Infrastructure.Models;
using Pea.Infrastructure.Models.MeterData;
using Pea.Meter.Extension;
using Pea.Meter.Models;
using Pea.Meter.Services;
using Pea.Meter.ViewModel.Interface;

namespace Pea.Meter.ViewModel;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0034:Direct field reference to [ObservableProperty] backing field")]
public partial class SolarSystemSizingViewModel : ObservableObject, ICanExecuteViewModel
{
    private readonly ILogger<SolarSystemSizingViewModel> logger;
    private readonly StorageService storageService;

    private DateTime lastPopulateChartData = DateTime.MinValue;
    
    [ObservableProperty] private ObservableCollection<PvMonthlyAggregatedModel> pvCalculatedPerMonth = [];
    [ObservableProperty] private ObservableCollection<MeterDataManagerMonth> costCompareMonthList = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataMonthSummary = [];
    [ObservableProperty] private ObservableCollection<MeterDataManagerMonth> energyProducedMonthlySummary = [];
    [ObservableProperty] private string yearlyConsumptionText;
    [ObservableProperty] private string dailyUsageAveragePeekKwText;
    [ObservableProperty] private string averageKwUsedBetween08To17MonthlyText;
    [ObservableProperty] private string batterySizeNeededText;
    [ObservableProperty] private string solarSizeNeededText;
    [ObservableProperty] private decimal yearlyConsumption;
    [ObservableProperty] private decimal dailyUsageAveragePeekKw;
    [ObservableProperty] private decimal averageKwUsedBetween08To17Monthly;
    [ObservableProperty] private decimal consumption6HighestMonth;
    [ObservableProperty] private decimal solarSizeNeeded;
    [ObservableProperty] private decimal batterySizeNeeded;
    [ObservableProperty] private decimal batterySize = 10;
    [ObservableProperty] private decimal tilt = 5;
    [ObservableProperty] private decimal solarArraySize = 21;
    [ObservableProperty] private int selectedDirection = 180;
    private bool canExecute;
    private decimal oldBatterySize;
    private decimal oldTilt;
    private decimal oldArraySize;
    private int oldDirection;
    private decimal averageUsedBetween08To17;

    public SolarSystemSizingViewModel(ILogger<SolarSystemSizingViewModel> logger, StorageService storageService)
    {
        this.logger = logger;
        this.storageService = storageService;

        YearlyConsumptionText = "0 W";

        CreateLoggedInSubscription();
        CreateNewDaySubscription();
        CreateAllDataImportedSubscription();
        CreateAllAggregationsCompletedSubscription();
    }

    private void CreateAllAggregationsCompletedSubscription()
    {
        WeakReferenceMessenger.Default.Register<AllAggregationsCompletedMessage>(this,
            (r, m) =>
            {
                try
                {
                    PopulateChartData();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error in {Method}: {Message}",
                        nameof(CreateAllAggregationsCompletedSubscription), e.Message);
                }
            });
    }

    private void CreateAllDataImportedSubscription()
    {
        WeakReferenceMessenger.Default.Register<AllImportedDataCompletedMessage>(this,
            (r, m) =>
            {
                try
                {
                    PopulateChartData();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateAllDataImportedSubscription),
                        e.Message);
                }
            });
    }

    private void CreateLoggedInSubscription()
    {
        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this,
            (r, m) =>
            {
                try
                {
                    PopulateChartData();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateLoggedInSubscription), e.Message);
                }
            });
    }

    private void CreateNewDaySubscription()
    {
        WeakReferenceMessenger.Default.Register<DateChangedMessage>(this,
            (r, m) =>
            {
                try
                {
                    PopulateChartData();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateNewDaySubscription), e.Message);
                }
            });
    }

    private void PopulateChartData()
    {
        if (!canExecute)
        {
            return;
        }

        if (DateTime.Now - lastPopulateChartData < TimeSpan.FromSeconds(5))
        {
            return;
        }

        lastPopulateChartData = DateTime.Now;

        _ = Task.Run(PopulateChartDataInternal);
    }

    partial void OnSolarArraySizeChanged(decimal value)
    {
        PopulateChartDataInternal();
    }

    partial void OnSelectedDirectionChanged(int value)
    {
        PopulateChartDataInternal();
    }

    partial void OnTiltChanged(decimal value)
    {
        PopulateChartDataInternal();
    }

    partial void OnBatterySizeChanged(decimal value)
    {
        PopulateChartDataInternal();
    }

    bool isCalculationInProgress;

    private ObservableCollection<PvMonthlyAggregatedModel>? CalculateDisplayData()
    {
        if (isCalculationInProgress)
        {
            logger.LogWarning("Calculation is already in progress");
            return null;
        }

        try
        {
            isCalculationInProgress = true;

            if (tilt == oldTilt && solarArraySize == oldArraySize && batterySize == oldBatterySize &&
                selectedDirection == oldDirection)
            {
                logger.LogInformation("No changes in parameters (Tilt, Solar Array Size, Battery Size or Direction)");
                return null;
            }

            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var startDate = endDate.AddMonths(-12).Date;

            logger.LogInformation($"Calculating display data for period from {startDate} to {endDate}");

            var orderedHours = storageService.HourlyAggregated
                .Where(period => period.PeriodStart >= startDate && period.PeriodStart < endDate)
                .OrderBy(o => o.PeriodStart.Month)
                .ToList();

            var monthlyAggregated = storageService.MonthlyAggregated
                .Where(period => period.PeriodStart >= startDate && period.PeriodStart < endDate)
                .OrderBy(o => o.PeriodStart.Month)
                .ToList();

            averageUsedBetween08To17 = orderedHours
                .Where(period => period.PeriodStart.Hour is >= 8 and < 17)
                .GroupBy(kvp => new DateOnly(kvp.PeriodStart.Year, kvp.PeriodStart.Month, kvp.PeriodStart.Day))
                .Select(a => a.Sum(r => r.Total))
                .DefaultIfEmpty(0)
                .Average();

            var pvCalculatedPerHour = orderedHours
                .Select(s =>
                {
                    var calculatedKw = PvCalculatorService.CalculateKw(
                        s.PeriodStart, (double)SolarArraySize, (double)Tilt, SelectedDirection);

                    return new
                    {
                        PeriodStart = new DateTime(
                            s.PeriodStart.Year, s.PeriodStart.Month, s.PeriodStart.Day,
                            s.PeriodStart.Hour, 0, 0),
                        CalculatedKw = (decimal)calculatedKw
                    };
                })
                .ToDictionary(s => s.PeriodStart, s => s.CalculatedKw);

            // Group into daily aggregates
            var pvCalculatedPerDay = pvCalculatedPerHour
                .GroupBy(kvp => new DateOnly(kvp.Key.Year, kvp.Key.Month, kvp.Key.Day))
                .Select(g =>
                {
                    var periodStart = g.Key;

                    return new PvDailyAggregatedModel
                    {
                        PeriodStart = periodStart,
                        Readings = g.ToDictionary(x => x.Key, x => x.Value), // all hourly readings for that day

                        PeakTotal = g.Where(w => w.Key.Hour is >= 9 and < 22 && !w.Key.IsWeekendOrHoliday())
                            .Sum(w => w.Value),

                        OffPeakTotal = periodStart.IsWeekendOrHoliday()
                            ? g.Sum(x => x.Value)
                            : g.Where(w => w.Key.Hour is < 9 or >= 22).Sum(w => w.Value),

                        DayTotal = g.Sum(x => x.Value)
                    };
                })
                .ToDictionary(x => x.PeriodStart, x => x);

            var result = pvCalculatedPerDay
                .GroupBy(x => x.Key.Month)
                .Select(g =>
                {
                    var periodStart = new DateOnly(2020, g.Key, 1);
                    var monthlyPeaReadings = monthlyAggregated
                                                 .SingleOrDefault(s => s.PeriodStart.Month == g.Key) ??
                                             new PeaMeterReading(periodStart.ToDateTime(TimeOnly.MinValue), []);

                    var dailyCalculatedKw = g.Sum(s => s.Value.DayTotal * 1000);

                    var batteryKw = DateTime.DaysInMonth(periodStart.Year, periodStart.Month) * BatterySize * 1000;
                    var unusedBatteryKw = 0.0m;
                    
                    if (batteryKw > dailyCalculatedKw)
                    {
                        unusedBatteryKw = batteryKw - dailyCalculatedKw;
                        batteryKw = dailyCalculatedKw;
                        dailyCalculatedKw = 0;
                        
                    }
                    
                    var dailyCalculateExcludedBatteryKw = MathHelpers.ClampToZero(dailyCalculatedKw - batteryKw);

                    return new PvMonthlyAggregatedModel
                    {
                        PeriodStart = periodStart.ToDateTime(TimeOnly.MinValue),

                        DailyCalculatedKw = dailyCalculatedKw,
                        BatteryKw = batteryKw,
                        UnusedBatteryKw = unusedBatteryKw,
                        DailyCalculateExcludedBatteryKw = dailyCalculateExcludedBatteryKw,

                        OffPeakUsedKw = monthlyPeaReadings.OffPeek * 1000,
                        PeakUsedKw = monthlyPeaReadings.Peek * 1000,
                        TotalUsedKw = monthlyPeaReadings.Total * 1000
                    };
                })
                .ToObservableCollection();

            oldTilt = tilt;
            oldArraySize = solarArraySize;
            oldDirection = selectedDirection;
            oldBatterySize = batterySize;

            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in {Method}: {Message}", nameof(PopulateChartData), e.Message);
            return null;
        }
        finally
        {
            isCalculationInProgress = false;
            logger.LogInformation("Calculation completed");
        }
    }

    private void PopulateChartDataInternal()
    {
        try
        {
            var newPvData = CalculateDisplayData();

            if (newPvData == null)
                return;

            List<decimal> listSolarArraySizes =
                [2, 3, 4, 5, 10, 15, 20, 25, 30, 40, 50, 75, 100, 150, 200, 300, 400, 500, 1000];

            var monthly = newPvData;
            var yearlyConsumptionPeekKw = monthly.Sum(s => s.PeakUsedKw);
            var yearlyConsumptionOffPeekKw = monthly.Sum(s => s.OffPeakUsedKw);
            var yearlyConsumptionTotalKw = yearlyConsumptionPeekKw + yearlyConsumptionOffPeekKw;
            var averageUsedBetween08To17Monthly = averageUsedBetween08To17 * 1000;
            var dailyConsumptionAveragePeekKw = yearlyConsumptionPeekKw / 365;

            var solarSize = listSolarArraySizes.ClosestGreater(dailyConsumptionAveragePeekKw / 4.0m);

            MainThread.InvokeOnMainThreadAsync(() =>
            {
                    PvCalculatedPerMonth = newPvData;

                AverageKwUsedBetween08To17Monthly = averageUsedBetween08To17Monthly;
                AverageKwUsedBetween08To17MonthlyText = WattFormatter.Format(averageUsedBetween08To17Monthly);
                YearlyConsumptionText = WattFormatter.Format(yearlyConsumptionTotalKw);
                YearlyConsumption = yearlyConsumptionTotalKw;
                DailyUsageAveragePeekKwText = WattFormatter.Format(dailyConsumptionAveragePeekKw);
                // BatterySizeNeeded = (dailyConsumptionAveragePeekKw - averageUsedBetween08To17Monthly)
                //     .RoundUpToNearestFive();
                BatterySizeNeededText = WattFormatter.Format(BatterySize * 1000);
                SolarSizeNeededText = WattFormatter.Format(SolarArraySize * 1000);
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in {Method}: {Message}", nameof(PopulateChartData), e.Message);
        }
    }


    public void CanExecute(bool isVisible)
    {
        canExecute = isVisible;

        if (canExecute)
        {
            lastPopulateChartData = DateTime.MinValue;
            PopulateChartData();
        }
    }
}