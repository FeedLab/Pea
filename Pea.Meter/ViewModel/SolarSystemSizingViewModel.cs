using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Infrastructure.Models;
using Pea.Infrastructure.Models.MeterData;
using Pea.Meter.Extension;
using Pea.Meter.Models;
using Pea.Meter.Services;
using Pea.Meter.ViewModel.Interface;

namespace Pea.Meter.ViewModel;

public partial class SolarSystemSizingViewModel : ObservableObject, ICanExecuteViewModel
{
    private readonly ILogger<SolarSystemSizingViewModel> logger;
    private readonly StorageService storageService;

    private DateTime lastPopulateChartData = DateTime.MinValue;

    [ObservableProperty] private ObservableCollection<MeterDataManagerMonth> costCompareMonthList = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataMonthSummary = [];
    [ObservableProperty] private ObservableCollection<MeterDataManagerMonth> energyProducedMonthlySummary = [];
    [ObservableProperty] private decimal yearlyConsumption;
    [ObservableProperty] private decimal dailyUsageAveragePeekKw;
    [ObservableProperty] private decimal averageKwUsedBetween08To17Monthly;
    [ObservableProperty] private decimal consumption6HighestMonth;
    [ObservableProperty] private decimal solarSizeNeeded;
    [ObservableProperty] private decimal batterySizeNeeded;
    private bool canExecute;

    public SolarSystemSizingViewModel(ILogger<SolarSystemSizingViewModel> logger, StorageService storageService)
    {
        this.logger = logger;
        this.storageService = storageService;

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

    private void PopulateChartDataInternal()
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

            var meterDataReadings = storageService.AllMeterReadingsAsync
                .Where(period => period.PeriodStart >= startDate && period.PeriodStart < endDate)
                .Select(s =>
                    new MeterDataReading(s.PeriodStart, s.RateA, s.RateB, s.RateC))
                .ToList();

            var meterDataManager = new MeterDataManager(meterDataReadings, 3.9086m, 5.1135m, 2.6037m);

            var last12FullOrderedMonths = meterDataManager
                .GetMonthsInRange(startDate.Year, startDate.Month, endDate.Year, endDate.Month)
                .OrderBy(o => o.Date.Month)
                .ToList();

            if (!last12FullOrderedMonths.Any())
            {
                return;
            }

            var numberOfDaysInPeriod = last12FullOrderedMonths.Sum(s => s.GetBuckets().Count);

            var yearlyConsumptionPeekKw = last12FullOrderedMonths.Sum(s => s.MeterDataUsageInKw.PeekUsage);
            var yearlyConsumptionOffPeekKw = last12FullOrderedMonths.Sum(s => s.MeterDataUsageInKw.OffPeekUsage);
            var yearlyConsumptionHoliday = last12FullOrderedMonths.Sum(s => s.MeterDataUsageInKw.Holiday);
            var yearlyConsumptionTotalKw =
                yearlyConsumptionPeekKw + yearlyConsumptionOffPeekKw + yearlyConsumptionHoliday;
            var averageUsedBetween08To17Monthly =
                last12FullOrderedMonths.Average(s => s.AverageKwUsedBetween08To17Monthly);
            var dailyConsumptionAveragePeekKw = yearlyConsumptionPeekKw / numberOfDaysInPeriod;

            var solarArraySize = listSolarArraySizes.ClosestGreater(dailyConsumptionAveragePeekKw / 4.0m);
            meterDataManager.CalculateSolarProduction(solarArraySize, BatterySizeNeeded, 180, 3);
            
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                AverageKwUsedBetween08To17Monthly = averageUsedBetween08To17Monthly;

                YearlyConsumption = yearlyConsumptionTotalKw;
                DailyUsageAveragePeekKw = dailyConsumptionAveragePeekKw;
                BatterySizeNeeded = (dailyConsumptionAveragePeekKw - averageUsedBetween08To17Monthly)
                    .RoundUpToNearestFive();
                
                SolarSizeNeeded = solarArraySize;
                EnergyProducedMonthlySummary = last12FullOrderedMonths.ToObservableCollection();
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in {Method}: {Message}", nameof(PopulateChartData), e.Message);
        }
    }

    private decimal GetBatterySize()
    {
        return 10;
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