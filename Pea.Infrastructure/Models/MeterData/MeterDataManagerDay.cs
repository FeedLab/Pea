namespace Pea.Infrastructure.Models.MeterData;

public class MeterDataManagerDay : MeterDataManagerBase<MeterDataManagerHour>
{
    private decimal calculatedSolarProduction;
    
    public MeterDataManagerDay(decimal flatRatePrice, decimal peekPrice,
        decimal offPeekPrice)
        : base(flatRatePrice, peekPrice, offPeekPrice)
    {
        TimeResolution = FilterLevel.Day;
    }

    public decimal GetSolarProduction(FilterLevel timeResolution)
    {
        if (timeResolution != TimeResolution)
        {
            throw new ArgumentException("Time resolution is not the same as the day level");
        }
        
        return calculatedSolarProduction;
    }
    
    public void CalculateSolarProduction(DateOnly date, decimal solarArraySize, decimal panelAzimuth, decimal panelTilt)
    {
        calculatedSolarProduction = PvCalculatorService.CalculateKwDaily(date, solarArraySize, panelTilt, panelAzimuth);
    }
    
    public List<MeterDataReading> GetReadings(DateTime date, FilterLevel filterLevel)
    {
        if (filterLevel == TimeResolution)
            return MeterReadings;

        if (filterLevel == FilterLevel.Hour)
        {
            var existsKey = DataBucket.ContainsKey(date.Hour);

            if (existsKey)
            {
                return DataBucket[date.Hour].GetReadings(date, filterLevel);
            }
        }
        else
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

        MeterDataUsageInKwSummary.Reset();
        MeterDataUsageInMoneySummary.Reset();
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