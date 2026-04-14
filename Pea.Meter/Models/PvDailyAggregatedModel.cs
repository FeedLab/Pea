namespace Pea.Meter.Models;

public class PvDailyAggregatedModel
{
    public DateOnly PeriodStart { get; set; }
    public Dictionary<DateTime, decimal> Readings { get; set; } = [];
    public decimal PeakTotal { get; set; }
    public decimal DayTotal { get; set; }
    public decimal OffPeakTotal { get; set; }
}
