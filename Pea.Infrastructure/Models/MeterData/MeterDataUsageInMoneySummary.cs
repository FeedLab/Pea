namespace Pea.Infrastructure.Models.MeterData;

public class MeterDataUsageInMoneySummary
{
    public decimal PeekUsage { get; set; }
    public decimal OffPeekUsage { get; set; }

    public decimal TotalUsage => PeekUsage + OffPeekUsage;

    public decimal PeekTouUsagePriceSummary { get; set; }
    public decimal OffPeekTouUsagePriceSummary { get; set; }
    public decimal FlatRateUsagePriceSummary { get; set; }

    public decimal TotalTouUsagePriceSummary => PeekTouUsagePriceSummary + OffPeekTouUsagePriceSummary;
    
    public void Reset()
    {
        PeekUsage = 0;
        OffPeekUsage = 0;
        PeekTouUsagePriceSummary = 0;
        OffPeekTouUsagePriceSummary = 0;
        FlatRateUsagePriceSummary = 0;
    }
    
    public void Calculate(List<MeterDataReading> meterReadings, decimal flatRatePrice, decimal peekPrice, decimal offPeekPrice)
    {
        PeekTouUsagePriceSummary += meterReadings.Sum(r => r.PeekUsage) * peekPrice;
        OffPeekTouUsagePriceSummary += meterReadings.Sum(r => r.OffPeekUsage) * offPeekPrice;
        FlatRateUsagePriceSummary += meterReadings.Sum(r => r.HolidayUsage) * flatRatePrice;
    }
}