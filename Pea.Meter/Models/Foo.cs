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

    public PhotovoltaicPowerMonth(int year, int month)
    {
        Year = year;
        Month = month;
        DaysInMonth = DateTime.DaysInMonth(year, month);

        MonthName = GetMonthNameFromValue(month);

        Days = Enumerable.Range(1, daysInMonth)
            .Select(d => new PhotovoltaicPowerDay(new DateOnly(year, month, d)))
            .ToList();
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

public partial class PhotovoltaicPowerDay : ObservableObject
{
    [ObservableProperty] private DateOnly date;
    [ObservableProperty] private List<PhotovoltaicPowerHour> hours;

    public PhotovoltaicPowerDay(DateOnly date)
    {
        Date = date;

        Hours = Enumerable.Range(0, 23)
            .Select(d => new PhotovoltaicPowerHour(Date, d))
            .ToList();
    }
}

public partial class PhotovoltaicPowerHour : ObservableObject
{
    [ObservableProperty] private int hour;
    [ObservableProperty] private DateOnly date;
    [ObservableProperty] private decimal photovoltaicPower;

    public PhotovoltaicPowerHour(DateOnly date, int hour)
    {
        Date = date;
        Hour = hour;
        
        photovoltaicPower = SolarData.GetHourlyData(Date.Month, Hour);
    }
}

public static class SolarData
{
    private const decimal BaseKw = 21.0m;

    public static decimal GetHourlyData(int month, int hour)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }

        return HourlyData[hour, month - 1] / BaseKw;
    }

    public static decimal GetMonthlySummary(int month, int beginHour, int endHour)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }

        if (beginHour < 0 || beginHour > 23)
        {
            throw new ArgumentOutOfRangeException(nameof(beginHour), "Hour must be between 0 and 23.");
        }

        if (endHour < 0 || endHour > 23)
        {
            throw new ArgumentOutOfRangeException(nameof(endHour), "Hour must be between 0 and 23.");
        }

        if (beginHour > endHour)
        {
            throw new ArgumentException("Begin hour must be less than or equal to end hour.");
        }

        var monthIndex = month - 1;
        var total = 0m;
        
        for (var hour = beginHour; hour <= endHour; hour++)
        {
            total += HourlyData[hour, monthIndex - 1];
        }

        return total;
    }

    public static List<decimal> GetDailyDataRange(int month, int beginHour, int endHour)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }

        if (beginHour < 0 || beginHour > 23)
        {
            throw new ArgumentOutOfRangeException(nameof(beginHour), "Hour must be between 0 and 23.");
        }

        if (endHour < 0 || endHour > 23)
        {
            throw new ArgumentOutOfRangeException(nameof(endHour), "Hour must be between 0 and 23.");
        }

        if (beginHour > endHour)
        {
            throw new ArgumentException("Begin hour must be less than or equal to end hour.");
        }

        var result = new List<decimal>();
        var monthIndex = month - 1;

        for (var hour = beginHour; hour <= endHour; hour++)
        {
            result.Add(HourlyData[hour, monthIndex] / BaseKw);
        }

        return result;
    }


    private static readonly decimal[,] HourlyData = new decimal[24, 12]
    {
        // Jan, Feb, Mar, Apr, May, Jun, Jul, Aug, Sep, Oct, Nov, Dec
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 0 - 1
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 1 - 2
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 2 - 3
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 3 - 4
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 4 - 5
        { 0, 0, 0, 0, 0.006m, 0.012m, 0, 0, 0, 0, 0, 0 }, // 5 - 6
        { 0.040m, 0.061m, 0.198m, 0.810m, 1.190m, 1.192m, 0.829m, 0.676m, 0.659m, 0.684m, 0.350m, 0.128m }, // 6 - 7
        { 1.881m, 2.013m, 2.697m, 3.811m, 4.085m, 3.883m, 3.228m, 2.953m, 3.034m, 3.412m, 3.481m, 2.527m }, // 7 - 8
        { 5.510m, 5.620m, 6.207m, 7.298m, 7.272m, 6.845m, 5.955m, 5.725m, 5.788m, 6.505m, 7.039m, 6.184m }, // 8 - 9
        { 8.796m, 9.112m, 9.406m, 10.312m, 10.017m, 9.320m, 8.311m, 8.264m, 8.096m, 9.062m, 9.929m, 9.453m }, // 9 - 10
        {
            11.291m, 11.714m, 11.900m, 12.430m, 11.756m, 10.851m, 10.038m, 9.921m, 9.756m, 10.686m, 11.783m, 11.764m
        }, // 10 - 11
        {
            12.664m, 13.129m, 13.194m, 13.439m, 12.481m, 11.598m, 10.681m, 10.669m, 10.513m, 11.237m, 12.405m, 12.790m
        }, // 11 - 12
        {
            12.992m, 13.395m, 13.465m, 13.403m, 12.424m, 11.779m, 10.887m, 10.963m, 10.900m, 11.262m, 12.220m, 12.851m
        }, // 12 - 13
        {
            12.129m, 12.550m, 12.584m, 12.297m, 11.456m, 10.918m, 10.082m, 10.216m, 10.105m, 10.291m, 11.013m, 11.817m
        }, // 13 - 14
        {
            10.244m, 10.687m, 10.561m, 10.303m, 9.532m, 9.097m, 8.708m, 8.633m, 8.232m, 8.364m, 8.786m, 9.606m
        }, // 14 - 15
        { 7.349m, 7.834m, 7.637m, 7.430m, 6.610m, 6.413m, 6.359m, 6.358m, 5.687m, 5.638m, 5.825m, 6.515m }, // 15 - 16
        { 3.872m, 4.419m, 4.293m, 4.158m, 3.892m, 3.988m, 3.982m, 3.760m, 3.102m, 2.697m, 2.531m, 2.972m }, // 16 - 17
        { 0.744m, 1.134m, 1.248m, 1.310m, 1.303m, 1.566m, 1.661m, 1.404m, 0.769m, 0.257m, 0.168m, 0.233m }, // 17 - 18
        { 0, 0, 0.011m, 0.028m, 0.061m, 0.104m, 0.124m, 0.070m, 0, 0, 0, 0 }, // 18 - 19
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 19 - 20
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 20 - 21
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 21 - 22
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 22 - 23
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } // 23 - 24
    };
}