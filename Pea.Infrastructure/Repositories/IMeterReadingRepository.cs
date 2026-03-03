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
    
    }
