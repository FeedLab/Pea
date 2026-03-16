namespace Pea.Infrastructure.Models.MeterData;

public class MeterDataManagerYear : MeterDataManagerBase<MeterDataManagerMonth>
{
    public MeterDataManagerYear(decimal flatRatePrice, decimal peekPrice, decimal offPeekPrice)
        : base(flatRatePrice, peekPrice, offPeekPrice)
    {
    }

    public List<MeterDataReading> GetReadings(DateTime date, FilterLevel filterLevel)
    {
        if (filterLevel == FilterLevel.Year)
            return MeterReadings;

        if (filterLevel > FilterLevel.Year)
        {
            var existsKey = DataBucket.ContainsKey(date.Month);

            if (existsKey)
            {
                return DataBucket[date.Month].GetReadings(date, filterLevel);
            }
        }

        return MeterReadings;
    }

    internal void AddRange(List<MeterDataReading> readings)
    {
        MeterReadings.AddRange(readings);
        Add(readings);
    }

    internal void Clear()
    {
        foreach (var meterData in DataBucket.Values)
        {
            meterData.Clear();
        }

        MeterReadings.Clear();
        DataBucket.Clear();

        MeterDataUsageInKwSummary.Reset();
        MeterDataUsageInMoneySummary.Reset();
    }

    private void Add(List<MeterDataReading> readings)
    {
        var groups = readings.GroupBy(r => new { Year = r.PeriodStart.Year, Month = r.PeriodStart.Month });

        foreach (var group in groups)
        {
            if (!DataBucket.ContainsKey(group.Key.Month))
            {
                DataBucket[group.Key.Month] = new MeterDataManagerMonth(FlatRatePrice, PeekPrice, OffPeekPrice);
            }

            DataBucket[group.Key.Month].AddRange(group.ToList());
        }
        
        MeterDataUsageInKwSummary.Reset();
        MeterDataUsageInMoneySummary.Reset();
        
        CalculateMeterDataUsageSummary();
        CalculateUsagePriceSummaries();
    }

    private void CalculateUsagePriceSummaries()
    {
        MeterDataUsageInMoneySummary.PeekTouUsagePriceSummary =
            DataBucket.Sum(s => s.Value.MeterDataUsageInMoneySummary.PeekTouUsagePriceSummary);
        MeterDataUsageInMoneySummary.OffPeekTouUsagePriceSummary =
            DataBucket.Sum(s => s.Value.MeterDataUsageInMoneySummary.OffPeekTouUsagePriceSummary);
    }

    private void CalculateMeterDataUsageSummary()
    {
        MeterDataUsageInKwSummary.PeekUsage = DataBucket.Sum(s => s.Value.MeterDataUsageInKwSummary.PeekUsage);
        MeterDataUsageInKwSummary.OffPeekUsage = DataBucket.Sum(s => s.Value.MeterDataUsageInKwSummary.OffPeekUsage);
        MeterDataUsageInKwSummary.Holiday = DataBucket.Sum(s => s.Value.MeterDataUsageInKwSummary.Holiday);
    }
}