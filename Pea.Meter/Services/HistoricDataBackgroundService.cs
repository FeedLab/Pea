using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;

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
    private readonly StorageService storageService;
    private CancellationTokenSource? cancellationTokenSource;
    private Task? runningTask;

    public HistoricDataBackgroundService(PeaAdapter peaAdapter, ILogger<HistoricDataBackgroundService> logger,
        PeaDbContextFactory dbContextFactory,
        StorageService storageService)
    {
        this.peaAdapter = peaAdapter;
        this.logger = logger;
        this.dbContextFactory = dbContextFactory;
        this.storageService = storageService;

        cancellationTokenSource = new CancellationTokenSource();

        WeakReferenceMessenger.Default.Register<AllAggregationsCompletedMessage>(this,
            async void (r, m) => { TriggerImportTask(); });
    }

    private void TriggerImportTask()
    {
        try
        {
            cancellationTokenSource = new CancellationTokenSource();

            if (runningTask != null && !runningTask.IsCompleted)
            {
                logger.LogWarning("Import is already running, ignoring trigger request.");
                return;
            }

            logger.LogInformation("Import triggered by user login for user: {UserId}", "N/A");

            runningTask = Task.Run(async () => await ImportHistoricDataAsync(cancellationTokenSource.Token));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in {Method}: {Message}", "Import", e.Message);
        }
    }

    /// <summary>
    /// Triggers the background import to start
    /// </summary>
    public void TriggerImport()
    {
        CancelImport();
        TriggerImportTask();
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
        // Create database context and repository
        using var dbContext = dbContextFactory.CreateDbContext();
        var repository = new MeterReadingRepository(dbContext);

        var startDate = DateTime.Now.Date.AddDays(-1);
        var startDateOldest = await GetOldestPeriodStartAsync(cancellationToken, repository);
        var maxDaysToTry = 365 * 2; // Safety limit: don't go back more than 2 years
        var oldestDateToImport = startDate.AddDays(-maxDaysToTry);
        logger.LogInformation(
            "Starting historic data import from {Date} for user {UserId}, will stop when ShowDailyReadings returns 0 items",
            startDate, "N/A");

        var totalReadings = 0;
        var targetDate = startDate;

        do
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Import cancelled by user.");
                break;
            }

            var historyLengthTs = DateTime.Now.Date - targetDate;
            if (historyLengthTs.TotalDays > maxDaysToTry)
            {
                logger.LogWarning(
                    "Reached maximum history length of {MaxDays} days. Stopping import to avoid excessive data fetching.",
                    maxDaysToTry);

                break;
            }

            try
            {
                // Check if data already exists for this date in the database
                // var hasExistingData = await repository.HasReadingsForDateAsync(targetDate, cancellationToken);
                if (storageService.DailyAggregated.Any(r => r.PeriodStart == targetDate))
                {
                    logger.LogInformation("Data already exists for {Date}, skipping",
                        targetDate.ToString("yyyy-MM-dd"));

                    logger.LogInformation("Skipping over to date {Date}", startDateOldest.ToString("yyyy-MM-dd"));
                    targetDate = startDateOldest;
                    continue;
                }

                logger.LogInformation("Fetching data for {Date}...", targetDate.ToString("yyyy-MM-dd"));
                var readings = await peaAdapter.ShowDailyReadings(targetDate);

                if (readings.Count > 0 && readings.Sum(s => s.Total) > 0)
                {
                    // Save to database
                    await repository.AddRangeAsync(readings, cancellationToken);
                    totalReadings += readings.Count;
                    logger.LogInformation("Successfully imported and saved {Count} readings for {Date}", readings.Count,
                        targetDate.ToString("yyyy-MM-dd"));

                    WeakReferenceMessenger.Default.Send(new DataImportedMessage(readings, targetDate));
                }
                else
                {
                    logger.LogInformation(
                        "No readings found for {Date}. Stopping import as there is no more data to read.",
                        targetDate.ToString("yyyy-MM-dd"));
                    break; // Stop when ShowDailyReadings returns 0 items or the total is 0
                }

                // Add a small delay between requests to avoid overwhelming the server
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error importing data for {Date}", targetDate.ToString("yyyy-MM-dd"));
            }

            targetDate = targetDate.AddDays(-1);
        } while (targetDate > oldestDateToImport);

        logger.LogInformation("Import completed. Total readings imported and saved: {Total}", totalReadings);
    }

    private static async Task<DateTime> GetOldestPeriodStartAsync(CancellationToken cancellationToken,
        MeterReadingRepository repository)
    {
        var date = await repository.GetOldestPeriodStartAsync(cancellationToken);

        return date.AddDays(-1);
    }
}

internal partial class DataImportedMessage : ObservableObject
{
    [ObservableProperty] private IList<PeaMeterReading> readings;
    [ObservableProperty] private DateTime date;

    public DataImportedMessage(IList<PeaMeterReading> readings, DateTime date)
    {
        Readings = readings;
        Date = date;
    }
}