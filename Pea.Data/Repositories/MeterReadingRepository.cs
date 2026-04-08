using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pea.Data.Entities;
using Pea.Infrastructure.Models;
using Pea.Infrastructure.Repositories;

namespace Pea.Data.Repositories;

/// <summary>
/// Repository implementation for meter readings
/// </summary>
public class MeterReadingRepository : IMeterReadingRepository
{
    private static readonly SemaphoreSlim syncLock = new(1, 1);
    private readonly ILogger<MeterReadingRepository> logger;
    private readonly PeaDbContextFactory contextFactory;

    /// <summary>
    /// Repository implementation for meter readings
    /// </summary>
    public MeterReadingRepository(ILogger<MeterReadingRepository> logger, PeaDbContextFactory contextFactory)
    {
        this.logger = logger;
        this.contextFactory = contextFactory;
    }

    public async Task AddRangeAsync(IEnumerable<PeaMeterReading> readings, string meterNumber,
        CancellationToken cancellationToken = default)
    {
        var readingsList = readings.ToList();
        logger.LogInformation("Enter {Method} for meter {MeterNumber}, {Count} readings", nameof(AddRangeAsync), meterNumber, readingsList.Count);
        await syncLock.WaitAsync(cancellationToken);
        try
        {
            await using var context = contextFactory.CreateDbContext();
            var utcNow = DateTime.UtcNow;
            var entities = readingsList.Select(r => new MeterReadingEntity
            {
                MeterNumber = meterNumber,
                PeriodStart = r.PeriodStart,
                PeriodEnd = r.PeriodEnd,
                RateA = r.RateA,
                RateB = r.RateB,
                RateC = r.RateC,
                Total = r.Total,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            });
            await context.MeterReadings.AddRangeAsync(entities, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "DbUpdateException in {Method} for meter {MeterNumber}", nameof(AddRangeAsync), meterNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected exception in {Method} for meter {MeterNumber}", nameof(AddRangeAsync), meterNumber);
        }
        finally
        {
            syncLock.Release();
            logger.LogInformation("Exit {Method} for meter {MeterNumber}", nameof(AddRangeAsync), meterNumber);
        }
    }

    public async Task AddRangeUpsertAsync(IEnumerable<PeaMeterReading> readings, string meterNumber,
        CancellationToken cancellationToken = default)
    {
        var readingsList = readings.ToList();
        logger.LogInformation("Enter {Method} for meter {MeterNumber}, {Count} readings", nameof(AddRangeUpsertAsync), meterNumber, readingsList.Count);
        await syncLock.WaitAsync(cancellationToken);
        try
        {
            await using var context = contextFactory.CreateDbContext();
            var utcNow = DateTime.UtcNow;

            var periodStarts = readingsList.Select(r => r.PeriodStart).ToList();
            var existingEntities = await context.MeterReadings
                .Where(m => periodStarts.Contains(m.PeriodStart) && m.MeterNumber == meterNumber)
                .ToListAsync(cancellationToken);

            var existingDict = existingEntities.ToDictionary(e => e.PeriodStart);
            var insertCount = 0;
            var updateCount = 0;

            foreach (var reading in readingsList)
            {
                if (existingDict.TryGetValue(reading.PeriodStart, out var existingEntity))
                {
                    existingEntity.MeterNumber = meterNumber;
                    existingEntity.PeriodStart = reading.PeriodStart;
                    existingEntity.PeriodEnd = reading.PeriodEnd;
                    existingEntity.RateA = reading.RateA;
                    existingEntity.RateB = reading.RateB;
                    existingEntity.RateC = reading.RateC;
                    existingEntity.Total = reading.Total;
                    existingEntity.UpdatedAt = utcNow;
                    updateCount++;
                }
                else
                {
                    var newEntity = new MeterReadingEntity
                    {
                        MeterNumber = meterNumber,
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
                    insertCount++;
                }
            }

            logger.LogInformation("{Method}: {InsertCount} inserts, {UpdateCount} updates for meter {MeterNumber}", nameof(AddRangeUpsertAsync), insertCount, updateCount, meterNumber);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "DbUpdateException in {Method} for meter {MeterNumber}", nameof(AddRangeUpsertAsync), meterNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected exception in {Method} for meter {MeterNumber}", nameof(AddRangeUpsertAsync), meterNumber);
        }
        finally
        {
            syncLock.Release();
            logger.LogInformation("Exit {Method} for meter {MeterNumber}", nameof(AddRangeUpsertAsync), meterNumber);
        }
    }

    public async Task<IList<PeaMeterReading>> GetAllMeterReadingsAsync(string meterNumber,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Enter {Method} for meter {MeterNumber}", nameof(GetAllMeterReadingsAsync), meterNumber);
        await syncLock.WaitAsync(cancellationToken);
        try
        {
            await using var context = contextFactory.CreateDbContext();
            var entities = await context.MeterReadings
                .Where(w => w.MeterNumber == meterNumber)
                .ToListAsync(cancellationToken);

            var result = entities
                .OrderBy(e => e.PeriodStart)
                .Select(e => new PeaMeterReading(e.PeriodStart, e.RateA, e.RateB, e.RateC))
                .ToList();

            logger.LogInformation("{Method}: returning {Count} readings for meter {MeterNumber}", nameof(GetAllMeterReadingsAsync), result.Count, meterNumber);
            return result;
        }
        finally
        {
            syncLock.Release();
            logger.LogInformation("Exit {Method} for meter {MeterNumber}", nameof(GetAllMeterReadingsAsync), meterNumber);
        }
    }

    public async Task DeleteAllAsync(string meterNumber, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Enter {Method} for meter {MeterNumber}", nameof(DeleteAllAsync), meterNumber);
        await syncLock.WaitAsync(cancellationToken);
        try
        {
            await using var context = contextFactory.CreateDbContext();
            var deleted = await context.MeterReadings
                .Where(w => w.MeterNumber == meterNumber)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation("{Method}: deleted {Count} readings for meter {MeterNumber}", nameof(DeleteAllAsync), deleted, meterNumber);
        }
        finally
        {
            syncLock.Release();
            logger.LogInformation("Exit {Method} for meter {MeterNumber}", nameof(DeleteAllAsync), meterNumber);
        }
    }

    public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Enter {Method} (all meters)", nameof(DeleteAllAsync));
        await syncLock.WaitAsync(cancellationToken);
        try
        {
            await using var context = contextFactory.CreateDbContext();
            var deleted = await context.MeterReadings
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation("{Method}: deleted {Count} readings across all meters", nameof(DeleteAllAsync), deleted);
        }
        finally
        {
            syncLock.Release();
            logger.LogInformation("Exit {Method} (all meters)", nameof(DeleteAllAsync));
        }
    }

    public async Task<bool> HasReadingsForDateAsync(string meterNumber, DateTime date,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Enter {Method} for meter {MeterNumber}, date {Date:yyyy-MM-dd}", nameof(HasReadingsForDateAsync), meterNumber, date);
        await syncLock.WaitAsync(cancellationToken);
        try
        {
            await using var context = contextFactory.CreateDbContext();
            var result = await context.MeterReadings
                .Where(w => w.MeterNumber == meterNumber)
                .AnyAsync(m => m.PeriodStart.Date == date.Date, cancellationToken);

            logger.LogInformation("{Method}: meter {MeterNumber} has readings for {Date:yyyy-MM-dd}: {Result}", nameof(HasReadingsForDateAsync), meterNumber, date, result);
            return result;
        }
        finally
        {
            syncLock.Release();
            logger.LogInformation("Exit {Method} for meter {MeterNumber}", nameof(HasReadingsForDateAsync), meterNumber);
        }
    }

    public async Task<DateTime?> GetOldestPeriodStartAsync(string meterNumber,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Enter {Method} for meter {MeterNumber}", nameof(GetOldestPeriodStartAsync), meterNumber);
        await syncLock.WaitAsync(cancellationToken);
        try
        {
            await using var context = contextFactory.CreateDbContext();
            if (!await context.MeterReadings
                    .Where(w => w.MeterNumber == meterNumber)
                    .AnyAsync(cancellationToken))
            {
                logger.LogInformation("{Method}: no readings found for meter {MeterNumber}", nameof(GetOldestPeriodStartAsync), meterNumber);
                return null;
            }

            var result = await context.MeterReadings
                .Where(w => w.MeterNumber == meterNumber)
                .MinAsync(m => m.PeriodStart, cancellationToken);

            logger.LogInformation("{Method}: oldest reading for meter {MeterNumber} is {Date:yyyy-MM-dd}", nameof(GetOldestPeriodStartAsync), meterNumber, result);
            return result;
        }
        finally
        {
            syncLock.Release();
            logger.LogInformation("Exit {Method} for meter {MeterNumber}", nameof(GetOldestPeriodStartAsync), meterNumber);
        }
    }

    public async Task DeleteBeforeDateAsync(string meterNumber, DateTime date, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Enter {Method} for meter {MeterNumber}, deleting readings after {Date:yyyy-MM-dd}", nameof(DeleteBeforeDateAsync), meterNumber, date);
        await syncLock.WaitAsync(cancellationToken);
        try
        {
            await using var context = contextFactory.CreateDbContext();
            var deleted = await context.MeterReadings
                .Where(w => w.MeterNumber == meterNumber && w.PeriodStart > date)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation("{Method}: deleted {Count} readings for meter {MeterNumber}", nameof(DeleteBeforeDateAsync), deleted, meterNumber);
        }
        finally
        {
            syncLock.Release();
            logger.LogInformation("Exit {Method} for meter {MeterNumber}", nameof(DeleteBeforeDateAsync), meterNumber);
        }
    }
}
