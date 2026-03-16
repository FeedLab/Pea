namespace Pea.Infrastructure.Models.MeterData;

public class MeterDataManagerDay : MeterDataManagerBase<MeterDataManagerHour>
{
    public MeterDataManagerDay(decimal flatRatePrice, decimal peekPrice,
        decimal offPeekPrice)
        : base(flatRatePrice, peekPrice, offPeekPrice)
    {
    }

    public List<MeterDataReading> GetReadings(DateTime date, FilterLevel filterLevel)
    {
        if (filterLevel == FilterLevel.Day)
            return MeterReadings;

        if (filterLevel > FilterLevel.Day)
        {
            var existsKey = DataBucket.ContainsKey(date.Hour);

            if (existsKey)
            {
                return DataBucket[date.Hour].GetReadings(date, filterLevel);
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
    }

    private void Add(List<MeterDataReading> readings)
    {
        var groups = readings.GroupBy(r => new
            { Year = r.PeriodStart.Year, Month = r.PeriodStart.Month, r.PeriodStart.Day, r.PeriodStart.Hour });

        foreach (var group in groups)
        {
            if (!DataBucket.ContainsKey(group.Key.Hour))
            {
                DataBucket[group.Key.Hour] = new MeterDataManagerHour( FlatRatePrice, PeekPrice, OffPeekPrice);
            }

            DataBucket[group.Key.Hour].AddRange(group.ToList());
        }

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