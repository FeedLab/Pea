using Microsoft.EntityFrameworkCore;
using Pea.Data.Entities;
using Pea.Infrastructure.Models;
using Pea.Infrastructure.Repositories;

namespace Pea.Data.Repositories;

/// <summary>
/// Repository implementation for meter readings
/// </summary>
public class MeterReadingRepository(PeaDbContext context) : IMeterReadingRepository
{
    public async Task AddRangeUpsertAsync(IEnumerable<PeaMeterReading> readings,
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var readingsList = readings.ToList();

        // Get existing records by PeriodStart
        var periodStarts = readingsList.Select(r => r.PeriodStart).ToList();
        var existingEntities = await context.MeterReadings
            .Where(m => periodStarts.Contains(m.PeriodStart))
            .ToListAsync(cancellationToken);

        var existingDict = existingEntities.ToDictionary(e => e.PeriodStart);

        foreach (var reading in readingsList)
        {
            if (existingDict.TryGetValue(reading.PeriodStart, out var existingEntity))
            {
                // Update existing
                existingEntity.PeriodEnd = reading.PeriodEnd;
                existingEntity.RateA = reading.RateA;
                existingEntity.RateB = reading.RateB;
                existingEntity.RateC = reading.RateC;
                existingEntity.Total = reading.Total;
                existingEntity.UpdatedAt = utcNow;
            }
            else
            {
                // Insert new
                var newEntity = new MeterReadingEntity
                {
                    PeriodStart = reading.PeriodStart,
                    PeriodEnd = reading.PeriodEnd,
                    RateA = reading.RateA,
                    RateB = reading.RateB,
                    RateC = reading.RateC,
                    Total = reading.Total,
                    CreatedAt = utcNow,
                    UpdatedAt = utcNow
                };
                await context.MeterReadings.AddAsync(newEntity, cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<PeaMeterReading> readings, CancellationToken cancellationToken = default)
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
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };
        });

        await context.MeterReadings.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IList<PeaMeterReading>> GetAllMeterReadingsAsync( CancellationToken cancellationToken = default)
    {
        var entities = await context.MeterReadings.ToListAsync(cancellationToken);

        return entities
            .OrderBy(e => e.PeriodStart)
            .Select(e => new PeaMeterReading(e.PeriodStart, e.RateA, e.RateB, e.RateC))
            .ToList();
    }
    
    public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        await context.MeterReadings.ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<bool> HasReadingsForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await context.MeterReadings
            .AnyAsync(m => m.PeriodStart.Date == date.Date, cancellationToken);
    }
    
    public async Task<DateTime> GetOldestPeriodStartAsync(CancellationToken cancellationToken = default)
    {
        if (!await context.MeterReadings.AnyAsync(cancellationToken))
        {
            return DateTime.Now.Date;
        }
        return await context.MeterReadings.MinAsync(m => m.PeriodStart, cancellationToken);
    }
    
    public async Task DeleteBeforeDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        await context.MeterReadings
            .Where(m => m.PeriodStart < date)
            .ExecuteDeleteAsync(cancellationToken);
    }
    
}