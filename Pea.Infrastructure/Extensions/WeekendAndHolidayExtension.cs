namespace Pea.Infrastructure.Extensions;

public static class WeekendAndHolidayExtension
{
    public static List<DateOnly> Holidays =
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

    public static bool IsWeekend(this DateTime periodStart) =>
        periodStart.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    public static bool IsHoliday(this DateTime periodStart) =>
        Holidays.Contains(DateOnly.FromDateTime(periodStart));

    public static bool IsWeekendOrHoliday(this DateTime periodStart) =>
        periodStart.IsWeekend() || periodStart.IsHoliday();

    public static bool IsWeekend(this DateOnly periodStart) =>
        periodStart.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    public static bool IsHoliday(this DateOnly periodStart) =>
        Holidays.Contains(periodStart);

    public static bool IsWeekendOrHoliday(this DateOnly periodStart) =>
        periodStart.IsWeekend() || periodStart.IsHoliday();
}
