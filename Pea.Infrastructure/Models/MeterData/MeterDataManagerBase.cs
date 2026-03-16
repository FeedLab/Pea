namespace Pea.Infrastructure.Models.MeterData;

public class MeterDataManagerBase<T>
{
    protected readonly List<MeterDataReading> MeterReadings = [];
    protected readonly Dictionary<int, T> DataBucket = [];
    
    protected readonly decimal FlatRatePrice;
    protected readonly decimal PeekPrice;
    protected readonly decimal OffPeekPrice;

    public readonly MeterDataUsageInKwSummary MeterDataUsageInKwSummary;
    public readonly MeterDataUsageInMoneySummary MeterDataUsageInMoneySummary;

    
    protected MeterDataManagerBase(decimal flatRatePrice, decimal peekPrice, decimal offPeekPrice)
    {
        FlatRatePrice = flatRatePrice;
        PeekPrice = peekPrice;
        OffPeekPrice = offPeekPrice;
        
        MeterDataUsageInKwSummary = new MeterDataUsageInKwSummary();
        MeterDataUsageInMoneySummary = new MeterDataUsageInMoneySummary();
    }
}