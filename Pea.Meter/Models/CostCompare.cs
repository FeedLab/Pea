using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using Pea.Infrastructure.Models;

namespace Pea.Meter.Models;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class CostCompare : ObservableObject
{
    public decimal FlatRatePrice { get; }
    public decimal PeekPrice { get; }
    public decimal OffPeekPrice { get; }
    
    [ObservableProperty] private decimal touCost;
    [ObservableProperty] private decimal flatRateCost;
    [ObservableProperty] private decimal kwCostAtPeek;
    [ObservableProperty] private decimal kwCostAtOffPeek;  
    [ObservableProperty] private decimal kwUsed;  
    [ObservableProperty] private bool isPeekPeriod;
    [ObservableProperty] private PeaMeterReading meterReading;

    public CostCompare(PeaMeterReading meterReading, decimal flatRatePrice, decimal peekPrice, decimal offPeekPrice)
    {
        FlatRatePrice = flatRatePrice;
        PeekPrice = peekPrice;
        OffPeekPrice = offPeekPrice;
        MeterReading = meterReading;
        KwUsed = meterReading.Total;
 
        var isWeekday = meterReading.PeriodStart.DayOfWeek >= DayOfWeek.Monday &&
                        meterReading.PeriodStart.DayOfWeek <= DayOfWeek.Friday;
        var isWeekend = !isWeekday;


        if (meterReading.PeriodStart.Hour is >= 9 and < 22 && isWeekday)
        {
            TouCost += meterReading.Total * peekPrice;
            KwCostAtPeek += meterReading.Total * peekPrice;
            
            isPeekPeriod = true;
        }
        else if (meterReading.PeriodStart.Hour >= 22 && isWeekday)
        {
            TouCost += meterReading.Total * offPeekPrice;
            KwCostAtOffPeek += meterReading.Total * offPeekPrice;
            
            isPeekPeriod = false;
        }
        else if (meterReading.PeriodStart.Hour < 9 && isWeekday)
        {
            TouCost += meterReading.Total * offPeekPrice;
            KwCostAtOffPeek += meterReading.Total * offPeekPrice;
       
            isPeekPeriod = false;
        }
        else if (isWeekend)
        {
            TouCost += meterReading.Total * offPeekPrice;
            KwCostAtOffPeek += meterReading.Total * offPeekPrice;
            
            isPeekPeriod = false;
        }
        else
        {
            throw new Exception("Invalid time");
        }

        FlatRateCost = meterReading.Total * flatRatePrice;
    }
}