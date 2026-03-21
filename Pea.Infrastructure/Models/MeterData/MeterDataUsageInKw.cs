namespace Pea.Infrastructure.Models.MeterData;

public class MeterDataUsageInKw
{
    public decimal PeekUsage { get; set; }
    public decimal OffPeekUsage { get; set; }
    public decimal Holiday { get; set; }
    public decimal TotalUsage => PeekUsage + OffPeekUsage + Holiday;
    
    public void Reset()
    {
        PeekUsage = 0;
        OffPeekUsage = 0;
        Holiday = 0;
    }
    
    public void Calculate(List<MeterDataReading> meterReadings)
    {
        PeekUsage += meterReadings.Sum(r => r.PeekUsage);
        OffPeekUsage += meterReadings.Sum(r => r.OffPeekUsage);
        Holiday += meterReadings.Sum(r => r.HolidayUsage);
    }
}