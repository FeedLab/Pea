using Microsoft.Extensions.Logging;
using Pea.Meter.Models;

namespace Pea.Meter.Services;

/// <summary>
/// Background service that imports historic meter reading data from PEA AMR system
/// Triggered manually after user login
/// </summary>
public class HistoricDataBackgroundService
{
    private readonly PeaAdapter _peaAdapter;
    private readonly ILogger<HistoricDataBackgroundService> _logger;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _runningTask;

    public HistoricDataBackgroundService(PeaAdapter peaAdapter, ILogger<HistoricDataBackgroundService> logger)
    {
        _peaAdapter = peaAdapter;
        _logger = logger;
    }

    /// <summary>
    /// Triggers the background import to start
    /// </summary>
    public void TriggerImport()
    {
        if (_runningTask != null && !_runningTask.IsCompleted)
        {
            _logger.LogWarning("Import is already running, ignoring trigger request.");
            return;
        }

        _logger.LogInformation("Import triggered by user login.");

        _cancellationTokenSource = new CancellationTokenSource();
        _runningTask = Task.Run(async () => await ImportHistoricDataAsync(_cancellationTokenSource.Token));
    }

    /// <summary>
    /// Cancels the running import if any
    /// </summary>
    public void CancelImport()
    {
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            _logger.LogInformation("Cancelling historic data import.");
            _cancellationTokenSource.Cancel();
        }
    }

    private async Task ImportHistoricDataAsync(CancellationToken cancellationToken)
    {
        var numberOfDays = 7;
        var startDate = DateTime.Today.AddDays(-1); // Yesterday

        _logger.LogInformation("Starting historic data import for {Days} days from {Date}", numberOfDays, startDate);

        var allReadings = new List<PeaMeterReading>();

        for (int i = 0; i < numberOfDays; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Import cancelled by user.");
                break;
            }

            var targetDate = startDate.AddDays(-i);

            try
            {
                _logger.LogInformation("Fetching data for {Date}...", targetDate.ToString("yyyy-MM-dd"));
                var readings = await _peaAdapter.ShowDailyReadings(targetDate);

                allReadings.AddRange(readings);
                _logger.LogInformation("Successfully imported {Count} readings for {Date}", readings.Count, targetDate.ToString("yyyy-MM-dd"));

                // Add small delay between requests to avoid overwhelming the server
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing data for {Date}", targetDate.ToString("yyyy-MM-dd"));
            }
        }

        _logger.LogInformation("Import completed. Total readings imported: {Total}", allReadings.Count);

        // TODO: Store the data in a database or file as needed
        // For now, the data is just logged
    }
}
