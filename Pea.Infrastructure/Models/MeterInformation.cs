namespace Pea.Infrastructure.Models;

/// <summary>
/// Model for meter information
/// </summary>
public class MeterInformation
{
    public string? MeterNumber { get; set; }
    public string? MeterPointId { get; set; }
    public string? CtVtRatio { get; set; }
    public string? Kva { get; set; }
    public string? BillingCycle { get; set; }
}
