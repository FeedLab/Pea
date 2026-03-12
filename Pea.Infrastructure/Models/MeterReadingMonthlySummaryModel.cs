namespace Pea.Infrastructure.Models;

/// <summary>
/// Model representing a monthly summary of meter readings aggregated by peak and off-peak periods
/// </summary>
public class MeterReadingMonthlySummary
{
    /// <summary>
    /// The first day of the month for this summary
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Total kilowatt-hours used during peak period
    /// </summary>
    public decimal KwUsedAtPeek { get; set; }

    /// <summary>
    /// Total kilowatt-hours used during off-peak period
    /// </summary>
    public decimal KwUsedAtOffPeek { get; set; }

    /// <summary>
    /// Total kilowatt-hours used (peak + off-peak)
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Average kilowatt-hours used at peak period per record
    /// </summary>
    public decimal AverageKwUsedAtPeekPerRecord { get; set; }

    /// <summary>
    /// Average kilowatt-hours used at off-peak period per record
    /// </summary>
    public decimal AverageKwUsedAtOffPeekPerRecord { get; set; }

    /// <summary>
    /// Average kilowatt-hours used at peak period per day
    /// </summary>
    public decimal AverageKwUsedAtPeekPerDay { get; set; }

    /// <summary>
    /// Average kilowatt-hours used at off-peak period per day
    /// </summary>
    public decimal AverageKwUsedAtOffPeekPerDay { get; set; }
}
