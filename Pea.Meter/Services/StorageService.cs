using CommunityToolkit.Mvvm.Messaging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;

namespace Pea.Meter.Services;

public class StorageService(PeaDbContextFactory dbContextFactory)
{
    private IList<PeaMeterReading> allMeterReadingsAsync;
    private IList<PeaMeterReading> hourlyAggregated;
    private IList<PeaMeterReading> dailyAggregated;
    private IList<PeaMeterReading> weeklyAggregated;

    public async Task Init(string userId)
    {
        var context = dbContextFactory.CreateDbContext(userId);
        var meterReadingRepository = new MeterReadingRepository(context);
        allMeterReadingsAsync = await meterReadingRepository.GetAllMeterReadingsAsync();

        // Start aggregations in background without blocking
        await Task.Run(() =>
        {
            ProcessAggregations();
            WeakReferenceMessenger.Default.Send(new AllAggregationsCompletedMessage());
        });
    }

    private void ProcessAggregations()
    {
        // Aggregate by hour
        hourlyAggregated = allMeterReadingsAsync
            .GroupBy(r => new DateTime(r.PeriodStart.Year, r.PeriodStart.Month, r.PeriodStart.Day, r.PeriodStart.Hour, 0, 0))
            .Select(g => new PeaMeterReading(
                g.Key,
                g.Sum(r => r.RateA),
                g.Sum(r => r.RateB),
                g.Sum(r => r.RateC),
                60
            ))
            .OrderBy(r => r.PeriodStart)
            .ToList();
        WeakReferenceMessenger.Default.Send(new HourlyAggregationCompletedMessage(hourlyAggregated));

        // Aggregate by day
        dailyAggregated = allMeterReadingsAsync
            .GroupBy(r => r.PeriodStart.Date)
            .Select(g => new PeaMeterReading(
                g.Key,
                g.Sum(r => r.RateA),
                g.Sum(r => r.RateB),
                g.Sum(r => r.RateC),
                60 * 24
            ))
            .OrderBy(r => r.PeriodStart)
            .ToList();
        WeakReferenceMessenger.Default.Send(new DailyAggregationCompletedMessage(dailyAggregated));

        // Aggregate by week
        weeklyAggregated = allMeterReadingsAsync
            .GroupBy(r =>
            {
                var weekStart = r.PeriodStart.Date.AddDays(-(int)r.PeriodStart.DayOfWeek);
                return weekStart;
            })
            .Select(g => new PeaMeterReading(
                g.Key,
                g.Sum(r => r.RateA),
                g.Sum(r => r.RateB),
                g.Sum(r => r.RateC),
                60 * 24 * 7
            ))
            .OrderBy(r => r.PeriodStart)
            .ToList();
        WeakReferenceMessenger.Default.Send(new WeeklyAggregationCompletedMessage(weeklyAggregated));
    }

    public IList<PeaMeterReading> FetchAverageQuarterlyReadingsForPeriodAsync(DateTime startDate, DateTime endDate)
    {
        var readings = GetQuarterlyAggregated()
            .Where(m => m.PeriodStart >= startDate && m.PeriodStart < endDate)
            .ToList();

        if (readings.Count == 0)
        {
            return new List<PeaMeterReading>();
        }

        // Group by time of day (hour and minute) and calculate average for each 15-minute slot
        var averageReadings = readings
            .GroupBy(r => new { r.PeriodStart.Hour, r.PeriodStart.Minute })
            .Select(g => new PeaMeterReading(
                new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, g.Key.Hour, g.Key.Minute, 0),
                g.Average(r => r.RateA),
                g.Average(r => r.RateB),
                g.Average(r => r.RateC)
            ))
            .OrderBy(r => r.PeriodStart)
            .ToList();
        
        return averageReadings;
    }
    
    public IList<PeaMeterReading> FetchAverageHourlyReadingsForPeriodAsync(DateTime startDate, DateTime endDate)
    {
        var readings = GetHourlyAggregated()
            .Where(m => m.PeriodStart >= startDate && m.PeriodStart < endDate)
            .ToList();

        if (readings.Count == 0)
        {
            return new List<PeaMeterReading>();
        }

        // Group by time of day (hour and minute) and calculate average for each 15-minute slot
        var averageReadings = readings
            .GroupBy(r => new { r.PeriodStart.Hour })
            .Select(g => new PeaMeterReading(
                new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, g.Key.Hour, 0, 0),
                g.Average(r => r.RateA),
                g.Average(r => r.RateB),
                g.Average(r => r.RateC)
            ))
            .OrderBy(r => r.PeriodStart)
            .ToList();
        
        return averageReadings;
    }
    
    public IList<PeaMeterReading> FetchDailyAggregatedForPeriodAsync(DateTime startDate, DateTime endDate)
    {
        var readings = GetDailyAggregated()
            .Where(m => m.PeriodStart >= startDate && m.PeriodStart < endDate)
            .ToList();
        
        return readings;
    }
    
    public IList<PeaMeterReading> GetQuarterlyAggregated() => allMeterReadingsAsync;
    public IList<PeaMeterReading> GetHourlyAggregated() => hourlyAggregated;
    public IList<PeaMeterReading> GetDailyAggregated() => dailyAggregated;
    public IList<PeaMeterReading> GetWeeklyAggregated() => weeklyAggregated;
}

public record HourlyAggregationCompletedMessage(IList<PeaMeterReading> Data);
public record DailyAggregationCompletedMessage(IList<PeaMeterReading> Data);
public record WeeklyAggregationCompletedMessage(IList<PeaMeterReading> Data);
public record AllAggregationsCompletedMessage();