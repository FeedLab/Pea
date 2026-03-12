namespace Pea.Meter.Services;

public class SolarIrradianceService
{
    const double SolarConstant = 1361; // W/m2

    public static double CalculateIrradiance(
        DateTime time,
        double latitude,
        double longitude,
        double tilt,
        double panelAzimuth)
    {
        var dayOfYear = time.DayOfYear;

        var latRad = DegToRad(latitude);

        // Declination angle
        var decl = DegToRad(23.45 * Math.Sin(DegToRad(360.0 / 365.0 * (284 + dayOfYear))));

        // Solar hour angle
        var hour = time.Hour + time.Minute / 60.0;
        var hra = DegToRad(15 * (hour - 12));

        // Solar altitude
        var sinAlt =
            Math.Sin(latRad) * Math.Sin(decl) +
            Math.Cos(latRad) * Math.Cos(decl) * Math.Cos(hra);

        if (sinAlt <= 0)
            return 0;

        var altitude = Math.Asin(sinAlt);

        // Clear sky irradiance
        var ghi = SolarConstant * sinAlt * 0.75;

        // Panel tilt effect
        var tiltRad = DegToRad(tilt);

        var incidence =
            Math.Sin(altitude) * Math.Cos(tiltRad) +
            Math.Cos(altitude) * Math.Sin(tiltRad);

        if (incidence < 0)
            incidence = 0;

        return ghi * incidence;
    }

    static double DegToRad(double deg)
    {
        return deg * Math.PI / 180.0;
    }
}