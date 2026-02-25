using Pea.Infrastructure.Models;

namespace Pea.Meter.Services;

/// <summary>
/// Service for importing historic meter reading data from PEA AMR system
/// </summary>
public class HistoricDataImportService
{
    private readonly PeaAdapter _peaAdapter;

    public HistoricDataImportService(PeaAdapter peaAdapter)
    {
        _peaAdapter = peaAdapter;
    }

    /// <summary>
    /// Imports historic daily readings starting from yesterday and going back specified number of days
    /// </summary>
    /// <param name="numberOfDays">Number of days to import (default: 7)</param>
    /// <returns>Dictionary with date as key and list of meter readings as value</returns>
    public async Task<Dictionary<DateTime, IList<PeaMeterReading>>> ImportHistoricDataAsync(int numberOfDays = 7)
    {
        var historicData = new Dictionary<DateTime, IList<PeaMeterReading>>();
        var startDate = DateTime.Today.AddDays(-1); // Yesterday

        Console.WriteLine($"Starting historic data import for {numberOfDays} days from {startDate:yyyy-MM-dd}");

        for (int i = 0; i < numberOfDays; i++)
        {
            var targetDate = startDate.AddDays(-i);
            
            try
            {
                Console.WriteLine($"Fetching data for {targetDate:yyyy-MM-dd}...");
                var readings = await _peaAdapter.ShowDailyReadings(targetDate);
                
                historicData[targetDate] = readings;
                Console.WriteLine($"Successfully imported {readings.Count} readings for {targetDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing data for {targetDate:yyyy-MM-dd}: {ex.Message}");
            }
        }

        return historicData;
    }

    /// <summary>
    /// Imports historic data for a specific date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>Dictionary with date as key and list of meter readings as value</returns>
    public async Task<Dictionary<DateTime, IList<PeaMeterReading>>> ImportHistoricDataRangeAsync(DateTime startDate, DateTime endDate)
    {
        var historicData = new Dictionary<DateTime, IList<PeaMeterReading>>();
        var currentDate = startDate.Date;
        var finalDate = endDate.Date;

        Console.WriteLine($"Starting historic data import from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

        while (currentDate <= finalDate)
        {
            try
            {
                Console.WriteLine($"Fetching data for {currentDate:yyyy-MM-dd}...");
                var readings = await _peaAdapter.ShowDailyReadings(currentDate);
                
                historicData[currentDate] = readings;
                Console.WriteLine($"Successfully imported {readings.Count} readings for {currentDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing data for {currentDate:yyyy-MM-dd}: {ex.Message}");
            }

            currentDate = currentDate.AddDays(1);
        }

        return historicData;
    }
}
