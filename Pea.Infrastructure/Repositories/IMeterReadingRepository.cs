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
    Task AddRangeAsync(IEnumerable<PeaMeterReading> readings, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets meter readings for a specific date range
    /// </summary>
    Task<IList<PeaMeterReading>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets meter readings for a specific date
    /// </summary>
    Task<IList<PeaMeterReading>> GetByDateAsync(DateTime date, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if readings exist for a specific date
    /// </summary>
    Task<bool> ExistsForDateAsync(DateTime date, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all readings for a specific user
    /// </summary>
    Task DeleteAllAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest reading date for a user
    /// </summary>
    Task<DateTime?> GetLatestReadingDateAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates the average consumption for each 15-minute reading period over a given number of days from today going back in time
    /// </summary>
    /// <param name="periodInDays">Number of days to go back from today</param>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of average readings by time of day (96 readings per day)</returns>
    Task<IList<PeaMeterReading>> GetAverageReadingsByTimeOfDayAsync(int periodInDays, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total aggregated per hour for readings between start and end time
    /// </summary>
    /// <param name="startTime">Start date/time</param>
    /// <param name="endTime">End date/time</param>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of readings with hourly totals</returns>
    Task<IList<PeaMeterReading>> GetHourlyTotalsAsync(DateTime startTime, DateTime endTime, string userId, CancellationToken cancellationToken = default);

    Task<IList<PeaMeterReading>> GetDailyTotalsAsync(DateTime startTime, DateTime endTime, string userId,
        CancellationToken cancellationToken = default);
    
    
    /// <summary>
    /// Gets average per hour for readings between start and end time
    /// </summary>
    /// <param name="startTime">Start date/time</param>
    /// <param name="endTime">End date/time</param>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of readings with hourly averages</returns>
    Task<IList<PeaMeterReading>> GetHourlyAveragesDuringPeriodAsync(DateTime startTime, DateTime endTime, string userId, CancellationToken cancellationToken = default);
}
