namespace Pea.Infrastructure.Models.MeterData;

public class MeterDataUsageInKwSummary
{
    public decimal PeekUsage { get; set; }
    public decimal OffPeekUsage { get; set; }
    public decimal Holiday { get; set; }
    public decimal TotalUsage => PeekUsage + OffPeekUsage;
    
    public void Reset()
    {
        PeekUsage = 0;
        OffPeekUsage = 0;
    }
    
    public void Calculate(List<MeterDataReading> meterReadings)
    {
        PeekUsage += meterReadings.Sum(r => r.PeekUsage);
        OffPeekUsage += meterReadings.Sum(r => r.OffPeekUsage);
        Holiday += meterReadings.Sum(r => r.HolidayUsage);
    }
}