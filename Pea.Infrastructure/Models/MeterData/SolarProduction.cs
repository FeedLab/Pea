using CommunityToolkit.Mvvm.ComponentModel;

namespace Pea.Infrastructure.Models.MeterData;

public partial class SolarProduction(decimal flatRatePrice, decimal peekPrice, decimal offPeekPrice) : ObservableObject
{
    [ObservableProperty] private decimal calculatedSolarProductionInKw;
    [ObservableProperty] private decimal calculatedBatteryNeeded;

    public decimal Calculate(decimal solarProductionInKw, decimal batteryNeeded, MeterDataUsageInKw meterDataUsage,
        MeterDataUsageInMoney meterDataUsageInMoney)
    {
        CalculatedBatteryNeeded = batteryNeeded;
        CalculatedSolarProductionInKw = solarProductionInKw - batteryNeeded;


        var surplus = SavedKw.Calculate(meterDataUsage, solarProductionInKw);
        SavedMoney.Calculate(SavedKw, solarProductionInKw, flatRatePrice, peekPrice, offPeekPrice);
        DiscountedMoney.Calculate(meterDataUsageInMoney, SavedMoney);

        return surplus;
    }
    

    
    [ObservableProperty] private SavedInKw savedKw = new();
    [ObservableProperty] private SavedInMoney savedMoney = new();
    [ObservableProperty] private DiscountedInMoney discountedMoney = new();
}

public partial class SavedInKw : ObservableObject
{
    [ObservableProperty] private decimal peek;
    [ObservableProperty] private decimal offPeek;
    [ObservableProperty] private decimal holiday;
    [ObservableProperty] private decimal flatRate;

    public decimal Calculate(MeterDataUsageInKw meterDataUsage, decimal solarProduction)
    {
        var surplusHoliday = 0m;
        var surplusFlatRate = 0m;

        System.Diagnostics.Debug.WriteLine(
            $"[SavedInKw.Calculate] Input: solarProduction={solarProduction}, Holiday={meterDataUsage.Holiday}, Peek={meterDataUsage.PeekUsage}, OffPeek={meterDataUsage.OffPeekUsage}");

        if (meterDataUsage.Holiday > 0.0m)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[SavedInKw.Calculate] Branch: Holiday > 0 (value={meterDataUsage.Holiday})");
            if (solarProduction > meterDataUsage.Holiday)
            {
                Holiday = meterDataUsage.Holiday;
                surplusHoliday += solarProduction - meterDataUsage.Holiday;
                System.Diagnostics.Debug.WriteLine(
                    $"[SavedInKw.Calculate] Holiday path: All holiday covered, surplus={surplusHoliday}");
            }
            else
            {
                Holiday = solarProduction;
                System.Diagnostics.Debug.WriteLine(
                    $"[SavedInKw.Calculate] Holiday path: Partial holiday covered={solarProduction}");
            }
        }

        System.Diagnostics.Debug.WriteLine($"[SavedInKw.Calculate] Branch: No Holiday, processing Peek/OffPeek");
        if (solarProduction > meterDataUsage.PeekUsage)
        {
            Peek = meterDataUsage.PeekUsage;
            OffPeek = (solarProduction - meterDataUsage.PeekUsage);
            System.Diagnostics.Debug.WriteLine(
                $"[SavedInKw.Calculate] Solar covers all Peek ({Peek}), remaining={OffPeek}");

            if (OffPeek > meterDataUsage.OffPeekUsage)
            {
                OffPeek = meterDataUsage.OffPeekUsage;
                System.Diagnostics.Debug.WriteLine($"[SavedInKw.Calculate] Solar covers all OffPeek too ({OffPeek})");
            }
        }
        else
        {
            Peek = solarProduction;
            OffPeek = 0;
            System.Diagnostics.Debug.WriteLine($"[SavedInKw.Calculate] Solar only partially covers Peek ({Peek})");
        }

        if (solarProduction > meterDataUsage.TotalUsage)
        {
            FlatRate = meterDataUsage.TotalUsage;
            surplusFlatRate += solarProduction - meterDataUsage.TotalUsage;
            System.Diagnostics.Debug.WriteLine(
                $"[SavedInKw.Calculate] FlatRate: All usage covered, surplus={surplusFlatRate}");
        }
        else
        {
            FlatRate = solarProduction;
            System.Diagnostics.Debug.WriteLine($"[SavedInKw.Calculate] FlatRate: Partial coverage={FlatRate}");
        }

        System.Diagnostics.Debug.WriteLine(
            $"[SavedInKw.Calculate] Final: Peek={Peek}, OffPeek={OffPeek}, Holiday={Holiday}, FlatRate={FlatRate}, TotalSurplus={surplusHoliday + surplusFlatRate}");

        return surplusHoliday + surplusFlatRate;
    }
}

public partial class SavedInMoney : ObservableObject
{
    [ObservableProperty] private decimal peek;
    [ObservableProperty] private decimal offPeek;
    [ObservableProperty] private decimal holiday;
    [ObservableProperty] private decimal flatRate;

    public void Calculate(SavedInKw meterDataSaved, decimal solarProduction, decimal flatRatePrice, decimal peekPrice,
        decimal offPeekPrice)
    {
        FlatRate = meterDataSaved.FlatRate * flatRatePrice;
        Peek = meterDataSaved.Peek * peekPrice;
        OffPeek = meterDataSaved.OffPeek * offPeekPrice;
        Holiday = meterDataSaved.Holiday * offPeekPrice;
    }
}

public partial class DiscountedInMoney : ObservableObject
{
    [ObservableProperty] private decimal peek;
    [ObservableProperty] private decimal offPeek;
    [ObservableProperty] private decimal holiday;
    [ObservableProperty] private decimal flatRate;

    public void Calculate(MeterDataUsageInMoney meterDataUsageInMoney, SavedInMoney meterDataSaved)
    {
        // Calculate the discounted (remaining) cost after solar savings
        Peek = (meterDataUsageInMoney.PeekTouUsagePriceSummary - meterDataSaved.Peek);
        OffPeek = (meterDataUsageInMoney.OffPeekTouUsagePriceSummary - meterDataSaved.OffPeek);
        // Holiday = (meterDataUsageInMoney. - meterDataSaved.Holiday) * offPeekPrice;
        FlatRate = (meterDataUsageInMoney.FlatRateUsagePriceSummary - meterDataSaved.FlatRate);
    }
}