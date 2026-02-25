namespace Pea.Meter.Models;

/// <summary>
/// Model for session information
/// </summary>
public class SessionInformation
{
    public string? IpAddress { get; set; }
    public string? Username { get; set; }
    public DateTime? LastLoginTime { get; set; }
}
