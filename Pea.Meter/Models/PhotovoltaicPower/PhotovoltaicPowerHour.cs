using CommunityToolkit.Mvvm.ComponentModel;
using Pea.Meter.Services;

namespace Pea.Meter.Models.PhotovoltaicPower;

public partial class PhotovoltaicPowerHour : ObservableObject
{
    [ObservableProperty] private int hour;
    [ObservableProperty] private DateOnly date;
    [ObservableProperty] private decimal photovoltaicPower;

    public PhotovoltaicPowerHour(decimal installedKw, DateOnly date, int hour)
    {
        Date = date;
        Hour = hour;
        
        photovoltaicPower = SolarDataService.GetHourlyData(installedKw, Date.Month, Hour);
    }
    
    public decimal GetPhotovoltaicPower()
    {
        return PhotovoltaicPower;
    }
}