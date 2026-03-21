namespace Pea.Infrastructure.Models.MeterData;

public class MeterDataManagerHour : MeterDataManagerBase<MeterDataManagerQuarter>
{
    public MeterDataManagerHour(decimal flatRatePrice, decimal peekPrice,
        decimal offPeekPrice)
        : base(flatRatePrice, peekPrice, offPeekPrice)
    {
        TimeResolution = FilterLevel.Hour;
    }
    
    public void CalculateSolarProduction()
    {
        foreach (var meterData in DataBucket.Values)
        {
        }
    }
    
    public List<MeterDataReading> GetReadings(DateTime date, FilterLevel filterLevel)
    {
        if (filterLevel == TimeResolution)
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

        MeterDataUsageInKw.Reset();
        MeterDataUsageInMoney.Reset();
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

        
        foreach (var group in groups)
        {
            if (!DataBucket.ContainsKey(group.Key.Quarter))
            {
                DataBucket[group.Key.Quarter] = new MeterDataManagerQuarter(FlatRatePrice, PeekPrice, OffPeekPrice);
            }

            DataBucket[group.Key.Quarter].AddRange(group.ToList());
            
        }
        MeterDataUsageInKw.Reset();
        MeterDataUsageInMoney.Reset();

        CalculateMeterDataUsageSummary();
        CalculateUsagePriceSummaries();
    }

    private void CalculateUsagePriceSummaries()
    {
        MeterDataUsageInMoney.PeekTouUsagePriceSummary =
            DataBucket.Sum(s => s.Value.MeterDataUsageInMoney.PeekTouUsagePriceSummary);
        MeterDataUsageInMoney.OffPeekTouUsagePriceSummary =
            DataBucket.Sum(s => s.Value.MeterDataUsageInMoney.OffPeekTouUsagePriceSummary);
        
        MeterDataUsageInMoney.FlatRateUsagePriceSummary =
            DataBucket.Sum(s => s.Value.MeterDataUsageInMoney.FlatRateUsagePriceSummary); 
    }

    private void CalculateMeterDataUsageSummary()
    {
        MeterDataUsageInKw.PeekUsage = DataBucket.Sum(s => s.Value.MeterDataUsageInKw.PeekUsage);
        MeterDataUsageInKw.OffPeekUsage = DataBucket.Sum(s => s.Value.MeterDataUsageInKw.OffPeekUsage);
        MeterDataUsageInKw.Holiday = DataBucket.Sum(s => s.Value.MeterDataUsageInKw.Holiday);
    }
}