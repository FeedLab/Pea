namespace Pea.Infrastructure.Models.MeterData;

public class MeterDataManagerHour : MeterDataManagerBase<MeterDataManagerQuarter>
{
    public MeterDataManagerHour(decimal flatRatePrice, decimal peekPrice,
        decimal offPeekPrice)
        : base(flatRatePrice, peekPrice, offPeekPrice)
    {
    }
    
    public List<MeterDataReading> GetReadings(DateTime date, FilterLevel filterLevel)
    {
        if (filterLevel == FilterLevel.Hour)
            return MeterReadings;

        if (filterLevel == FilterLevel.Quarter)
        {
            var quarter = date.Minute / 15;
            var existsKey = DataBucket.ContainsKey(quarter);

            if (existsKey)
            {
                return DataBucket[quarter].GetReadings();
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
        {
            Year = r.PeriodStart.Year,
            Month = r.PeriodStart.Month,
            r.PeriodStart.Day,
            r.PeriodStart.Hour,
            Quarter = r.PeriodStart.Minute / 15  
        });

        MeterDataUsageInKwSummary.Reset();
        
        foreach (var group in groups)
        {
            if (!DataBucket.ContainsKey(group.Key.Quarter))
            {
                DataBucket[group.Key.Quarter] = new MeterDataManagerQuarter(FlatRatePrice, PeekPrice, OffPeekPrice);
            }

            DataBucket[group.Key.Quarter].AddRange(group.ToList());
            
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