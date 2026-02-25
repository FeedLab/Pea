using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Repositories;

namespace Pea.Meter.Services;

/// <summary>
/// Background service that imports historic meter reading data from PEA AMR system
/// Triggered manually after user login
/// </summary>
public class HistoricDataBackgroundService
{
    private readonly PeaAdapter peaAdapter;
    private readonly ILogger<HistoricDataBackgroundService> logger;
    private readonly PeaDbContextFactory dbContextFactory;
    private CancellationTokenSource? cancellationTokenSource;
    private Task? runningTask;
    private string? currentUserId;

    public HistoricDataBackgroundService(PeaAdapter peaAdapter, ILogger<HistoricDataBackgroundService> logger, PeaDbContextFactory dbContextFactory)
    {
        this.peaAdapter = peaAdapter;
        this.logger = logger;
        this.dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Triggers the background import to start
    /// </summary>
    public void TriggerImport(string userId)
    {
        if (runningTask != null && !runningTask.IsCompleted)
        {
            logger.LogWarning("Import is already running, ignoring trigger request.");
            return;
        }

        logger.LogInformation("Import triggered by user login for user: {UserId}", userId);

        currentUserId = userId;
        cancellationTokenSource = new CancellationTokenSource();
        runningTask = Task.Run(async () => await ImportHistoricDataAsync(cancellationTokenSource.Token));
    }

    /// <summary>
    /// Cancels the running import if any
    /// </summary>
    public void CancelImport()
    {
        if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
        {
            logger.LogInformation("Cancelling historic data import.");
            cancellationTokenSource.Cancel();
        }
    }

    private async Task ImportHistoricDataAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(currentUserId))
        {
            logger.LogError("Cannot import data: User ID is not set");
            return;
        }

        var startDate = DateTime.Today.AddDays(-1); // Yesterday
        var maxDaysToTry = 365 * 10; // Safety limit: don't go back more than 10 years

        logger.LogInformation("Starting historic data import from {Date} for user {UserId}, will stop when ShowDailyReadings returns 0 items", startDate, currentUserId);

        // Create user-specific database context and repository
        using var dbContext = dbContextFactory.CreateDbContext(currentUserId);
        var repository = new MeterReadingRepository(dbContext);

        var totalReadings = 0;

        for (int i = 0; i < maxDaysToTry; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Import cancelled by user.");
                break;
            }

            var targetDate = startDate.AddDays(-i);

            try
            {
                // Check if data already exists for this date
                if (await repository.ExistsForDateAsync(targetDate, currentUserId, cancellationToken))
                {
                    logger.LogInformation("Data already exists for {Date}, skipping", targetDate.ToString("yyyy-MM-dd"));
                    continue;
                }

                logger.LogInformation("Fetching data for {Date}...", targetDate.ToString("yyyy-MM-dd"));
                var readings = await peaAdapter.ShowDailyReadings(targetDate);

                if (readings.Count > 0)
                {
                    // Save to database
                    await repository.AddRangeAsync(readings, currentUserId, cancellationToken);
                    totalReadings += readings.Count;
                    logger.LogInformation("Successfully imported and saved {Count} readings for {Date}", readings.Count, targetDate.ToString("yyyy-MM-dd"));
                }
                else
                {
                    logger.LogInformation("No readings found for {Date}. Stopping import as there is no more data to read.", targetDate.ToString("yyyy-MM-dd"));
                    break; // Stop when ShowDailyReadings returns 0 items
                }

                // Add small delay between requests to avoid overwhelming the server
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error importing data for {Date}", targetDate.ToString("yyyy-MM-dd"));
            }
        }

        logger.LogInformation("Import completed. Total readings imported and saved: {Total}", totalReadings);
    }
}
