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
    public decimal AverageKwUsedBetween08To17Monthly { get; private set; }
    public decimal SumKwUsedBetween08To17Monthly { get; private set; }
    public decimal CalculatedBatteryNeeded { get; private set; }
    
    private decimal calculatedSolarProduction;
    private decimal calculatedBatteryProduction;

    public MeterDataManager(List<MeterDataReading> meterReadings, decimal flatRatePrice, decimal peekPrice, decimal offPeekPrice)
    : base(flatRatePrice, peekPrice, offPeekPrice)
    {
        TimeResolution = FilterLevel.None;
        AddRange(meterReadings);
    }

    public decimal GetSolarProduction()
    {
        return calculatedSolarProduction;
    }
    
    public decimal GetBatteryProduction()
    {
        return calculatedBatteryProduction;
    }
    
    public void CalculateSolarProduction(decimal solarArraySize, decimal batterySizeNeeded, decimal panelAzimuth,
        decimal panelTilt)
    {
        var years = DataBucket.Values.ToList();
        
        Parallel.ForEach(years, yearManager =>
        {
            yearManager.CalculateSolarProduction(yearManager.Date.Year, solarArraySize, batterySizeNeeded, panelAzimuth, panelTilt);
        });
        
        calculatedSolarProduction = DataBucket.Sum(s => s.Value.GetSolarProduction());
        calculatedBatteryProduction = DataBucket.Average(s => s.Value.GetBatteryProduction());
        SolarProduction.Calculate(calculatedSolarProduction, CalculatedBatteryNeeded, MeterDataUsageInKw, MeterDataUsageInMoney);
    }
    
    public List<MeterDataReading> GetReadings(DateTime date, FilterLevel filterLevel = FilterLevel.None)
    {
        if (filterLevel == TimeResolution)
        {
            return MeterReadings;
        }

        if (filterLevel == FilterLevel.Year)
        {
            var existsKey = DataBucket.ContainsKey(date.Year);

            if (existsKey)
            {
                return DataBucket[date.Year].GetReadings(date, filterLevel);
            }
        }
        else
        {
            var existsKey = DataBucket.ContainsKey(date.Year);

            if (existsKey)
            {
                return DataBucket[date.Year].GetReadings(date, filterLevel);
            }  
        }

        return MeterReadings;
    }

    public List<MeterDataManagerMonth> GetMonthsInRange(int startYear, int startMonth, int endYear, int endMonth)
    {
        var result = new List<MeterDataManagerMonth>();

        // Validate input
        if (startYear > endYear || (startYear == endYear && startMonth > endMonth))
        {
            return result; // Return empty list if range is invalid
        }

        // Iterate through all months in the range
        var currentDate = new DateTime(startYear, startMonth, 1);
        var endDate = new DateTime(endYear, endMonth, 1);

        while (currentDate <= endDate)  
        {
            // Check if year exists
            if (DataBucket.TryGetValue(currentDate.Year, out var yearManager))
            {
                // Check if a month exists in that year
                if (yearManager.MonthsData.TryGetValue(currentDate.Month, out var value))
                {
                    result.Add(value);
                }
            }

            // Move to next month
            currentDate = currentDate.AddMonths(1);
        }

        return result;
    }

    
    public void AddRange(List<MeterDataReading> readings)
    {
        try
        {
            MeterReadings.AddRange(readings);

            var groups = readings.GroupBy(r => r.PeriodStart.Year);

            foreach (var group in groups)
            {
                if (!DataBucket.ContainsKey(group.Key))
                {
                    var date = new DateOnly(group.Key, 1, 1);
                    DataBucket[group.Key] = new MeterDataManagerYear(date, FlatRatePrice, PeekPrice, OffPeekPrice);
                }

                DataBucket[group.Key].AddRange(group.ToList());
            }

            if (DataBucket.Count > 0)
            {
                AverageKwUsedBetween08To17Monthly = DataBucket.Average(d => d.Value.SumKwUsedBetween08To17Monthly);
                SumKwUsedBetween08To17Monthly = DataBucket.Sum(d => d.Value.SumKwUsedBetween08To17Monthly);
                CalculatedBatteryNeeded = DataBucket.Sum(d => d.Value.CalculatedBatteryNeeded);
            }


            MeterDataUsageInKw.Reset();
            MeterDataUsageInMoney.Reset();

            CalculateMeterDataUsageSummary();
            CalculateUsagePriceSummaries();
        }
        catch (Exception e)
        {
            throw new Exception("Error adding meter data readings", e);
        }
    }

    private void CalculateUsagePriceSummaries()
    {
        MeterDataUsageInMoney.PeekTouUsagePriceSummary = DataBucket.Sum(s => s.Value.MeterDataUsageInMoney.PeekTouUsagePriceSummary);
        MeterDataUsageInMoney.OffPeekTouUsagePriceSummary = DataBucket.Sum(s => s.Value.MeterDataUsageInMoney.OffPeekTouUsagePriceSummary);
        MeterDataUsageInMoney.FlatRateUsagePriceSummary =
            DataBucket.Sum(s => s.Value.MeterDataUsageInMoney.FlatRateUsagePriceSummary);    }

    private void CalculateMeterDataUsageSummary()
    {
        MeterDataUsageInKw.PeekUsage = DataBucket.Sum(s => s.Value.MeterDataUsageInKw.PeekUsage);
        MeterDataUsageInKw.OffPeekUsage = DataBucket.Sum(s => s.Value.MeterDataUsageInKw.OffPeekUsage);
        MeterDataUsageInKw.Holiday = DataBucket.Sum(s => s.Value.MeterDataUsageInKw.Holiday);
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
}