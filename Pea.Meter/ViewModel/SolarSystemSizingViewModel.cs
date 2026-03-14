using System.Collections.ObjectModel;
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

    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataMonthSummary = [];
    [ObservableProperty] private ObservableCollection<MeterReadingMonthlySummary> meterReadingMonthlySummary = [];
    [ObservableProperty] private decimal yearlyConsumption;
    [ObservableProperty] private decimal dailyConsumptionAverage6Month;
    [ObservableProperty] private decimal dailyPeekConsumptionAverage6Month;
    [ObservableProperty] private decimal consumption6HighestMonth;
    [ObservableProperty] private decimal solarSizeNeeded;
    [ObservableProperty] private decimal batterySizeNeeded;

    const int PeekMonths = 6;

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
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await PopulateChartData();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateNewDaySubscription), e.Message);
                    }

                    return Task.CompletedTask;
                });
            });
    }

    private void CreateLoggedInSubscription()
    {
        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this,
            (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await PopulateChartData();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateNewDaySubscription), e.Message);
                    }

                    return Task.CompletedTask;
                });
            });
    }

    private void CreateNewDaySubscription()
    {
        WeakReferenceMessenger.Default.Register<DateChangedMessage>(this,
            (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await PopulateChartData();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateNewDaySubscription), e.Message);
                    }

                    return Task.CompletedTask;
                });
            });
    }

    private async Task PopulateChartData()
    {

        try
        {
            if (storageService.AllMeterReadingsAsync.Count == 0)
            {
                return;
            }

            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var startDate = endDate.AddMonths(-12).Date;

            var allPeriods = storageService.AllMeterReadingsAsync
                .Where(period => period.PeriodStart >= startDate && period.PeriodStart < endDate)
                .ToList();

            var monthlySummaryList = allPeriods
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
        
            MeterDataMonthSummary.Clear();
            MeterDataMonthSummary.AddRange(monthlySummaryList);

            var costCompareMonthList = storageService.GetMeterReadingMonthlySummaries(allPeriods);
        
            YearlyConsumption = monthlySummaryList.Sum(s => s.Total);
        
            var consumption6HighestMonthList = costCompareMonthList
                .OrderByDescending(o => o.KwUsedTotal)
                .Take(PeekMonths)
                .ToList();

            if (consumption6HighestMonthList.Count == 0)
            {
                return;
            }

            DailyConsumptionAverage6Month = consumption6HighestMonthList
                .Sum(s => s.KwUsedTotal) / (consumption6HighestMonthList.Count * 30);

            DailyPeekConsumptionAverage6Month = consumption6HighestMonthList
                .Sum(s => s.KwUsedAtPeek) / (consumption6HighestMonthList.Count * 30);
        
            var calculateProducedSolarKwDailyAverage = consumption6HighestMonthList
                .Average(s => s.CalculateProducedSolarKwDaily);
        
            SolarSizeNeeded = DailyPeekConsumptionAverage6Month / 5;

            BatterySizeNeeded = DailyPeekConsumptionAverage6Month - (calculateProducedSolarKwDailyAverage * SolarSizeNeeded);

            var meterReadingMonthlySummaries = costCompareMonthList
                .Where(period => period.Date >= startDate && period.Date < endDate)
                .OrderBy(o => o.Date.Month)
                .ToList();
        
            foreach (var monthlyData in meterReadingMonthlySummaries)
            {
                monthlyData.KwProducedPerMonth = SolarDataService.GetMonthlySummary(SolarSizeNeeded, monthlyData.Date.Month, 1, 23);
            }
        
            MeterReadingMonthlySummary = new ObservableCollection<MeterReadingMonthlySummary>(meterReadingMonthlySummaries);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in {Method}: {Message}", nameof(PopulateChartData), e.Message);
        }
    }
}