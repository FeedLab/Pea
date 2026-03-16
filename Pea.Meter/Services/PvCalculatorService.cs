namespace Pea.Meter.Services;

public class PvCalculatorService
{
    private const double ThailandLatitude = 15.274053;
    private const double ThailandLongitude = 102.622572;
    
    const double SolarConstant = 1361;   // W/m2 outside atmosphere
    // System efficiency accounts for:
    // - Inverter losses (~4%)
    // - Wiring losses (~2%)
    // - Temperature derating in hot climate (~15%)
    // - Soiling/dust (~5%)
    // - Mismatch/shading (~3%)
    // Combined: 0.96 × 0.98 × 0.85 × 0.95 × 0.97 ≈ 0.58
    const double SystemEfficiency = 0.58;

    public static double CalculateKwDaily(
        DateTime time,
        double systemKWp,
        double tilt,
        double panelAzimuth,
        int timezone = 7)
    {
        return CalculateKwDaily(ThailandLatitude, ThailandLongitude, time, systemKWp, tilt, panelAzimuth, timezone);
    }

    public static double CalculateKwDaily(double latitude,
        double longitude,
        DateTime time,
        double systemKWp,
        double tilt,
        double panelAzimuth,
        int timezone = 7)
    {
        double totalKwh = 0;
        var dayStart = time.Date;

        for (var hour = 0; hour < 24; hour++)
        {
            var currentTime = dayStart.AddHours(hour);
            var powerKw = CalculateKw(latitude, longitude, currentTime, systemKWp, tilt, panelAzimuth, timezone);
            totalKwh += powerKw;
        }

        return totalKwh;
    }
    
    public static double CalculateKwMonthly(
        DateTime time,
        double systemKWp,
        double tilt,
        double panelAzimuth,
        int timezone = 7)
    {
        return CalculateKwMonthly(ThailandLatitude, ThailandLongitude, time, systemKWp, tilt, panelAzimuth, timezone);
    }
    
    public static double CalculateKwMonthly(double latitude,
        double longitude,
        DateTime time,
        double systemKWp,
        double tilt,
        double panelAzimuth,
        int timezone = 7)
    {
        double totalKwh = 0;
        var monthStart = new DateTime(time.Year, time.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(time.Year, time.Month);

        for (var day = 0; day < daysInMonth; day++)
        {
            var currentDay = monthStart.AddDays(day);
            var dailyKwh = CalculateKwDaily(latitude, longitude, currentDay, systemKWp, tilt, panelAzimuth, timezone);
            totalKwh += dailyKwh;
        }

        return totalKwh;
    }

    public static double CalculateKw(
        DateTime time,
        double systemKWp,
        double tilt,
        double panelAzimuth,
        int timezone = 7)
    {
        return CalculateKw(ThailandLatitude, ThailandLongitude, time, systemKWp, tilt, panelAzimuth, timezone);
    }

    public static int GetProductiveSolarHours(
        DateTime time,
        double systemKWp,
        double tilt,
        double panelAzimuth,
        int timezone = 7,
        double fraction = 0.03)
    {
        return GetProductiveSolarHours(ThailandLatitude, ThailandLongitude, time, systemKWp, tilt, panelAzimuth, timezone, fraction);
    }

    public static int GetProductiveSolarHours(
        double latitude,
        double longitude,
        DateTime time,
        double systemKWp,
        double tilt,
        double panelAzimuth,
        int timezone = 7,
        double fraction = 0.03) // 3 Percent
    {
        // Step 1: total daily energy
        var totalKwh = CalculateKwDaily(latitude, longitude, time, systemKWp, tilt, panelAzimuth, timezone);

        // Step 2: threshold
        var threshold = totalKwh * fraction;

        var productiveHours = 0;
        var dayStart = time.Date;

        // Step 3: loop through hours
        for (var hour = 0; hour < 24; hour++)
        {
            var currentTime = dayStart.AddHours(hour);
            var powerKw = CalculateKw(latitude, longitude, currentTime, systemKWp, tilt, panelAzimuth, timezone);

            if (powerKw > threshold)
                productiveHours++;
        }

        return productiveHours;
    }

    
    public static double CalculateKw(
        double latitude,
        double longitude,
        DateTime time,
        double systemKWp,
        double tilt,
        double panelAzimuth,
        int timezone = 7)
    {
        var day = time.DayOfYear;

        var lat = DegToRad(latitude);
        var tiltRad = DegToRad(tilt);
        var panelAzRad = DegToRad(panelAzimuth);

        // solar declination
        var decl = DegToRad(23.45 *
                            Math.Sin(DegToRad(360.0 * (284 + day) / 365)));

        // equation of time
        var b = DegToRad((360.0 / 365.0) * (day - 81));
        var eoT = 9.87 * Math.Sin(2 * b)
                  - 7.53 * Math.Cos(b)
                  - 1.5 * Math.Sin(b);

        // local standard time meridian
        double lstm = 15 * timezone;

        var tc = 4 * (longitude - lstm) + eoT;

        var localTime =
            time.Hour +
            time.Minute / 60.0 +
            time.Second / 3600.0;

        var solarTime = localTime + tc / 60.0;

        // hour angle
        var hra = DegToRad(15 * (solarTime - 12));

        // solar altitude
        var sinAlt =
            Math.Sin(lat) * Math.Sin(decl) +
            Math.Cos(lat) * Math.Cos(decl) * Math.Cos(hra);

        if (sinAlt <= 0)
            return 0;

        var altitude = Math.Asin(sinAlt);

        // solar azimuth
        var cosAz =
            (Math.Sin(decl) - Math.Sin(altitude) * Math.Sin(lat)) /
            (Math.Cos(altitude) * Math.Cos(lat));

        var solarAzimuth = Math.Acos(cosAz);

        if (hra > 0)
            solarAzimuth = 2 * Math.PI - solarAzimuth;

        // air mass
        var airMass = 1 / Math.Sin(altitude);

        // atmospheric attenuation
        var transmittance = Math.Pow(0.7, Math.Pow(airMass, 0.678));

        // direct normal irradiance
        var dni = SolarConstant * transmittance;

        // diffuse irradiance
        var dhi = 0.1 * dni;

        // angle of incidence
        var cosIncidence =
            Math.Sin(altitude) * Math.Cos(tiltRad) +
            Math.Cos(altitude) * Math.Sin(tiltRad) *
            Math.Cos(solarAzimuth - panelAzRad);

        cosIncidence = Math.Max(0, cosIncidence);

        // plane of array irradiance
        var poa =
            dni * cosIncidence +
            dhi * (1 + Math.Cos(tiltRad)) / 2;

        // convert to PV output
        var powerKw = systemKWp * (poa / 1000.0) * SystemEfficiency;

        return Math.Max(0, powerKw);
    }

    static double DegToRad(double deg)
    {
        return deg * Math.PI / 180.0;
    }
}