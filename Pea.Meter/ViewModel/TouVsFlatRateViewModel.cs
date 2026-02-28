using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0019:Invalid containing type for [ObservableProperty] field or property")]
[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class TouVsFlatRateViewModel: ObservableObject
{
    private readonly PeaDbContextFactory dbContextFactory;

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
    private string userName;

    public TouVsFlatRateViewModel(PeaDbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;

        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, async (r, m) =>
        {
            using var dbContext = dbContextFactory.CreateDbContext(m.AuthData.Username);
            var repository = new MeterReadingRepository(dbContext);
            var meterReading = await repository.GetEarliestMeterReading(m.AuthData.Username);

            if (meterReading == null)
            {
                return;
            }
            
            var today = DateTime.Now;
            StartDate = today.AddYears(-1);
            EndDate = today;
            
            StartTimePickerMinimumDate = meterReading.PeriodStart.Date;
            StartTimePickerMaximumDate = EndDate.AddDays(-1);

            if (StartDate < StartTimePickerMinimumDate)
            {
                StartDate = StartTimePickerMinimumDate;
            }
            
            EndTimePickerMinimumDate = StartDate.AddDays(1);
            EndTimePickerMaximumDate = today;
            await Task.Delay(5000);
            userName = m.AuthData.Username;
            await CalculateCostComparisons();
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

    public async Task CalculateCostComparisons()
    {
        IsFlatRateVisible = false;
        IsTouVisible = false;
        
        var meterReadingsPerDay = await FetchDailyAverageReadingsAsync();

        if (meterReadingsPerDay.Count == 0)
        {
            return;
        }

        CostCompares = meterReadingsPerDay
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
    }

    private async Task<IList<PeaMeterReading>> FetchDailyAverageReadingsAsync( )
    {
        var meterDataAverageDaysTask = await Task.Run(async () =>
        {
            using var dbContext = dbContextFactory.CreateDbContext(userName);
            var repo = new MeterReadingRepository(dbContext);
            
            var startTime = StartDate;
            var endTime = EndDate;
            
            return await repo.GetDailyTotalsAsync(startTime, endTime, userName);
        });
        
        return meterDataAverageDaysTask;
    }
}

public partial class CostCompare : ObservableObject
{
    [ObservableProperty] private decimal touCost;
    [ObservableProperty] private decimal flatRateCost;
    [ObservableProperty] private PeaMeterReading meterReading;

    public CostCompare(PeaMeterReading meterReading, decimal flatRate, decimal peek, decimal offPeek)
    {
        MeterReading = meterReading;
        
        TouCost += meterReading.RateA * peek;
        TouCost += meterReading.RateB * offPeek;
        TouCost += meterReading.RateC * offPeek;
        
        FlatRateCost = meterReading.Total * flatRate;
    }
}