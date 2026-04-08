using Pea.Infrastructure.Models;

namespace Pea.Infrastructure.Repositories;

public interface IMeterReadingRepository
{
    Task AddRangeAsync(IEnumerable<PeaMeterReading> readings, string meterNumber,
        CancellationToken cancellationToken = default);

    Task AddRangeUpsertAsync(IEnumerable<PeaMeterReading> readings, string meterNumber,
        CancellationToken cancellationToken = default);

    Task<IList<PeaMeterReading>> GetAllMeterReadingsAsync(string meterNumber,
        CancellationToken cancellationToken = default);

    Task DeleteAllAsync(string meterNumber, CancellationToken cancellationToken = default);

    Task<bool> HasReadingsForDateAsync(string meterNumber, DateTime date,
        CancellationToken cancellationToken = default);

    Task<DateTime?> GetOldestPeriodStartAsync(string meterNumber,
        CancellationToken cancellationToken = default);

    Task DeleteBeforeDateAsync(string meterNumber, DateTime date, CancellationToken cancellationToken = default);
}