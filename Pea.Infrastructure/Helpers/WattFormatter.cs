namespace Pea.Infrastructure.Helpers;

public static class WattFormatter
{
    public static string Format(decimal watts)
    {
        return Math.Abs(watts) switch
        {
            >= 1_000_000m => $"{watts / 1_000_000m:G3} MW",
            >= 1_000m => $"{watts / 1_000m:G3} kW",
            _ => $"{watts:G3} W"
        };
    }
}
