namespace Pea.Infrastructure.Models;

/// <summary>
/// Aggregate model containing all customer profile information
/// </summary>
public class CustomerProfile
{
    public PersonalInformation Personal { get; set; } = new();
    public MeterInformation Meter { get; set; } = new();
    public BusinessInformation Business { get; set; } = new();
    public ContactInformation Contact { get; set; } = new();
    public SessionInformation Session { get; set; } = new();
}
