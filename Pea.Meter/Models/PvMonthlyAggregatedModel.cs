using Pea.Infrastructure.Helpers;

namespace Pea.Meter.Models;

public class PvMonthlyAggregatedModel
{
    public DateTime PeriodStart { get; set; }

    public decimal BatteryKw { get; set; }
    public decimal PeakCalculatedKw { get; set; }
    public decimal OffPeakCalculatedKw { get; set; }
    public decimal DailyCalculatedKw { get; set; }
    public decimal DailyCalculateExcludedBatteryKw { get; set; }

    public decimal OffPeakUsedKw { get; set; }
    public decimal PeakUsedKw { get; set; }
    public decimal TotalUsedKw { get; set; }

    public string PeakUsedKwFormatted => WattFormatter.Format(PeakUsedKw);
    public string OffPeakUsedKwFormatted => WattFormatter.Format(OffPeakUsedKw);
    public string TotalUsedKwFormatted => WattFormatter.Format(TotalUsedKw);
    public string DailyCalculatedKwFormatted => WattFormatter.Format(DailyCalculatedKw);
    public string BatteryKwFormatted => WattFormatter.Format(BatteryKw);
    public string UnusedBatteryKwFormatted => WattFormatter.Format(UnusedBatteryKw);    
    public decimal UnusedBatteryKw { get; set; }
}
