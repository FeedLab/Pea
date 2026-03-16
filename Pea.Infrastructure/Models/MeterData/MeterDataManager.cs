namespace Pea.Infrastructure.Models.MeterData;

public enum FilterLevel
{
    None,
    Year,
    Month,
    Day,
    Hour,
    Quarter
}

public class MeterDataManager : MeterDataManagerBase<MeterDataManagerYear>
{

    public MeterDataManager(List<MeterDataReading> meterReadings, decimal flatRatePrice, decimal peekPrice, decimal offPeekPrice)
    : base(flatRatePrice, peekPrice, offPeekPrice)
    {
        AddRange(meterReadings);
    }

    public List<MeterDataReading> GetReadings(DateTime date, FilterLevel filterLevel = FilterLevel.None)
    {
        if (filterLevel == FilterLevel.None)
        {
            return MeterReadings;
        }

        if (filterLevel >= FilterLevel.Year)
        {
            var existsKey = DataBucket.ContainsKey(date.Year);

            if (existsKey)
            {
                return DataBucket[date.Year].GetReadings(date, filterLevel);
            }
        }

        return MeterReadings;
    }
    
    public void AddRange(List<MeterDataReading> readings)
    {
        MeterReadings.AddRange(readings);

        var groups = readings.GroupBy(r => r.PeriodStart.Year);

        foreach (var group in groups)
        {
            if (!DataBucket.ContainsKey(group.Key))
            {
                DataBucket[group.Key] = new MeterDataManagerYear( FlatRatePrice, PeekPrice, OffPeekPrice);
            }

            DataBucket[group.Key].AddRange(group.ToList());
        }
        
        CalculateMeterDataUsageSummary();
        CalculateUsagePriceSummaries();
    }

    private void CalculateUsagePriceSummaries()
    {
        MeterDataUsageInMoneySummary.PeekTouUsagePriceSummary = DataBucket.Sum(s => s.Value.MeterDataUsageInMoneySummary.PeekTouUsagePriceSummary);
        MeterDataUsageInMoneySummary.OffPeekTouUsagePriceSummary = DataBucket.Sum(s => s.Value.MeterDataUsageInMoneySummary.OffPeekTouUsagePriceSummary);
    }

    private void CalculateMeterDataUsageSummary()
    {
        MeterDataUsageInKwSummary.PeekUsage = DataBucket.Sum(s => s.Value.MeterDataUsageInKwSummary.PeekUsage);
        MeterDataUsageInKwSummary.OffPeekUsage = DataBucket.Sum(s => s.Value.MeterDataUsageInKwSummary.OffPeekUsage);
        MeterDataUsageInKwSummary.Holiday = DataBucket.Sum(s => s.Value.MeterDataUsageInKwSummary.Holiday);
    }

    public void Clear()
    {
        foreach (var meterData in DataBucket.Values)
        {
            meterData.Clear();
        }

        MeterReadings.Clear();
        DataBucket.Clear();
    }
}