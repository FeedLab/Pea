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
    private decimal calculatedSolarProduction;

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
    
    public void CalculateSolarProduction(decimal solarArraySize, decimal panelAzimuth, decimal panelTilt)
    {
        foreach (var meterData in DataBucket)
        {
            meterData.Value.CalculateSolarProduction(meterData.Key, solarArraySize, panelAzimuth, panelTilt);
        }
        
        calculatedSolarProduction = DataBucket.Sum(s => s.Value.GetSolarProduction());
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
            if (DataBucket.ContainsKey(currentDate.Year))
            {
                var yearManager = DataBucket[currentDate.Year];

                // Check if month exists in that year
                if (yearManager.MonthsData.ContainsKey(currentDate.Month))
                {
                    result.Add(yearManager.MonthsData[currentDate.Month]);
                }
            }

            // Move to next month
            currentDate = currentDate.AddMonths(1);
        }

        return result;
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
        
        MeterDataUsageInKwSummary.Reset();
        MeterDataUsageInMoneySummary.Reset();
        
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

        MeterDataUsageInKwSummary.Reset();
        MeterDataUsageInMoneySummary.Reset();
    }
}