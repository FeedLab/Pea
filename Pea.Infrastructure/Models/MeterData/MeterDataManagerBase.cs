namespace Pea.Infrastructure.Models.MeterData;

public class MeterDataManagerBase<T>
{
    protected readonly List<MeterDataReading> MeterReadings = [];
    protected readonly Dictionary<int, T> DataBucket = [];
    public DateOnly Date { get; set; }
    public DateTime DateAsDateTime => Date.ToDateTime(TimeOnly.MinValue);

    protected readonly decimal FlatRatePrice;
    protected readonly decimal PeekPrice;
    protected readonly decimal OffPeekPrice;

    public FilterLevel TimeResolution { get; set; } = FilterLevel.None;

    public SolarProduction SolarProduction { get; }
    public MeterDataUsageInKw MeterDataUsageInKw { get; }
    public MeterDataUsageInMoney MeterDataUsageInMoney { get; }

        
    protected MeterDataManagerBase(decimal flatRatePrice, decimal peekPrice, decimal offPeekPrice)
    {
        FlatRatePrice = flatRatePrice;
        PeekPrice = peekPrice;
        OffPeekPrice = offPeekPrice;
        
        MeterDataUsageInKw = new MeterDataUsageInKw();
        MeterDataUsageInMoney = new MeterDataUsageInMoney(FlatRatePrice, PeekPrice, OffPeekPrice);
        SolarProduction = new SolarProduction(FlatRatePrice, PeekPrice, OffPeekPrice);
    }
    
    public Dictionary<int, T> GetBuckets()
    {
        return DataBucket;
    }
}