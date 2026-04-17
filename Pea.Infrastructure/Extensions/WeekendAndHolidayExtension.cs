namespace Pea.Infrastructure.Extensions;

public static class WeekendAndHolidayExtension
{
    public static IReadOnlySet<DateOnly> Holidays { get; } = new HashSet<DateOnly>
    {
        new(2026, 1, 1),
        new(2026, 3, 3),
        new(2026, 4, 6),
        new(2026, 4, 13),
        new(2026, 4, 14),
        new(2026, 4, 15),
        new(2026, 5, 1),
        new(2026, 6, 3),
        new(2026, 7, 28),
        new(2026, 7, 29),
        new(2026, 7, 30),
        new(2026, 8, 12),
        new(2026, 10, 13),
        new(2026, 12, 10),
        new(2026, 12, 31),
    };

    public static bool IsWeekend(this DateTime date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    public static bool IsHoliday(this DateTime date) =>
        Holidays.Contains(DateOnly.FromDateTime(date));

    public static bool IsWeekendOrHoliday(this DateTime date) =>
        date.IsWeekend() || date.IsHoliday();

    public static bool IsWorkingDay(this DateTime date) =>
        !date.IsWeekendOrHoliday();

    public static bool IsWeekend(this DateOnly date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    public static bool IsHoliday(this DateOnly date) =>
        Holidays.Contains(date);

    public static bool IsWeekendOrHoliday(this DateOnly date) =>
        date.IsWeekend() || date.IsHoliday();

    public static bool IsWorkingDay(this DateOnly date) =>
        !date.IsWeekendOrHoliday();

    public static DateOnly NextWorkingDay(this DateOnly date)
    {
        do { date = date.AddDays(1); } while (date.IsWeekendOrHoliday());
        return date;
    }

    public static DateOnly PreviousWorkingDay(this DateOnly date)
    {
        do { date = date.AddDays(-1); } while (date.IsWeekendOrHoliday());
        return date;
    }

    public static DateOnly AddWorkingDays(this DateOnly date, int days)
    {
        int step = days < 0 ? -1 : 1;
        int remaining = Math.Abs(days);
        while (remaining > 0)
        {
            date = date.AddDays(step);
            if (date.IsWorkingDay()) remaining--;
        }
        return date;
    }

    public static int CountWorkingDays(this DateOnly from, DateOnly to)
    {
        int count = 0;
        for (var d = from; d < to; d = d.AddDays(1))
            if (d.IsWorkingDay()) count++;
        return count;
    }

    public static IEnumerable<DateOnly> WorkingDaysInRange(this DateOnly from, DateOnly to)
    {
        for (var d = from; d <= to; d = d.AddDays(1))
            if (d.IsWorkingDay()) yield return d;
    }
}
