using Microsoft.EntityFrameworkCore;
using Pea.Data.Entities;
using Pea.Infrastructure.Models;
using Pea.Infrastructure.Repositories;

namespace Pea.Data.Repositories;

/// <summary>
/// Repository implementation for meter readings
/// </summary>
public class MeterReadingRepository : IMeterReadingRepository
{
    private readonly PeaDbContext context;

    public MeterReadingRepository(PeaDbContext context)
    {
        this.context = context;
    }

    
    public async Task AddRangeAsync(IEnumerable<PeaMeterReading> readings, string userId, CancellationToken cancellationToken = default)
    {
        var entities = readings.Select(r =>
        {
            var utcNow = DateTime.UtcNow;
            return new MeterReadingEntity
            {
                PeriodStart = r.PeriodStart,
                PeriodEnd = r.PeriodEnd,
                RateA = r.RateA,
                RateB = r.RateB,
                RateC = r.RateC,
                Total = r.Total,
                UserId = userId,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };
        });

        await context.MeterReadings.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IList<PeaMeterReading>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, string userId, CancellationToken cancellationToken = default)
    {
        var entities = await context.MeterReadings
            .Where(m => m.UserId == userId && m.PeriodStart >= startDate && m.PeriodStart <= endDate)
            .OrderBy(m => m.PeriodStart)
            .ToListAsync(cancellationToken);

        return entities.Select(e => new PeaMeterReading(e.PeriodStart, e.RateA, e.RateB, e.RateC)).ToList();
    }

    public async Task<IList<PeaMeterReading>> GetByDateAsync(DateTime date, string userId, CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        return await GetByDateRangeAsync(startOfDay, endOfDay, userId, cancellationToken);
    }

    public async Task<bool> ExistsForDateAsync(DateTime date, string userId, CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        return await context.MeterReadings
            .AnyAsync(m => m.UserId == userId && m.PeriodStart >= startOfDay && m.PeriodStart <= endOfDay, cancellationToken);
    }

    public async Task DeleteAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        var readings = await context.MeterReadings
            .Where(m => m.UserId == userId)
            .ToListAsync(cancellationToken);

        context.MeterReadings.RemoveRange(readings);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<DateTime?> GetLatestReadingDateAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await context.MeterReadings
            .Where(m => m.UserId == userId)
            .MaxAsync(m => (DateTime?)m.PeriodStart, cancellationToken);
    }

    public async Task<IList<PeaMeterReading>> GetAverageReadingsByTimeOfDayAsync(int periodInDays, string userId, CancellationToken cancellationToken = default)
    {
        var endDate = DateTime.Today; // Today at 00:00:00 (excluded)
        var startDate = endDate.AddDays(-periodInDays); // Start date at 00:00:00 (included)

        // Get all readings in the period (excluding today because it's incomplete)
        var readings = await context.MeterReadings
            .Where(m => m.UserId == userId && m.PeriodStart >= startDate && m.PeriodStart < endDate)
            .ToListAsync(cancellationToken);

        if (readings.Count == 0)
        {
            return new List<PeaMeterReading>();
        }

        // Group by time of day (hour and minute) and calculate average for each 15-minute slot
        var averageReadings = readings
            .GroupBy(r => new { r.PeriodStart.Hour, r.PeriodStart.Minute })
            .Select(g => new PeaMeterReading(
                new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, g.Key.Hour, g.Key.Minute, 0),
                g.Average(r => r.RateA),
                g.Average(r => r.RateB),
                g.Average(r => r.RateC)
            ))
            .OrderBy(r => r.PeriodStart)
            .ToList();

        return averageReadings;
    }

    public async Task<IList<PeaMeterReading>> GetHourlyTotalsAsync(DateTime startTime, DateTime endTime, string userId, CancellationToken cancellationToken = default)
    {
        var readings = await context.MeterReadings
            .Where(m => m.UserId == userId && m.PeriodStart >= startTime && m.PeriodStart < endTime)
            .ToListAsync(cancellationToken);

        if (readings.Count == 0)
        {
            return new List<PeaMeterReading>();
        }

        // Group by hour and sum the Total for each hour
        var hourlyTotals = readings
            .GroupBy(r => new DateTime(r.PeriodStart.Year, r.PeriodStart.Month, r.PeriodStart.Day, r.PeriodStart.Hour, 0, 0))
            .Select(g => new PeaMeterReading(
                g.Key,
                g.Sum(r => r.RateA),
                g.Sum(r => r.RateB),
                g.Sum(r => r.RateC),
                60
            ))
            .OrderBy(r => r.PeriodStart)
            .ToList();

        return hourlyTotals;
    }

    public async Task<IList<PeaMeterReading>> GetHourlyAveragesDuringPeriodAsync(DateTime startTime, DateTime endTime, string userId, CancellationToken cancellationToken = default)
    {
        var readings = await context.MeterReadings
            .Where(m => m.UserId == userId && m.PeriodStart >= startTime && m.PeriodStart < endTime)
            .ToListAsync(cancellationToken);

        if (readings.Count == 0)
        {
            return new List<PeaMeterReading>();
        }

        // Group by hour of day (0-23) and calculate average across all days
        var hourlyAverages = readings
            .GroupBy(r => r.PeriodStart.Hour)
            .Select(g => new PeaMeterReading(
                new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, g.Key, 0, 0),
                g.Average(r => r.RateA),
                g.Average(r => r.RateB),
                g.Average(r => r.RateC),
                60
            ))
            .OrderBy(r => r.PeriodStart)
            .ToList();

        return hourlyAverages;
    }
}
