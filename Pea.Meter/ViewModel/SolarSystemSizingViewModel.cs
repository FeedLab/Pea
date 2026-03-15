using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Infrastructure.Models;
using Pea.Meter.Extension;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

public partial class SolarSystemSizingViewModel : ObservableObject
{
    private readonly ILogger<SolarSystemSizingViewModel> logger;
    private readonly StorageService storageService;

    [ObservableProperty] private ObservableCollection<MeterReadingMonthlySummary> costCompareMonthList = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataMonthSummary = [];
    [ObservableProperty] private ObservableCollection<MeterReadingMonthlySummary> energyProducedMonthlySummary = [];
    [ObservableProperty] private decimal yearlyConsumption;
    [ObservableProperty] private decimal dailyUsageAveragePeekKw;
    [ObservableProperty] private decimal averageKwUsedBetween08To17Monthly;
    [ObservableProperty] private decimal consumption6HighestMonth;
    [ObservableProperty] private decimal solarSizeNeeded;
    [ObservableProperty] private decimal batterySizeNeeded;

    public SolarSystemSizingViewModel(ILogger<SolarSystemSizingViewModel> logger, StorageService storageService)
    {
        this.logger = logger;
        this.storageService = storageService;

        CreateLoggedInSubscription();
        CreateNewDaySubscription();
        CreateDataImportedSubscription();
    }

    private void CreateDataImportedSubscription()
    {
        WeakReferenceMessenger.Default.Register<DataImportedMessage>(this,
            (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        PopulateChartData();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateNewDaySubscription), e.Message);
                    }

                    return Task.FromResult(Task.CompletedTask);
                });
            });
    }

    private void CreateLoggedInSubscription()
    {
        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this,
            (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        PopulateChartData();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateNewDaySubscription), e.Message);
                    }

                    return Task.FromResult(Task.CompletedTask);
                });
            });
    }

    private void CreateNewDaySubscription()
    {
        WeakReferenceMessenger.Default.Register<DateChangedMessage>(this,
            (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        PopulateChartData();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateNewDaySubscription), e.Message);
                    }

                    return Task.FromResult(Task.CompletedTask);
                });
            });
    }

    private void PopulateChartData()
    {
        try
        {
            List<decimal> listSolarArraySizes =
                [2, 3, 4, 5, 10, 15, 20, 25, 30, 40, 50, 75, 100, 150, 200, 300, 400, 500, 1000];
            
            if (storageService.AllMeterReadingsAsync.Count == 0)
            {
                return;
            }

            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var startDate = endDate.AddMonths(-12).Date;

            var peaMonthlyMeterReadings = storageService.MonthlyAggregated
                .Where(period => period.PeriodStart >= startDate && period.PeriodStart < endDate)
                .OrderBy(o => o.PeriodStart.Month);
            
            MeterDataMonthSummary = new ObservableCollection<PeaMeterReading>(peaMonthlyMeterReadings);

            var allPeriods = storageService.AllMeterReadingsAsync
                .Where(period => period.PeriodStart >= startDate && period.PeriodStart < endDate)
                .ToList();

            var listOfAllCostCompares = allPeriods
                .Select(s => new CostCompare(s, 3.9086m, 5.1135m, 2.6037m))
                .ToList();

            var yearlyConsumptionPeekKw = listOfAllCostCompares.Where(w => w.IsPeekPeriod).Sum(s => s.KwUsed);
            var yearlyConsumptionOffPeekKw = listOfAllCostCompares.Where(w => !w.IsPeekPeriod).Sum(s => s.KwUsed);
            var yearlyConsumptionTotalKw = yearlyConsumptionPeekKw + yearlyConsumptionOffPeekKw;
            var dailyConsumptionAverageTotalKw = yearlyConsumptionTotalKw / 365;
            var dailyConsumptionAveragePeekKw = yearlyConsumptionPeekKw / 365;
            var dailyConsumptionAverageOffPeekKw = yearlyConsumptionOffPeekKw / 365;
            var monthlyMeterReadingsList = GetMonthlyMeterReadingsList(allPeriods);

            YearlyConsumption = yearlyConsumptionTotalKw;

            var solarArraySize = listSolarArraySizes.ClosestGreater(dailyConsumptionAveragePeekKw / 4.0m);
            var batterySize = (solarArraySize / 2.0m).RoundUpToNearestFive();

            var meterReadingMonthlySummaries = storageService
                .GetMeterReadingMonthlySummaries(allPeriods, solarArraySize, batterySize)
                .Where(period => period.Date >= startDate && period.Date < endDate)
                .OrderBy(o => o.Date.Month)
                .ToList();

            DailyUsageAveragePeekKw = dailyConsumptionAveragePeekKw;
            AverageKwUsedBetween08To17Monthly = dailyConsumptionAveragePeekKw -
                                                meterReadingMonthlySummaries.Average(s =>
                                                    s.AverageKwUsedBetween08To17Monthly);
            SolarSizeNeeded = solarArraySize;
            BatterySizeNeeded = AverageKwUsedBetween08To17Monthly.RoundUpToNearestFive();

            EnergyProducedMonthlySummary = meterReadingMonthlySummaries.ToObservableCollection();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in {Method}: {Message}", nameof(PopulateChartData), e.Message);
        }
    }

    private static List<PeaMeterReading> GetMonthlyMeterReadingsList(List<PeaMeterReading> allPeriods)
    {
        var monthlyMeterReadingList = allPeriods
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
            .OrderBy(o => o.PeriodStart.Month)
            .ToList();

        return monthlyMeterReadingList;
    }
}