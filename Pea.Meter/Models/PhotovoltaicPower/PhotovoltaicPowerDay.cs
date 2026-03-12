using CommunityToolkit.Mvvm.ComponentModel;

namespace Pea.Meter.Models;

public partial class PhotovoltaicPowerDay : ObservableObject
{
    [ObservableProperty] private DateOnly date;
    [ObservableProperty] private List<PhotovoltaicPowerHour> hours;

    public PhotovoltaicPowerDay(decimal installedKw, DateOnly date)
    {
        Date = date;

        Hours = Enumerable.Range(0, 23)
            .Select(d => new PhotovoltaicPowerHour(installedKw, Date, d))
            .ToList();
    }

    public decimal GetPhotovoltaicPower()
    {
        return Hours.Sum(s => s.GetPhotovoltaicPower());
    }
    
    public decimal GetPhotovoltaicPower(int hour)
    {
        if (hour is < 0 or > 23)
        {
            throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 23.");
        }

        return (Hours[hour - 1].GetPhotovoltaicPower());
    }
}