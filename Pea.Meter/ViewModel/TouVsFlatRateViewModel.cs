using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;
using Pea.Meter.Models;

namespace Pea.Meter.ViewModel;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0019:Invalid containing type for [ObservableProperty] field or property")]
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
    
    public TouVsFlatRateViewModel(PeaDbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;

        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, async (r, m) =>
        {
            await Task.Delay(5000);
            await CalculateCostComparisons(m.AuthData.Username);
        });
    }

    private async Task CalculateCostComparisons(string userName)
    {
        IsFlatRateVisible = false;
        IsTouVisible = false;
        
        var meterReadingsPerDay = await FetchDailyAverageReadingsAsync(dbContextFactory, userName);

        CostCompares = meterReadingsPerDay
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

    private static async Task<IList<PeaMeterReading>> FetchDailyAverageReadingsAsync(PeaDbContextFactory dbContextFactory, string userName)
    {
        var meterDataAverageDaysTask = await Task.Run(async () =>
        {
            using var dbContext = dbContextFactory.CreateDbContext(userName);
            var repo = new MeterReadingRepository(dbContext);
            return await repo.GetDailyTotalsAsync(new DateTime(2024, 1, 1), DateTime.MaxValue, userName);
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