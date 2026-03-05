using System.Collections;
using System.Collections.ObjectModel;
using ObservableCollections;
using Pea.Infrastructure.Models;

namespace Pea.Meter.Extension;

public static class ObservableCollectionExtensions
{
    public static ObservableCollection<PeaMeterReading> FilterByPeriod(
        this ObservableCollection<PeaMeterReading> source,
        DateTime startDate,
        DateTime endDate)
    {
        var readings = source
            .Where(m => m.PeriodStart >= startDate && m.PeriodStart < endDate)
            .ToList();

        return new ObservableCollection<PeaMeterReading>(readings);
    }

    public static ObservableCollection<PeaMeterReading> AverageByHour(
        this IEnumerable<PeaMeterReading> source)
    {
        var readings = source.ToList();
        
        if (readings.Count == 0)
        {
            return new ObservableCollection<PeaMeterReading>();
        }

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

        return new ObservableCollection<PeaMeterReading>(averageReadings);
    }
    
    public static ObservableCollection<PeaMeterReading> AverageBy15MinutesPeriod(
        this IEnumerable<PeaMeterReading> source)
    {
        var readings = source.ToList();
        
        if (readings.Count == 0)
        {
            return new ObservableCollection<PeaMeterReading>();
        }

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

        return new ObservableCollection<PeaMeterReading>(averageReadings);
    }
    
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);   
        }
    }

    public static void AddRange<T>(this ObservableCollection<T> collection, List<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);   
        }
    }
}
