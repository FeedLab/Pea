namespace Pea.Infrastructure.Models.MeterData;

public class MeterDataManagerQuarter : MeterDataManagerBase<MeterDataManagerQuarter>
{
    public MeterDataManagerQuarter(decimal flatRatePrice, decimal peekPrice,
        decimal offPeekPrice)
        : base(flatRatePrice, peekPrice, offPeekPrice)
    {
        TimeResolution = FilterLevel.Quarter;
    }

    public List<MeterDataReading> GetReadings()
    {
        return MeterReadings;
    }

    public void AddRange(List<MeterDataReading> readings)
    {
        MeterReadings.AddRange(readings);

        MeterDataUsageInKw.Reset();
        MeterDataUsageInKw.Calculate(MeterReadings);
        
        MeterDataUsageInMoney.Reset();
        MeterDataUsageInMoney.Calculate(MeterReadings);
    }

    public void Clear()
    {
        MeterReadings.Clear();

        MeterDataUsageInKw.Reset();
        MeterDataUsageInMoney.Reset();
    }
}