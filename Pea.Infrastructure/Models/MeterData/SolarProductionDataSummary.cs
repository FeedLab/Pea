using CommunityToolkit.Mvvm.ComponentModel;

namespace Pea.Infrastructure.Models.MeterData;

public partial class SolarProductionDataSummary(decimal flatRatePrice, decimal peekPrice, decimal offPeekPrice) : ObservableObject
{
    public void Calculate(decimal solarProduction, MeterDataUsageInKwSummary meterDataUsage)
    {
        // Reset all values
        SolarLostInKw = 0;
        AmountOfSavedTouPeekKw = 0;
        AmountOfSavedTouOffPeekKw = 0;
        AmountOfSavedTouHolidayKw = 0;
        AmountOfSavedFlatRateKw = 0;
        AmountOfSavedTouPeekMoney = 0;
        AmountOfSavedTouOffPeekMoney = 0;
        AmountOfSavedTouHolidayMoney = 0;
        SolarLostMoney = 0;

        SolarProductionInKw = solarProduction;

        if (meterDataUsage.Holiday > 0.0m)
        {
            if (solarProduction > meterDataUsage.Holiday)
            {
                AmountOfSavedTouHolidayKw = meterDataUsage.Holiday;
                SolarLostInKw = solarProduction - meterDataUsage.Holiday;
            }
            else
            {
                AmountOfSavedTouHolidayKw = solarProduction;
            }
        }
        else
        {
            if (solarProduction > meterDataUsage.PeekUsage)
            {
                AmountOfSavedTouPeekKw = meterDataUsage.PeekUsage;
                AmountOfSavedTouOffPeekKw = (solarProduction - meterDataUsage.PeekUsage);
            }
            else
            {
                AmountOfSavedTouPeekKw = solarProduction;
                AmountOfSavedTouOffPeekKw = 0;
            }
        }

        if (solarProduction > meterDataUsage.TotalUsage)
        {
            AmountOfSavedFlatRateKw = meterDataUsage.TotalUsage;
            SolarLostInKw = solarProduction - meterDataUsage.TotalUsage;
        }
        else
        {
            AmountOfSavedFlatRateKw = solarProduction;
        }
        
        AmountOfSavedFlatRateMoney = AmountOfSavedFlatRateKw * flatRatePrice;
        AmountOfSavedTouPeekMoney = AmountOfSavedTouPeekKw * peekPrice;
        AmountOfSavedTouOffPeekMoney = AmountOfSavedTouOffPeekKw * offPeekPrice;
        AmountOfSavedTouHolidayMoney = AmountOfSavedTouHolidayKw * offPeekPrice;
        SolarLostMoney = SolarLostInKw * offPeekPrice;
    }
    
    
    [ObservableProperty] private decimal solarLostInKw;
    [ObservableProperty] private decimal solarProductionInKw;
    [ObservableProperty] private decimal amountOfSavedTouPeekKw;
    [ObservableProperty] private decimal amountOfSavedTouOffPeekKw;
    [ObservableProperty] private decimal amountOfSavedTouHolidayKw;
    [ObservableProperty] private decimal amountOfSavedFlatRateKw;
    
    [ObservableProperty] private decimal amountOfSavedFlatRateMoney;
    [ObservableProperty] private decimal amountOfSavedTouPeekMoney;
    [ObservableProperty] private decimal amountOfSavedTouOffPeekMoney;
    [ObservableProperty] private decimal amountOfSavedTouHolidayMoney;
    [ObservableProperty] private decimal solarLostMoney;
    
}