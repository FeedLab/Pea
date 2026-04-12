using Pea.Infrastructure.Helpers;

namespace Pea.Infrastructure.Models;

/// <summary>
/// Model representing a meter reading for a specific time period
/// </summary>
public class PeaMeterReading
{
    private readonly int periodLength;
    private readonly bool isWeekend;
    private readonly bool isHoliday;

    public List<DateOnly> Holidays =
    [
        new DateOnly(2026, 1, 1),
        new DateOnly(2026, 1, 1),
        new DateOnly(2026, 3, 3),
        new DateOnly(2026, 4, 6),
        new DateOnly(2026, 4, 13),
        new DateOnly(2026, 4, 14),
        new DateOnly(2026, 4, 15),
        new DateOnly(2026, 5, 1),
        new DateOnly(2026, 6, 3),
        new DateOnly(2026, 7, 28),
        new DateOnly(2026, 7, 29),
        new DateOnly(2026, 7, 30),
        new DateOnly(2026, 8, 12),
        new DateOnly(2026, 10, 13),
        new DateOnly(2026, 12, 10),
        new DateOnly(2026, 12, 31),
    ];


    private PeaMeterReading(DateTime periodStart)
    {
        isWeekend = periodStart.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        isHoliday = IsHolidayInternal(periodStart);
    }

    public PeaMeterReading(DateTime periodStart, decimal rateA, decimal rateB, decimal rateC, int periodLength = 15)
        : this(periodStart)
    {
        PeriodStart = periodStart;
        this.periodLength = periodLength;

        RateA = rateA;
        RateB = rateB;
        RateC = rateC;
    }

    public PeaMeterReading(DateTime periodStart, List<PeaMeterReading> readings, int periodLength = 15)
        : this(periodStart)
    {
        PeriodStart = periodStart;
        this.periodLength = periodLength;

        RateA = readings.Sum(r => r.RateA);
        RateB = readings.Sum(r => r.RateB);
        RateC = readings.Sum(r => r.RateC);

        var result = readings
            .GroupBy(r => r.PeriodStart.Date)
            .Select(g =>
            {
                var isWeekendOrHoliday = g.Key.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                                         || Holidays.Contains(DateOnly.FromDateTime(g.Key));

                return isWeekendOrHoliday
                    ? (Morning: g.Where(w => w.PeriodStart.Hour < 12).Sum(r => r.RateC + r.RateB),
                        Evening: g.Where(w => w.PeriodStart.Hour >= 12).Sum(r => r.RateC + r.RateB))
                    : (Morning: g.Where(w => w.PeriodStart.Hour < 9).Sum(r => r.RateC + r.RateB),
                        Evening: g.Where(w => w.PeriodStart.Hour >= 22).Sum(r => r.RateC + r.RateB));
            })
            .ToList();
        
        OffPeakMorning = result.Sum(r => r.Morning);
        OffPeakEvening = result.Sum(r => r.Evening);
    }

    private bool IsHolidayInternal(DateTime dateTime)
    {
        var dateOnly = DateOnly.FromDateTime(dateTime);
        return Holidays.Contains(dateOnly);
    }

    public bool IsHoliday => isHoliday;
    public bool IsWeekend => isWeekend;

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd => PeriodStart.AddMinutes(periodLength).AddMilliseconds(-1);

    public decimal RateA { get; }

    public decimal RateB { get; }

    public decimal RateC { get; }

    public decimal Total => RateA + RateB + RateC;
    public decimal Peek => RateA;
    public decimal OffPeek => RateB + RateC;
    public decimal OffPeakMorning { get; init; }
    public decimal OffPeakEvening { get; init; }

    public string PeekFormatted => WattFormatter.Format(Peek * 1000);
    public string OffPeekFormatted => WattFormatter.Format(OffPeek * 1000);
    public string TotalFormatted => WattFormatter.Format(Total * 1000);
    public string OffPeakMorningFormatted => WattFormatter.Format(OffPeakMorning * 1000);
    public string OffPeakEveningFormatted => WattFormatter.Format(OffPeakEvening * 1000);


    public override string ToString()
    {
        return
            $"PeriodStart: {PeriodStart:dd/MM/yyyy HH:mm}, PeriodEnd: {PeriodEnd:dd/MM/yyyy HH:mm}, RateA: {RateA}, RateB: {RateB}, RateC: {RateC}";
    }
}