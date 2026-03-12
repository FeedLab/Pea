using CommunityToolkit.Mvvm.ComponentModel;

namespace Pea.Meter.Models;

public partial class PhotovoltaicPowerHour : ObservableObject
{
    [ObservableProperty] private int hour;
    [ObservableProperty] private DateOnly date;
    [ObservableProperty] private decimal photovoltaicPower;

    public PhotovoltaicPowerHour(decimal installedKw, DateOnly date, int hour)
    {
        Date = date;
        Hour = hour;
        
        photovoltaicPower = SolarData.GetHourlyData(installedKw, Date.Month, Hour);
    }
    
    public decimal GetPhotovoltaicPower()
    {
        return PhotovoltaicPower;
    }
}