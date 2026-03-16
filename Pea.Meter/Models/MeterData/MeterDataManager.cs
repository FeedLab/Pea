using Pea.Infrastructure.Models;

namespace Pea.Meter.Models.MeterData;

public enum FilterLevel
{
    None,
    Year,
    Month,
    Day,
    Hour,
    Quarter
}

public class MeterDataManager
{
    private List<PeaMeterReading> meterReadings;
    private Dictionary<int, MeterDataManagerYear> datas = [];

    public MeterDataManager()
    {
        meterReadings = new List<PeaMeterReading>();
    }

    public List<PeaMeterReading> GetReadings(DateTime date, FilterLevel filterLevel = FilterLevel.None)
    {
        if (filterLevel == FilterLevel.None)
        {
            return meterReadings;
        }

        if (filterLevel >= FilterLevel.Year)
        {
            var existsKey = datas.ContainsKey(date.Year);

            if (existsKey)
            {
                return datas[date.Year].GetReadings(date, filterLevel);
            }
        }

        return meterReadings;
    }
    
    public void AddRange(List<PeaMeterReading> readings)
    {
        meterReadings.AddRange(readings);

        var groups = readings.GroupBy(r => r.PeriodStart.Year);

        foreach (var group in groups)
        {
            if (!datas.ContainsKey(group.Key))
            {
                datas[group.Key] = new MeterDataManagerYear();
            }

            datas[group.Key].AddRange(group.ToList());
        }
    }

    public void Clear()
    {
        foreach (var meterData in datas.Values)
        {
            meterData.Clear();
        }

        meterReadings.Clear();
        datas.Clear();
    }
}

internal class MeterDataManagerYear
{
    private List<PeaMeterReading> meterReadings = [];
    private Dictionary<int, MeterDataManagerMonth> datas = [];

    internal MeterDataManagerYear()
    {
    }

    public List<PeaMeterReading> GetReadings(DateTime date, FilterLevel filterLevel)
    {
        if (filterLevel == FilterLevel.Year)
            return meterReadings;

        if (filterLevel > FilterLevel.Year)
        {
            var existsKey = datas.ContainsKey(date.Month);

            if (existsKey)
            {
                return datas[date.Month].GetReadings(date, filterLevel);
            }
        }

        return meterReadings;
    }
    
    internal void AddRange(List<PeaMeterReading> readings)
    {
        meterReadings.AddRange(readings);
        Add(readings);
    }

    internal void Clear()
    {
        foreach (var meterData in datas.Values)
        {
            meterData.Clear();
        }

        meterReadings.Clear();
        datas.Clear();
    }

    private void Add(List<PeaMeterReading> readings)
    {
        var groups = readings.GroupBy(r => new { Year = r.PeriodStart.Year, Month = r.PeriodStart.Month });

        foreach (var group in groups)
        {
            if (!datas.ContainsKey(group.Key.Month))
            {
                datas[group.Key.Month] = new MeterDataManagerMonth();
            }

            datas[group.Key.Month].AddRange(group.ToList());
        }
    }
}

internal class MeterDataManagerMonth
{
    private List<PeaMeterReading> meterReadings = [];
    private Dictionary<int, MeterDataManagerDay> datas = [];

    internal MeterDataManagerMonth()
    {
    }

    public List<PeaMeterReading> GetReadings(DateTime date, FilterLevel filterLevel)
    {
        if (filterLevel == FilterLevel.Month)
            return meterReadings;

        if (filterLevel > FilterLevel.Month)
        {
            var existsKey = datas.ContainsKey(date.Day);

            if (existsKey)
            {
                return datas[date.Day].GetReadings(date, filterLevel);
            }
        }

        return meterReadings;
    }
    
    internal void AddRange(List<PeaMeterReading> readings)
    {
        meterReadings.AddRange(readings);
        Add(readings);
    }

    internal void Clear()
    {
        foreach (var meterData in datas.Values)
        {
            meterData.Clear();
        }

        meterReadings.Clear();
        datas.Clear();
    }

    private void Add(List<PeaMeterReading> readings)
    {
        var groups = readings.GroupBy(r => new
            { Year = r.PeriodStart.Year, Month = r.PeriodStart.Month, r.PeriodStart.Day });

        foreach (var group in groups)
        {
            if (!datas.ContainsKey(group.Key.Day))
            {
                datas[group.Key.Day] = new MeterDataManagerDay();
            }

            datas[group.Key.Day].AddRange(group.ToList());
        }
    }
}

internal class MeterDataManagerDay
{
    private List<PeaMeterReading> meterReadings = [];
    private Dictionary<int, MeterDataManagerHour> datas = [];

    internal MeterDataManagerDay()
    {
    }

    public List<PeaMeterReading> GetReadings(DateTime date, FilterLevel filterLevel)
    {
        if (filterLevel == FilterLevel.Day)
            return meterReadings;

        if (filterLevel > FilterLevel.Day)
        {
            var existsKey = datas.ContainsKey(date.Hour);

            if (existsKey)
            {
                return datas[date.Hour].GetReadings(date, filterLevel);
            }
        }

        return meterReadings;
    }
    
    internal void AddRange(List<PeaMeterReading> readings)
    {
        meterReadings.AddRange(readings);
        Add(readings);
    }

    internal void Clear()
    {
        foreach (var meterData in datas.Values)
        {
            meterData.Clear();
        }

        meterReadings.Clear();
        datas.Clear();
    }

    private void Add(List<PeaMeterReading> readings)
    {
        var groups = readings.GroupBy(r => new
            { Year = r.PeriodStart.Year, Month = r.PeriodStart.Month, r.PeriodStart.Day, r.PeriodStart.Hour });

        foreach (var group in groups)
        {
            if (!datas.ContainsKey(group.Key.Hour))
            {
                datas[group.Key.Hour] = new MeterDataManagerHour();
            }

            datas[group.Key.Hour].AddRange(group.ToList());
        }
    }
}

internal class MeterDataManagerHour
{
    private List<PeaMeterReading> meterReadings = [];
    private Dictionary<int, MeterDataManagerQuarter> datas = [];

    internal MeterDataManagerHour()
    {
    }
    
    public List<PeaMeterReading> GetReadings(DateTime date, FilterLevel filterLevel)
    {
        if (filterLevel == FilterLevel.Hour)
            return meterReadings;

        if (filterLevel == FilterLevel.Quarter)
        {
            var quarter = date.Minute / 15;
            var existsKey = datas.ContainsKey(quarter);

            if (existsKey)
            {
                return datas[quarter].GetReadings();
            }
        }

        return meterReadings;
    }
    
    internal void AddRange(List<PeaMeterReading> readings)
    {
        meterReadings.AddRange(readings);
        Add(readings);
    }

    internal void Clear()
    {
        foreach (var meterData in datas.Values)
        {
            meterData.Clear();
        }

        meterReadings.Clear();
        datas.Clear();
    }

    private void Add(List<PeaMeterReading> readings)
    {
        var groups = readings.GroupBy(r => new
        {
            Year = r.PeriodStart.Year,
            Month = r.PeriodStart.Month,
            r.PeriodStart.Day,
            r.PeriodStart.Hour,
            Quarter = r.PeriodStart.Minute / 15  
        });

        foreach (var group in groups)
        {
            if (!datas.ContainsKey(group.Key.Quarter))
            {
                datas[group.Key.Quarter] = new MeterDataManagerQuarter();
            }

            datas[group.Key.Quarter].AddRange(group.ToList());
        }
    }
}

internal class MeterDataManagerQuarter
{
    private List<PeaMeterReading> meterReadings = [];

    internal MeterDataManagerQuarter()
    {
    }

    public List<PeaMeterReading> GetReadings()
    {
        return meterReadings;
    }
    
    internal void AddRange(List<PeaMeterReading> readings)
    {
        meterReadings.AddRange(readings);
    }

    internal void Clear()
    {
        meterReadings.Clear();
    }
}