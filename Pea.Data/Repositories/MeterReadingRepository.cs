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
        var entities = await context.MeterReadings
            .OrderBy(m => m.PeriodStart)
            .ToListAsync(cancellationToken);

        return entities.Select(e => new PeaMeterReading(e.PeriodStart, e.RateA, e.RateB, e.RateC)).ToList();
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
}
