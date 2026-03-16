namespace Pea.Infrastructure.Models.MeterData;

public class MeterDataManagerMonth : MeterDataManagerBase<MeterDataManagerDay>
{
    public MeterDataManagerMonth(decimal flatRatePrice, decimal peekPrice, decimal offPeekPrice)
        : base(flatRatePrice, peekPrice, offPeekPrice)
    {
    }

    public List<MeterDataReading> GetReadings(DateTime date, FilterLevel filterLevel)
    {
        if (filterLevel == FilterLevel.Month)
            return MeterReadings;

        if (filterLevel > FilterLevel.Month)
        {
            var existsKey = DataBucket.ContainsKey(date.Day);

            if (existsKey)
            {
                return DataBucket[date.Day].GetReadings(date, filterLevel);
            }
        }

        return MeterReadings;
    }

    public void AddRange(List<MeterDataReading> readings)
    {
        MeterReadings.AddRange(readings);
        Add(readings);
    }

    public void Clear()
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
        var groups = readings.GroupBy(r => new
            { Year = r.PeriodStart.Year, Month = r.PeriodStart.Month, r.PeriodStart.Day });

        foreach (var group in groups)
        {
            if (!DataBucket.ContainsKey(group.Key.Day))
            {
                DataBucket[group.Key.Day] = new MeterDataManagerDay(FlatRatePrice, PeekPrice, OffPeekPrice);
            }

            DataBucket[group.Key.Day].AddRange(group.ToList());
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