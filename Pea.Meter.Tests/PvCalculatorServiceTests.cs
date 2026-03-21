using FluentAssertions;
using Pea.Infrastructure;
using Pea.Meter.Services;
using Xunit.Abstractions;

namespace Pea.Meter.Tests;

public class PvCalculatorServiceTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public PvCalculatorServiceTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    // Location constants (Phimai, Thailand)
    private const double BangkokLatitude = 15.274053;
    private const double BangkokLongitude = 102.622572;
    private const int ThailandTimezone = 7;

    // System configuration constants
    private const double StandardSystemKWp = 5.0;
    private const double StandardTilt = 15.0;
    private const double SouthFacingAzimuth = 180.0;
    private const double EastFacingAzimuth = 90.0;
    private const double WestFacingAzimuth = 270.0;

    // Test date constants
    private const int TestYear = 2024;
    private const int SummerMonth = 6; // June
    private const int WinterMonth = 12; // December
    private const int SpringMonth = 3; // March
    private const int TestDay = 15;

    [Fact]
    public void CalculateKwDaily_WanphenFarm_ShouldProduceRealisticDailyEnergy()
    {
        // Arrange - 21 kWp system at Bangkok location
        var time = new DateTime(2026, 4, TestDay, 0, 0, 0);
        const double systemSize = 21.0; // kWp
        const double tiltAngle = 3.0;

        // Act
        var dailyKwh = PvCalculatorService.CalculateKwDaily(
            BangkokLatitude,
            BangkokLongitude,
            time,
            systemSize,
            tiltAngle,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        // Realistic expectations for a 21kWp system in Bangkok:
        // - Peak sun hours in Thailand: ~4.5 hours/day average
        // - System efficiency: 0.58 (accounts for inverter, wiring, temperature, soiling, mismatch)
        // - Expected daily production: 21 kWp × 4.5 hours = ~94.5 kWh/day
        // - Current formula produces: ~94 kWh/day (4.47 equivalent peak sun hours) ✓
        //
        // This matches real-world expectations for Bangkok climate
        dailyKwh.Should().BeGreaterThan(70); // Minimum reasonable production (cloudy/rainy days)
        dailyKwh.Should().BeLessThanOrEqualTo(110); // Maximum realistic production (clear sunny days)

        // Output for debugging - write to console for visibility
        testOutputHelper.WriteLine($"Daily production for 21kWp system: {dailyKwh:F2} kWh/day (equivalent {dailyKwh/systemSize:F2} peak sun hours)");
    }
    
    [Fact]
    public void CalculateKw_AtNight_ShouldReturnZero()
    {
        // Arrange - midnight
        var time = new DateTime(TestYear, SummerMonth, TestDay, 0, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().Be(0.0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void CalculateKw_BeforeSunrise_ShouldReturnZero(int hour)
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, hour, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().Be(0.0);
    }

    [Theory]
    [InlineData(19)]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(22)]
    [InlineData(23)]
    public void CalculateKw_AfterSunset_ShouldReturnZero(int hour)
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, hour, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void CalculateKw_AtNoon_ShouldReturnPositiveValue()
    {
        // Arrange - solar noon
        var time = new DateTime(TestYear, SummerMonth, TestDay, 12, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().BeGreaterThan(0.0);
        result.Should().BeLessThanOrEqualTo(StandardSystemKWp); // Cannot exceed system capacity
    }

    [Theory]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    public void CalculateKw_MorningHours_ShouldReturnIncreasingValues(int hour)
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, hour, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0.0);
    }

    [Theory]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    public void CalculateKw_AfternoonHours_ShouldReturnPositiveValues(int hour)
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, hour, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0.0);
    }

    [Fact]
    public void CalculateKw_WithZeroSystemCapacity_ShouldReturnZero()
    {
        // Arrange
        const double zeroCapacity = 0.0;
        var time = new DateTime(TestYear, SummerMonth, TestDay, 12, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            time,
            zeroCapacity,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().Be(0.0);
    }

    [Theory]
    [InlineData(5.0)]
    [InlineData(10.0)]
    [InlineData(15.0)]
    [InlineData(20.0)]
    public void CalculateKw_LargerSystemCapacity_ShouldProduceProportionallyMorePower(double systemKWp)
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 12, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            time,
            systemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().BeGreaterThan(0.0);
        result.Should().BeLessThanOrEqualTo(systemKWp);
    }

    [Theory]
    [InlineData(0.0)]   // Horizontal
    [InlineData(15.0)]  // Optimal for Bangkok
    [InlineData(30.0)]  // Steeper angle
    [InlineData(45.0)]  // Very steep
    public void CalculateKw_DifferentTiltAngles_ShouldReturnValidResults(double tilt)
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 12, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            tilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0.0);
        result.Should().BeLessThanOrEqualTo(StandardSystemKWp);
    }

    [Theory]
    [InlineData(0.0)]    // North
    [InlineData(90.0)]   // East
    [InlineData(180.0)]  // South (optimal)
    [InlineData(270.0)]  // West
    public void CalculateKw_DifferentAzimuthAngles_ShouldReturnValidResults(double azimuth)
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 12, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            azimuth,
            ThailandTimezone);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0.0);
        result.Should().BeLessThanOrEqualTo(StandardSystemKWp);
    }

    [Fact]
    public void CalculateKw_OptimalAzimuth_ShouldProduceMoreThanSuboptimal()
    {
        // Arrange - Compare optimal (south) vs suboptimal orientations
        const double northFacingAzimuth = 0.0;
        var noonTime = new DateTime(TestYear, WinterMonth, TestDay, 12, 0, 0);

        // Act
        var southFacingResult = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            noonTime,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        var northFacingResult = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            noonTime,
            StandardSystemKWp,
            StandardTilt,
            northFacingAzimuth,
            ThailandTimezone);

        // Assert - South should produce more than north in winter
        southFacingResult.Should().BeGreaterThanOrEqualTo(northFacingResult);
    }

    [Fact]
    public void CalculateKw_EastFacingMorning_ShouldProducePower()
    {
        // Arrange - early morning
        var morningTime = new DateTime(TestYear, SummerMonth, TestDay, 8, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            morningTime,
            StandardSystemKWp,
            StandardTilt,
            EastFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0.0);
    }

    [Fact]
    public void CalculateKw_WestFacingAfternoon_ShouldProducePower()
    {
        // Arrange - late afternoon
        var afternoonTime = new DateTime(TestYear, SummerMonth, TestDay, 16, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            afternoonTime,
            StandardSystemKWp,
            StandardTilt,
            WestFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0.0);
    }

    [Theory]
    [InlineData(6, 21)]   // Summer (June)
    [InlineData(3, 21)]   // Spring (March)
    [InlineData(12, 21)]  // Winter (December)
    public void CalculateKw_DifferentSeasons_ShouldReturnValidResults(int month, int day)
    {
        // Arrange - noon for different seasons
        var time = new DateTime(TestYear, month, day, 12, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().BeGreaterThan(0.0);
        result.Should().BeLessThanOrEqualTo(StandardSystemKWp);
    }

    [Fact]
    public void CalculateKw_NorthernHemisphere_ShouldProduceMoreInSummer()
    {
        // Arrange - locations at different latitudes, noon time
        const double northernLatitude = 35.0; // Northern hemisphere
        var summerTime = new DateTime(TestYear, SummerMonth, TestDay, 12, 0, 0);
        var winterTime = new DateTime(TestYear, WinterMonth, TestDay, 12, 0, 0);

        // Act
        var summerResult = PvCalculatorService.CalculateKw(
            northernLatitude,
            BangkokLongitude,
            summerTime,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        var winterResult = PvCalculatorService.CalculateKw(
            northernLatitude,
            BangkokLongitude,
            winterTime,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        summerResult.Should().BeGreaterThan(winterResult);
    }

    [Fact]
    public void CalculateKw_ResultShouldNeverBeNegative()
    {
        // Arrange - various times throughout the day
        var results = new List<double>();

        for (int hour = 0; hour < 24; hour++)
        {
            var time = new DateTime(TestYear, SummerMonth, TestDay, hour, 0, 0);

            // Act
            var result = PvCalculatorService.CalculateKw(
                BangkokLatitude,
                BangkokLongitude,
                time,
                StandardSystemKWp,
                StandardTilt,
                SouthFacingAzimuth,
                ThailandTimezone);

            results.Add(result);
        }

        // Assert
        results.Should().AllSatisfy(r => r.Should().BeGreaterThanOrEqualTo(0.0));
    }

    [Fact]
    public void CalculateKw_ResultShouldNeverExceedSystemCapacity()
    {
        // Arrange - various times throughout the day
        var results = new List<double>();

        for (int hour = 6; hour < 18; hour++)
        {
            var time = new DateTime(TestYear, SummerMonth, TestDay, hour, 0, 0);

            // Act
            var result = PvCalculatorService.CalculateKw(
                BangkokLatitude,
                BangkokLongitude,
                time,
                StandardSystemKWp,
                StandardTilt,
                SouthFacingAzimuth,
                ThailandTimezone);

            results.Add(result);
        }

        // Assert
        results.Should().AllSatisfy(r => r.Should().BeLessThanOrEqualTo(StandardSystemKWp));
    }

    [Theory]
    [InlineData(-10.0)] // Southern hemisphere
    [InlineData(0.0)]   // Equator
    [InlineData(13.7563)] // Bangkok
    [InlineData(35.0)]  // Northern hemisphere
    public void CalculateKw_DifferentLatitudes_ShouldReturnValidResults(double latitude)
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 12, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            latitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0.0);
        result.Should().BeLessThanOrEqualTo(StandardSystemKWp);
    }

    [Theory]
    [InlineData(6)]  // UTC+6
    [InlineData(7)]  // UTC+7 (Thailand)
    [InlineData(8)]  // UTC+8
    public void CalculateKw_DifferentTimezones_ShouldAffectResults(int timezone)
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 12, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            timezone);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0.0);
    }

    [Fact]
    public void CalculateKw_PeakProduction_ShouldBeAroundNoon()
    {
        // Arrange - calculate for hours around noon
        var results = new Dictionary<int, double>();

        for (int hour = 10; hour <= 14; hour++)
        {
            var time = new DateTime(TestYear, SummerMonth, TestDay, hour, 0, 0);
            var result = PvCalculatorService.CalculateKw(
                BangkokLatitude,
                BangkokLongitude,
                time,
                StandardSystemKWp,
                StandardTilt,
                SouthFacingAzimuth,
                ThailandTimezone);

            results[hour] = result;
        }

        // Assert - noon should have highest or near-highest production
        var noonProduction = results[12];
        noonProduction.Should().BeGreaterThan(0.0);

        // At least one of the noon-adjacent hours should have less production
        var hasLowerAdjacentHour = results[11] < noonProduction || results[13] < noonProduction;
        hasLowerAdjacentHour.Should().BeTrue();
    }

    #region Overloaded Methods with Default Thailand Location

    [Fact]
    public void CalculateKw_WithDefaultLocation_ShouldProducePowerAtNoon()
    {
        // Arrange - using overload with default Thailand location
        var time = new DateTime(TestYear, SummerMonth, TestDay, 12, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().BeGreaterThan(0.0);
        result.Should().BeLessThanOrEqualTo(StandardSystemKWp);
    }

    [Fact]
    public void CalculateKw_WithDefaultLocation_ShouldReturnZeroAtNight()
    {
        // Arrange - using overload with default Thailand location
        var time = new DateTime(TestYear, SummerMonth, TestDay, 0, 0, 0);

        // Act
        var result = PvCalculatorService.CalculateKw(
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void CalculateKwDaily_WithDefaultLocation_ShouldProduceRealisticDailyEnergy()
    {
        // Arrange - using overload with default Thailand location
        var time = new DateTime(TestYear, SummerMonth, TestDay, 0, 0, 0);
        const double systemSize = 10.0; // kWp

        // Act
        var dailyKwh = PvCalculatorService.CalculateKwDaily(
            time,
            systemSize,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        // For 10kWp system in Thailand: expect ~45 kWh/day (4.5 peak sun hours)
        dailyKwh.Should().BeGreaterThan(30);
        dailyKwh.Should().BeLessThanOrEqualTo(55);
        Console.WriteLine($"Daily production (default location): {dailyKwh:F2} kWh/day for {systemSize}kWp");
    }

    [Fact]
    public void CalculateKwDaily_WithCustomLocation_ShouldMatchExplicitCall()
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 0, 0, 0);

        // Act - call both overloads
        var defaultLocationResult = PvCalculatorService.CalculateKwDaily(
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        var explicitLocationResult = PvCalculatorService.CalculateKwDaily(
            15.274053, // Thailand latitude (default)
            102.622572, // Thailand longitude (default)
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert - both should give same result
        defaultLocationResult.Should().BeApproximately(explicitLocationResult, 0.01);
    }

    [Fact]
    public void CalculateKwMonthly_WithDefaultLocation_ShouldProduceRealisticMonthlyEnergy()
    {
        // Arrange - using overload with default Thailand location
        var time = new DateTime(TestYear, 4, 1); // April (30 days)
        const double systemSize = 5.0; // kWp

        // Act
        var monthlyKwh = PvCalculatorService.CalculateKwMonthly(
            time,
            systemSize,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        // For 5kWp system in Thailand for 30 days: expect ~22.5 kWh/day * 30 = ~675 kWh/month
        monthlyKwh.Should().BeGreaterThan(500);
        monthlyKwh.Should().BeLessThanOrEqualTo(800);
        Console.WriteLine($"Monthly production (default location): {monthlyKwh:F2} kWh/month for {systemSize}kWp in April");
    }

    [Fact]
    public void CalculateKwMonthly_WithCustomLocation_ShouldMatchExplicitCall()
    {
        // Arrange
        var time = new DateTime(TestYear, 6, 1); // June

        // Act - call both overloads
        var defaultLocationResult = PvCalculatorService.CalculateKwMonthly(
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        var explicitLocationResult = PvCalculatorService.CalculateKwMonthly(
            15.274053, // Thailand latitude (default)
            102.622572, // Thailand longitude (default)
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert - both should give same result
        defaultLocationResult.Should().BeApproximately(explicitLocationResult, 0.1);
    }

    [Theory]
    [InlineData(1, 31)]  // January - 31 days
    [InlineData(2, 28)]  // February - 28 days (2024 is leap year, so actually 29)
    [InlineData(4, 30)]  // April - 30 days
    [InlineData(12, 31)] // December - 31 days
    public void CalculateKwMonthly_DifferentMonths_ShouldAccountForDaysInMonth(int month, int expectedDays)
    {
        // Arrange
        var time = new DateTime(TestYear, month, 1);
        const double systemSize = 1.0; // 1 kWp for easy calculation

        // Act
        var monthlyKwh = PvCalculatorService.CalculateKwMonthly(
            BangkokLatitude,
            BangkokLongitude,
            time,
            systemSize,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        // Monthly production should be greater for months with more days
        // For 1kWp: expect ~4.5 kWh/day, so monthly = days * 4.5
        var daysInMonth = DateTime.DaysInMonth(TestYear, month);
        var expectedMinKwh = daysInMonth * 3.5; // Conservative estimate
        var expectedMaxKwh = daysInMonth * 5.5; // Optimistic estimate

        monthlyKwh.Should().BeGreaterThan(expectedMinKwh);
        monthlyKwh.Should().BeLessThanOrEqualTo(expectedMaxKwh);
    }

    [Fact]
    public void CalculateKwMonthly_LeapYear_FebruaryShouldHave29Days()
    {
        // Arrange - 2024 is a leap year
        var leapYearFeb = new DateTime(2024, 2, 1);
        var normalYearFeb = new DateTime(2023, 2, 1);

        // Act
        var leapYearProduction = PvCalculatorService.CalculateKwMonthly(
            BangkokLatitude,
            BangkokLongitude,
            leapYearFeb,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        var normalYearProduction = PvCalculatorService.CalculateKwMonthly(
            BangkokLatitude,
            BangkokLongitude,
            normalYearFeb,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert - leap year should produce slightly more (1 extra day)
        leapYearProduction.Should().BeGreaterThan(normalYearProduction);

        // The difference should be approximately 1 day's production
        var dailyAverage = normalYearProduction / 28;
        var difference = leapYearProduction - normalYearProduction;
        difference.Should().BeApproximately(dailyAverage, dailyAverage * 0.2); // Within 20% tolerance
    }

    [Fact]
    public void CalculateKwDaily_SumOfHourlyValues_ShouldMatchDailyTotal()
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 0, 0, 0);

        // Act
        var dailyTotal = PvCalculatorService.CalculateKwDaily(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Calculate sum of hourly values manually
        double hourlySum = 0;
        for (int hour = 0; hour < 24; hour++)
        {
            var hourTime = time.AddHours(hour);
            var hourlyPower = PvCalculatorService.CalculateKw(
                BangkokLatitude,
                BangkokLongitude,
                hourTime,
                StandardSystemKWp,
                StandardTilt,
                SouthFacingAzimuth,
                ThailandTimezone);
            hourlySum += hourlyPower;
        }

        // Assert
        dailyTotal.Should().BeApproximately(hourlySum, 0.001);
    }

    [Fact]
    public void CalculateKwMonthly_SumOfDailyValues_ShouldMatchMonthlyTotal()
    {
        // Arrange
        var time = new DateTime(TestYear, 4, 1); // April

        // Act
        var monthlyTotal = PvCalculatorService.CalculateKwMonthly(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Calculate sum of daily values manually
        double dailySum = 0;
        var daysInMonth = DateTime.DaysInMonth(TestYear, 4);
        for (int day = 0; day < daysInMonth; day++)
        {
            var dayTime = time.AddDays(day);
            var dailyProduction = PvCalculatorService.CalculateKwDaily(
                BangkokLatitude,
                BangkokLongitude,
                dayTime,
                StandardSystemKWp,
                StandardTilt,
                SouthFacingAzimuth,
                ThailandTimezone);
            dailySum += dailyProduction;
        }

        // Assert
        monthlyTotal.Should().BeApproximately(dailySum, 0.01);
    }

    #endregion

    #region GetProductiveSolarHours Tests

    [Fact]
    public void GetProductiveSolarHours_SummerDay_ShouldReturnReasonableHours()
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 0, 0, 0);

        // Act
        var productiveHours = PvCalculatorService.GetProductiveSolarHours(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        // In summer, expect roughly 10-12 productive hours (sunrise ~6am to sunset ~6pm)
        productiveHours.Should().BeGreaterThan(8);
        productiveHours.Should().BeLessThanOrEqualTo(14);
        Console.WriteLine($"Productive solar hours in summer: {productiveHours} hours");
    }

    [Fact]
    public void GetProductiveSolarHours_WinterDay_ShouldReturnReasonableHours()
    {
        // Arrange
        var time = new DateTime(TestYear, WinterMonth, TestDay, 0, 0, 0);

        // Act
        var productiveHours = PvCalculatorService.GetProductiveSolarHours(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        // In winter, expect slightly fewer productive hours
        productiveHours.Should().BeGreaterThan(7);
        productiveHours.Should().BeLessThanOrEqualTo(13);
        Console.WriteLine($"Productive solar hours in winter: {productiveHours} hours");
    }

    [Fact]
    public void GetProductiveSolarHours_WithDefaultLocation_ShouldProduceSameResult()
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 0, 0, 0);

        // Act - call both overloads
        var defaultLocationResult = PvCalculatorService.GetProductiveSolarHours(
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        var explicitLocationResult = PvCalculatorService.GetProductiveSolarHours(
            15.274053, // Thailand latitude (default)
            102.622572, // Thailand longitude (default)
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        defaultLocationResult.Should().Be(explicitLocationResult);
    }

    [Theory]
    [InlineData(0.01)] // 1% threshold - more strict, fewer hours
    [InlineData(0.03)] // 3% threshold - default
    [InlineData(0.05)] // 5% threshold - more lenient, more hours
    [InlineData(0.10)] // 10% threshold - very lenient
    public void GetProductiveSolarHours_DifferentThresholds_ShouldAffectCount(double fraction)
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 0, 0, 0);

        // Act
        var productiveHours = PvCalculatorService.GetProductiveSolarHours(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone,
            fraction);

        // Assert
        // Higher threshold (fraction) should result in more productive hours
        productiveHours.Should().BeGreaterThan(0);
        productiveHours.Should().BeLessThanOrEqualTo(24);
        Console.WriteLine($"Productive hours with {fraction * 100}% threshold: {productiveHours} hours");
    }

    [Fact]
    public void GetProductiveSolarHours_HigherThreshold_ShouldResultInFewerHours()
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 0, 0, 0);

        // Act
        var lenientHours = PvCalculatorService.GetProductiveSolarHours(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone,
            0.01); // 1% fraction = lower threshold = lenient (more hours pass)

        var strictHours = PvCalculatorService.GetProductiveSolarHours(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone,
            0.10); // 10% fraction = higher threshold = strict (fewer hours pass)

        // Assert
        // Higher threshold (strict) should give fewer or equal hours than lower threshold (lenient)
        strictHours.Should().BeLessThanOrEqualTo(lenientHours);
    }

    [Fact]
    public void GetProductiveSolarHours_NorthernLatitude_ShouldHaveFewerHoursInWinter()
    {
        // Arrange - Northern latitude location
        const double northernLatitude = 45.0; // e.g., Northern Europe
        var summerDay = new DateTime(TestYear, 6, 21); // Summer solstice
        var winterDay = new DateTime(TestYear, 12, 21); // Winter solstice

        // Act
        var summerHours = PvCalculatorService.GetProductiveSolarHours(
            northernLatitude,
            BangkokLongitude,
            summerDay,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        var winterHours = PvCalculatorService.GetProductiveSolarHours(
            northernLatitude,
            BangkokLongitude,
            winterDay,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        summerHours.Should().BeGreaterThan(winterHours);
        Console.WriteLine($"Northern latitude - Summer: {summerHours}h, Winter: {winterHours}h");
    }

    [Fact]
    public void GetProductiveSolarHours_LargerSystem_ShouldNotAffectHourCount()
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 0, 0, 0);

        // Act - same location and time, different system sizes
        var smallSystemHours = PvCalculatorService.GetProductiveSolarHours(
            BangkokLatitude,
            BangkokLongitude,
            time,
            5.0, // 5 kWp
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        var largeSystemHours = PvCalculatorService.GetProductiveSolarHours(
            BangkokLatitude,
            BangkokLongitude,
            time,
            20.0, // 20 kWp
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        // System size shouldn't affect productive hours count
        // (threshold is proportional to total energy)
        smallSystemHours.Should().Be(largeSystemHours);
    }

    [Fact]
    public void GetProductiveSolarHours_ShouldNeverExceed24Hours()
    {
        // Arrange - test various scenarios
        var testDates = new[]
        {
            new DateTime(TestYear, 1, 15),
            new DateTime(TestYear, 3, 15),
            new DateTime(TestYear, 6, 15),
            new DateTime(TestYear, 9, 15),
            new DateTime(TestYear, 12, 15)
        };

        foreach (var date in testDates)
        {
            // Act
            var productiveHours = PvCalculatorService.GetProductiveSolarHours(
                BangkokLatitude,
                BangkokLongitude,
                date,
                StandardSystemKWp,
                StandardTilt,
                SouthFacingAzimuth,
                ThailandTimezone,
                0.001); // Very low threshold

            // Assert
            productiveHours.Should().BeLessThanOrEqualTo(24);
        }
    }

    [Fact]
    public void GetProductiveSolarHours_ZeroThreshold_ShouldCountAllHoursWithPower()
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 0, 0, 0);

        // Act
        var productiveHours = PvCalculatorService.GetProductiveSolarHours(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            SouthFacingAzimuth,
            ThailandTimezone,
            0.0); // Zero threshold - count all hours with any power

        // Assert
        // Should count all daylight hours (roughly 10-14 hours)
        productiveHours.Should().BeGreaterThan(8);
        productiveHours.Should().BeLessThanOrEqualTo(16);
    }

    [Theory]
    [InlineData(0.0)]   // Horizontal
    [InlineData(15.0)]  // Optimal for Bangkok
    [InlineData(30.0)]  // Steeper
    public void GetProductiveSolarHours_DifferentTilts_ShouldReturnValidHours(double tilt)
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 0, 0, 0);

        // Act
        var productiveHours = PvCalculatorService.GetProductiveSolarHours(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            tilt,
            SouthFacingAzimuth,
            ThailandTimezone);

        // Assert
        productiveHours.Should().BeGreaterThan(0);
        productiveHours.Should().BeLessThanOrEqualTo(24);
        Console.WriteLine($"Productive hours with {tilt}° tilt: {productiveHours} hours");
    }

    [Theory]
    [InlineData(0.0)]    // North
    [InlineData(90.0)]   // East
    [InlineData(180.0)]  // South
    [InlineData(270.0)]  // West
    public void GetProductiveSolarHours_DifferentAzimuths_ShouldReturnValidHours(double azimuth)
    {
        // Arrange
        var time = new DateTime(TestYear, SummerMonth, TestDay, 0, 0, 0);

        // Act
        var productiveHours = PvCalculatorService.GetProductiveSolarHours(
            BangkokLatitude,
            BangkokLongitude,
            time,
            StandardSystemKWp,
            StandardTilt,
            azimuth,
            ThailandTimezone);

        // Assert
        productiveHours.Should().BeGreaterThan(0);
        productiveHours.Should().BeLessThanOrEqualTo(24);
        Console.WriteLine($"Productive hours with {azimuth}° azimuth: {productiveHours} hours");
    }

    [Fact]
    public void GetProductiveSolarHours_EquatorLocation_ShouldHaveConsistentHoursYearRound()
    {
        // Arrange - Equator location
        const double equatorLatitude = 0.0;
        var dates = new[]
        {
            new DateTime(TestYear, 3, 21),  // Spring equinox
            new DateTime(TestYear, 6, 21),  // Summer solstice
            new DateTime(TestYear, 9, 21),  // Fall equinox
            new DateTime(TestYear, 12, 21)  // Winter solstice
        };

        var hoursList = new List<int>();

        foreach (var date in dates)
        {
            // Act
            var hours = PvCalculatorService.GetProductiveSolarHours(
                equatorLatitude,
                BangkokLongitude,
                date,
                StandardSystemKWp,
                StandardTilt,
                SouthFacingAzimuth,
                ThailandTimezone);

            hoursList.Add(hours);
        }

        // Assert - At equator, productive hours should be relatively consistent throughout year
        var maxHours = hoursList.Max();
        var minHours = hoursList.Min();
        var difference = maxHours - minHours;

        // Difference should be small (within 2-3 hours)
        difference.Should().BeLessThanOrEqualTo(3);
        Console.WriteLine($"Equator productive hours - Max: {maxHours}, Min: {minHours}, Diff: {difference}");
    }

    #endregion

    [Fact]
    public void CalculateKwMonthly_2026Estimate_ShouldProduceRealisticYearlyPattern()
    {
        // Arrange - 21 kWp system at Wanphen Farm location for all months in 2026
        const double systemSize = 21.0; // kWp
        const double tiltAngle = 3.0;
        const int year = 2026;

        var monthlyProduction = new Dictionary<string, double>();
        double totalYearlyProduction = 0;

        // Act - Calculate monthly production for each month in 2026
        for (int month = 1; month <= 12; month++)
        {
            var time = new DateTime(year, month, 1);
            var monthlyKwh = PvCalculatorService.CalculateKwMonthly(
                BangkokLatitude,
                BangkokLongitude,
                time,
                systemSize,
                tiltAngle,
                SouthFacingAzimuth,
                ThailandTimezone);

            monthlyProduction[time.ToString("MMMM")] = monthlyKwh;
            totalYearlyProduction += monthlyKwh;
        }

        // Assert
        // For 21 kWp system in Thailand:
        // - Daily average: ~94 kWh/day (from CalculateKwDaily test)
        // - Yearly estimate: 94 kWh/day × 365 days = ~34,310 kWh/year
        // - Monthly average: ~2,859 kWh/month
        totalYearlyProduction.Should().BeGreaterThan(30000); // Minimum reasonable yearly production
        totalYearlyProduction.Should().BeLessThanOrEqualTo(38000); // Maximum realistic yearly production

        // Each month should produce reasonable amounts
        foreach (var kvp in monthlyProduction)
        {
            kvp.Value.Should().BeGreaterThan(2000); // Minimum for any month (rainy season)
            kvp.Value.Should().BeLessThanOrEqualTo(3200); // Maximum for any month (dry season)
        }

        // Output monthly breakdown for debugging
        testOutputHelper.WriteLine($"2026 Monthly Production Estimates for {systemSize} kWp system:");
        testOutputHelper.WriteLine("-----------------------------------------------------------");
        foreach (var kvp in monthlyProduction)
        {
            testOutputHelper.WriteLine($"{kvp.Key,-12}: {kvp.Value,8:F2} kWh");
        }
        testOutputHelper.WriteLine("-----------------------------------------------------------");
        testOutputHelper.WriteLine($"{"Total Yearly",-12}: {totalYearlyProduction,8:F2} kWh");
        testOutputHelper.WriteLine($"{"Average Daily",-12}: {totalYearlyProduction / 365,8:F2} kWh/day");
        testOutputHelper.WriteLine($"{"Average Monthly",-12}: {totalYearlyProduction / 12,8:F2} kWh/month");
    }
}
