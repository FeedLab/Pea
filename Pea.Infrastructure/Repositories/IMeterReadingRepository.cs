using Pea.Infrastructure.Models;

namespace Pea.Infrastructure.Repositories;

/// <summary>
/// Repository interface for meter readings
/// </summary>
public interface IMeterReadingRepository
{
    /// <summary>
    /// Adds a collection of meter readings
    /// </summary>
    Task AddRangeAsync(IEnumerable<PeaMeterReading> readings, CancellationToken cancellationToken = default);


    Task<IList<PeaMeterReading>> GetAllMeterReadingsAsync(CancellationToken cancellationToken = default);

    Task DeleteAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if readings exist for a specific date
    /// </summary>
    Task<bool> HasReadingsForDateAsync(DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the oldest period start date
    /// </summary>
    Task<DateTime> GetOldestPeriodStartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all readings before a specific date
    /// </summary>
    /// <param name="date"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteBeforeDateAsync(DateTime date, CancellationToken cancellationToken = default);

}