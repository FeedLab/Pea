namespace Pea.Infrastructure.Models.MeterData;

public class MeterDataManagerDay : MeterDataManagerBase<MeterDataManagerHour>
{
    private decimal calculatedSolarProduction;
    public decimal CalculatedBatteryNeeded { get; private set; }
    public decimal SumKwUsedBetween08To17Monthly { get; private set; }

    public MeterDataManagerDay(DateOnly date, decimal flatRatePrice, decimal peekPrice,
        decimal offPeekPrice)
        : base(flatRatePrice, peekPrice, offPeekPrice)
    {
        TimeResolution = FilterLevel.Day;
        Date = date;
    }

    public decimal GetSolarProduction(FilterLevel timeResolution)
    {
        if (timeResolution != TimeResolution)
        {
            throw new ArgumentException("Time resolution is not the same as the day level");
        }
        
        return calculatedSolarProduction;
    }
    
    public decimal GetBatteryProduction(FilterLevel timeResolution)
    {
        if (timeResolution != TimeResolution)
        {
            throw new ArgumentException("Time resolution is not the same as the day level");
        }
        
        return CalculatedBatteryNeeded;
    }
    
    public void CalculateSolarProduction(DateOnly date, decimal solarArraySize, decimal batterySizeNeeded, decimal panelAzimuth, decimal panelTilt)
    {
        calculatedSolarProduction = PvCalculatorService.CalculateKwDaily(date, solarArraySize, panelTilt, panelAzimuth);
        CalculatedBatteryNeeded = SolarProduction.Calculate(calculatedSolarProduction, batterySizeNeeded, MeterDataUsageInKw, MeterDataUsageInMoney);
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

        MeterDataUsageInKw.Reset();
        MeterDataUsageInMoney.Reset();
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
        
        MeterDataUsageInKw.Reset();
        MeterDataUsageInMoney.Reset();
        
        CalculateMeterDataUsageSummary();
        CalculateUsagePriceSummaries();

        SumKwUsedBetween08To17Monthly = DataBucket
            .Where(d => d.Key is >= 8 and <= 17 )
            .Sum(d => d.Value.MeterDataUsageInKw.PeekUsage);
        
        CalculatedBatteryNeeded = MeterDataUsageInKw.PeekUsage - SumKwUsedBetween08To17Monthly;
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