using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Pea.Infrastructure.Models;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0019:Invalid containing type for [ObservableProperty] field or property")]
[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class TouVsFlatRateViewModel: ObservableObject
{
    private readonly StorageService storageService;
    [ObservableProperty] private List<CostCompare> costCompares = [];
    [ObservableProperty] private decimal touTotalCost;
    [ObservableProperty] private decimal flatRateTotalCost;
    [ObservableProperty] private decimal diffInCurrency;
    [ObservableProperty] private decimal diffInPercent;
    [ObservableProperty] private bool isFlatRateVisible;
    [ObservableProperty] private bool isTouVisible;
    [ObservableProperty] private DateTime startDate;
    [ObservableProperty] private DateTime endDate;
    [ObservableProperty] private DateTime startTimePickerMaximumDate;
    [ObservableProperty] private DateTime startTimePickerMinimumDate;
    [ObservableProperty] private DateTime endTimePickerMaximumDate;
    [ObservableProperty] private DateTime endTimePickerMinimumDate;

    public TouVsFlatRateViewModel(StorageService storageService)
    {
        this.storageService = storageService;

        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, async (r, m) =>
        {
            var meterReading = storageService.DailyAggregated;

            var today = DateTime.Now;
            StartDate = today.AddYears(-1);
            EndDate = today;

            // meterReading = storageService
            //     .GetDailyAggregated()
            //     .Where(w => w.PeriodStart >= StartDate && w.PeriodStart < EndDate)
            //     .ToList();

            if (meterReading.Any())
            {
                StartTimePickerMinimumDate = meterReading.First().PeriodStart.Date;
                StartTimePickerMaximumDate = EndDate.AddDays(-1);

                if (StartDate < StartTimePickerMinimumDate)
                {
                    StartDate = StartTimePickerMinimumDate;
                }

                EndTimePickerMinimumDate = StartDate.AddDays(1);
                EndTimePickerMaximumDate = today;
                await Task.Delay(5000);
                await CalculateCostComparisons();
            }
        });

        WeakReferenceMessenger.Default.Register<DataImportedMessage>(this, async (r, m) =>
        {
            StartTimePickerMinimumDate = m.Date;

            if (StartDate < StartTimePickerMinimumDate)
            {
                StartDate = StartTimePickerMinimumDate;
            }
            
        });
    }

    public Task CalculateCostComparisons()
    {
        IsFlatRateVisible = false;
        IsTouVisible = false;
        
        var meterReading = storageService
            .DailyAggregated
            .Where(w => w.PeriodStart >= StartDate && w.PeriodStart < EndDate)
            .ToList();
        
        if (meterReading.Count == 0)
        {
            return Task.CompletedTask;
        }

        CostCompares = meterReading
            .Where(w => w.Total > 0)
            .Select(s => new CostCompare(s, 3.9086m, 5.1135m, 2.6037m))
            .ToList();
            
        TouTotalCost = CostCompares.Sum(c => c.TouCost);
        FlatRateTotalCost = CostCompares.Sum(c => c.FlatRateCost);
        DiffInCurrency = FlatRateTotalCost - TouTotalCost;
        var touAverageCostPerDay = TouTotalCost / CostCompares.Count;
        var flatRateAverageCostPerDay = FlatRateTotalCost / CostCompares.Count;
        DiffInPercent = (flatRateAverageCostPerDay - touAverageCostPerDay) / flatRateAverageCostPerDay * 100;
            
        if(DiffInCurrency < 0)
        {
            IsFlatRateVisible = true;
            IsTouVisible = false;
        }
        else
        {
            IsFlatRateVisible = false;
            IsTouVisible = true;
        }

        return Task.CompletedTask;
    }

}

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class CostCompare : ObservableObject
{
    [ObservableProperty] private decimal touCost;
    [ObservableProperty] private decimal flatRateCost;
    [ObservableProperty] private decimal kwUsedAtPeek;
    [ObservableProperty] private decimal kwUsedAtOffPeek;
    [ObservableProperty] private PeaMeterReading meterReading;

    public CostCompare(PeaMeterReading meterReading, decimal flatRate, decimal peek, decimal offPeek)
    {
        MeterReading = meterReading;

        var isWeekday = meterReading.PeriodStart.DayOfWeek >= DayOfWeek.Monday &&
                         meterReading.PeriodStart.DayOfWeek <= DayOfWeek.Friday;
        var isWeekend = !isWeekday;

        
        if (meterReading.PeriodStart.Hour is >= 9 and < 22 && isWeekday)
        {
            TouCost += meterReading.Total * peek;
            KwUsedAtPeek += meterReading.Total;
        }
        else if (meterReading.PeriodStart.Hour >= 22 && isWeekday)
        {
            TouCost += meterReading.Total * offPeek;
            KwUsedAtOffPeek += meterReading.Total;
        }
        else if (meterReading.PeriodStart.Hour < 9 && isWeekday)
        {
            TouCost += meterReading.Total * offPeek;
            KwUsedAtOffPeek += meterReading.Total;
        }
        else if (isWeekend)
        {
            TouCost += meterReading.Total * offPeek;
            KwUsedAtOffPeek += meterReading.Total;
        }
        else
        {
            throw new Exception("Invalid time");
        }
        
        FlatRateCost = meterReading.Total * flatRate;
    }
}