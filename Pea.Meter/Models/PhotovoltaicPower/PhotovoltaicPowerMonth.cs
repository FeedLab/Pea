using CommunityToolkit.Mvvm.ComponentModel;

namespace Pea.Meter.Models;

public partial class PhotovoltaicPowerMonth : ObservableObject
{
    [ObservableProperty] private int year;
    [ObservableProperty] private int month;
    [ObservableProperty] private int daysInMonth;
    [ObservableProperty] private string monthName;
    [ObservableProperty] private decimal touCost;
    [ObservableProperty] private List<PhotovoltaicPowerDay> days;

    public PhotovoltaicPowerMonth(decimal installedKw, int year, int month)
    {
        Year = year;
        Month = month;
        DaysInMonth = DateTime.DaysInMonth(year, month);

        MonthName = GetMonthNameFromValue(month);

        Days = Enumerable.Range(1, daysInMonth)
            .Select(d => new PhotovoltaicPowerDay(installedKw, new DateOnly(year, month, d)))
            .ToList();
    }

    public decimal GetPhotovoltaicPower()
    {
        return Days.Sum(s => s.GetPhotovoltaicPower());
    }

    public decimal GetPhotovoltaicPower(int day)
    {
        if (day < 0 || day -1 > DaysInMonth)
        {
            throw new ArgumentOutOfRangeException(nameof(day), $"Day must be between 0 and {DaysInMonth}.");
        }
        
        return Days[day - 1].GetPhotovoltaicPower();
    }

    partial void OnMonthChanged(int value)
    {
        MonthName = GetMonthNameFromValue(value);
    }

    private static string GetMonthNameFromValue(int value)
    {
        return new DateTime(DateTime.Now.Year, value, 1).ToString("MMMM");
    }
}