namespace Pea.Infrastructure.Models.MeterData;

public class MeterDataManagerYear : MeterDataManagerBase<MeterDataManagerMonth>
{
    private decimal calculatedSolarProduction;
    private decimal calculatedBatteryProduction;
    
    public decimal AverageKwUsedBetween08To17Monthly { get; private set; }
    public decimal SumKwUsedBetween08To17Monthly { get; private set; }
    public decimal CalculatedBatteryNeeded { get; private set; }
    
    internal Dictionary<int, MeterDataManagerMonth> MonthsData => DataBucket;

    public MeterDataManagerYear(DateOnly date, decimal flatRatePrice, decimal peekPrice, decimal offPeekPrice)
        : base(flatRatePrice, peekPrice, offPeekPrice)
    {
        TimeResolution = FilterLevel.Year;
        Date = date;
    }

    public decimal GetSolarProduction()
    {
        return calculatedSolarProduction;
    }
    
    public decimal GetBatteryProduction()
    {
        return calculatedBatteryProduction;
    }
    
    public void CalculateSolarProduction(int year, decimal solarArraySize, decimal batterySizeNeeded, decimal panelAzimuth, decimal panelTilt)
    {
        foreach (var meterData in DataBucket)
        {
            meterData.Value.CalculateSolarProduction(year, meterData.Key, solarArraySize, batterySizeNeeded, panelAzimuth, panelTilt);
        }
        
        calculatedSolarProduction = DataBucket.Sum(s => s.Value.GetSolarProduction());
        calculatedBatteryProduction = DataBucket.Sum(s => s.Value.SolarProduction.CalculatedBatteryNeeded);
        SolarProduction.Calculate(calculatedSolarProduction, calculatedBatteryProduction, MeterDataUsageInKw, MeterDataUsageInMoney);
    }
    
    public List<MeterDataReading> GetReadings(DateTime date, FilterLevel filterLevel)
    {
        if (filterLevel == TimeResolution)
            return MeterReadings;

        if (filterLevel == FilterLevel.Month)
        {
            var existsKey = DataBucket.ContainsKey(date.Month);

            if (existsKey)
            {
                return DataBucket[date.Month].GetReadings(date, filterLevel);
            }
        }
        else
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

        MeterDataUsageInKw.Reset();
        MeterDataUsageInMoney.Reset();
    }

    private void Add(List<MeterDataReading> readings)
    {
        var groups = readings.GroupBy(r => new { Year = r.PeriodStart.Year, Month = r.PeriodStart.Month });

        foreach (var group in groups)
        {
            if (!DataBucket.ContainsKey(group.Key.Month))
            {
                var date = new DateOnly(group.Key.Year, group.Key.Month, 1);
                DataBucket[group.Key.Month] = new MeterDataManagerMonth(date, FlatRatePrice, PeekPrice, OffPeekPrice);
            }

            DataBucket[group.Key.Month].AddRange(group.ToList(), Date);
        }
        
        AverageKwUsedBetween08To17Monthly = DataBucket.Average(d => d.Value.SumKwUsedBetween08To17Monthly);
        SumKwUsedBetween08To17Monthly = DataBucket.Average(d => d.Value.SumKwUsedBetween08To17Monthly);
        CalculatedBatteryNeeded = DataBucket.Sum(d => d.Value.CalculatedBatteryNeeded);
        
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
            DataBucket.Sum(s => s.Value.MeterDataUsageInMoney.FlatRateUsagePriceSummary);    }

    private void CalculateMeterDataUsageSummary()
    {
        MeterDataUsageInKw.PeekUsage = DataBucket.Sum(s => s.Value.MeterDataUsageInKw.PeekUsage);
        MeterDataUsageInKw.OffPeekUsage = DataBucket.Sum(s => s.Value.MeterDataUsageInKw.OffPeekUsage);
        MeterDataUsageInKw.Holiday = DataBucket.Sum(s => s.Value.MeterDataUsageInKw.Holiday);
    }
}