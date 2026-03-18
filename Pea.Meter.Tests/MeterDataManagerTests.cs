using FluentAssertions;
using Pea.Infrastructure.Models;
using Pea.Infrastructure.Models.MeterData;

namespace Pea.Meter.Tests;

/// <summary>
/// Unit tests for MeterDataManager class
/// </summary>
public class MeterDataManagerTests
{
    // Test data constants
    private const decimal DefaultPeekUsage = 10.5m;
    private const decimal DefaultOffPeekUsage = 20.3m;
    private const decimal DefaultHolidayUsage = 15.7m;
    private const decimal DefaultFlatRatePrice = 1.5m;
    private const decimal DefaultPeekPrice = 2.0m;
    private const decimal DefaultOffPeekPrice = 1.0m;

    // Time-related constants
    private const int MinutesPerQuarter = 15;
    private const int QuartersPerHour = 4;
    private const int HoursPerDay = 24;
    private const int MonthsInYear = 12;
    private const int DaysInJanuary = 31;
    private const int DaysInLeapYear = 366;
    private const int ReadingsPerDay = 96; // 24 hours * 4 quarters

    // Test scenario constants
    private const int MinuteForQuarterOne = 0;
    private const int MinuteForQuarterTwo = 15;
    private const int MinuteForQuarterThree = 30;
    private const int MinuteForQuarterFour = 45;
    private const int MidnightHour = 0;
    private const int MorningHour = 6;
    private const int NoonHour = 12;
    private const int EveningHour = 18;
    private const int LateNightHour = 23;
    private const int LateNightMinute = 45;

    // Test value constants
    private const decimal ZeroValue = 0m;
    private const decimal SmallTestValue = 0.01m;
    private const decimal MediumTestValue1 = 0.02m;
    private const decimal MediumTestValue2 = 0.03m;
    private const decimal LargeTestValue1 = 100m;
    private const decimal LargeTestValue2 = 200m;
    private const decimal LargeTestValue3 = 300m;

    // Solar test constants
    private const decimal SolarProduction15Kw = 15m;
    private const decimal SolarProduction10Kw = 10m;
    private const decimal SolarProduction20Kw = 20m;
    private const decimal SolarProduction30Kw = 30m;
    private const decimal SolarProduction25Kw = 25m;
    private const decimal SolarProduction8Kw = 8m;
    private const decimal SolarProduction12Kw = 12m;
    private const decimal MeterUsage10Kw = 10m;
    private const decimal MeterUsage20Kw = 20m;
    private const decimal MeterUsage5Kw = 5m;
    private const decimal MeterUsage15Kw = 15m;
    private const decimal MeterUsage6Kw = 6m;
    private const decimal MeterUsage2Kw = 2m;
    private const decimal MeterUsage3Kw = 3m;
    private const decimal SolarLost5Kw = 5m;
    private const decimal SolarLost3Kw = 3m;

    // Year constants
    private const int Year2023 = 2023;
    private const int Year2024 = 2024;
    private const int Year2025 = 2025;

    // Month constants
    private const int January = 1;
    private const int February = 2;
    private const int March = 3;
    private const int April = 4;
    private const int May = 5;
    private const int June = 6;
    private const int October = 10;
    private const int November = 11;
    private const int December = 12;

    // Day constants
    private const int FirstDayOfMonth = 1;
    private const int MidMonth = 15;
    private const int Day16 = 16;
    private const int Day17 = 17;
    private const int LastDayOfJanuary = 31;
    private const int LeapYearDay = 29;

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeWithEmptyReadings()
    {
        // Arrange
        var emptyReadings = new List<MeterDataReading>();

        // Act
        var manager = new MeterDataManager(emptyReadings, DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);

        // Assert
        manager.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithReadings()
    {
        // Arrange
        var readings = new List<MeterDataReading> { CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne) };

        // Act
        var manager = new MeterDataManager(readings, DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);

        // Assert
        manager.Should().NotBeNull();
        var result = manager.GetReadings(new DateTime(Year2024, January, FirstDayOfMonth), FilterLevel.None);
        result.Should().ContainSingle();
    }

    #endregion

    #region AddRange Tests - Single Reading

    [Fact]
    public void AddRange_WithSingleReading_ShouldAddSuccessfully()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var reading = CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne);
        var readings = new List<MeterDataReading> { reading };

        // Act
        manager.AddRange(readings);

        // Assert - No exception thrown means success
        manager.Should().NotBeNull();
    }

    [Fact]
    public void AddRange_WithEmptyList_ShouldNotThrowException()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>();

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region AddRange Tests - Multiple Readings

    [Fact]
    public void AddRange_WithMultipleReadingsSameDay_ShouldAddSuccessfully()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterTwo),
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterThree),
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterFour)
        };

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddRange_WithReadingsAcrossMultipleDays_ShouldAddSuccessfully()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, January, Day16, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, January, Day17, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddRange_WithReadingsAcrossMultipleMonths_ShouldAddSuccessfully()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, February, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, March, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddRange_WithReadingsAcrossMultipleYears_ShouldAddSuccessfully()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2023, December, LastDayOfJanuary, LateNightHour, LateNightMinute),
            CreateReading(Year2024, January, FirstDayOfMonth, MidnightHour, MinuteForQuarterOne),
            CreateReading(Year2025, January, FirstDayOfMonth, MidnightHour, MinuteForQuarterOne)
        };

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region AddRange Tests - Multiple Calls

    [Fact]
    public void AddRange_CalledMultipleTimes_ShouldAccumulateReadings()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings1 = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };
        var readings2 = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterTwo)
        };

        // Act
        manager.AddRange(readings1);
        var action = () => manager.AddRange(readings2);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddRange_WithSameYearMultipleTimes_ShouldMergeIntoExistingYear()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings1 = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };
        var readings2 = new List<MeterDataReading>
        {
            CreateReading(Year2024, February, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };

        // Act
        manager.AddRange(readings1);
        var action = () => manager.AddRange(readings2);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region AddRange Tests - Hour Boundaries

    [Fact]
    public void AddRange_WithReadingsAtDifferentHours_ShouldAddSuccessfully()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MidnightHour, MinuteForQuarterOne),   // Midnight
            CreateReading(Year2024, January, MidMonth, MorningHour, MinuteForQuarterOne),   // Morning
            CreateReading(Year2024, January, MidMonth, NoonHour, MinuteForQuarterOne),  // Noon
            CreateReading(Year2024, January, MidMonth, EveningHour, MinuteForQuarterOne),  // Evening
            CreateReading(Year2024, January, MidMonth, LateNightHour, LateNightMinute)  // Late night
        };

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddRange_WithAllQuartersInHour_ShouldAddSuccessfully()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterTwo),
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterThree),
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterFour)
        };

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_WithNoData_ShouldNotThrowException()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);

        // Act
        var action = () => manager.Clear();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Clear_WithData_ShouldRemoveAllReadings()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, February, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };
        manager.AddRange(readings);

        // Act
        manager.Clear();

        // Assert
        // After clear, should be able to add same readings again without issue
        var action = () => manager.AddRange(readings);
        action.Should().NotThrow();
    }

    [Fact]
    public void Clear_AfterMultipleAdds_ShouldClearAll()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        manager.AddRange(new List<MeterDataReading> { CreateReading(Year2023, January, FirstDayOfMonth, MidnightHour, MinuteForQuarterOne) });
        manager.AddRange(new List<MeterDataReading> { CreateReading(Year2024, January, FirstDayOfMonth, MidnightHour, MinuteForQuarterOne) });
        manager.AddRange(new List<MeterDataReading> { CreateReading(Year2025, January, FirstDayOfMonth, MidnightHour, MinuteForQuarterOne) });

        // Act
        manager.Clear();

        // Assert
        var action = () => manager.AddRange(new List<MeterDataReading> { CreateReading(Year2024, June, MidMonth, NoonHour, MinuteForQuarterOne) });
        action.Should().NotThrow();
    }

    [Fact]
    public void Clear_ShouldResetMeterDataUsageInKwSummary()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour + 1, MinuteForQuarterOne)
        };
        manager.AddRange(readings);

        // Verify summaries have data before clear
        var hasDataBefore = manager.MeterDataUsageInKwSummary.PeekUsage > ZeroValue ||
                           manager.MeterDataUsageInKwSummary.OffPeekUsage > ZeroValue ||
                           manager.MeterDataUsageInKwSummary.Holiday > ZeroValue;

        // Act
        manager.Clear();

        // Assert
        hasDataBefore.Should().BeTrue("summaries should have data before clear");
        manager.MeterDataUsageInKwSummary.PeekUsage.Should().Be(ZeroValue);
        manager.MeterDataUsageInKwSummary.OffPeekUsage.Should().Be(ZeroValue);
        manager.MeterDataUsageInKwSummary.Holiday.Should().Be(ZeroValue);
    }

    [Fact]
    public void Clear_ShouldResetMeterDataUsageInMoneySummary()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour + 1, MinuteForQuarterOne)
        };
        manager.AddRange(readings);

        // Verify summaries have data before clear
        var hasDataBefore = manager.MeterDataUsageInMoneySummary.PeekUsage > ZeroValue ||
                           manager.MeterDataUsageInMoneySummary.OffPeekUsage > ZeroValue ||
                           manager.MeterDataUsageInMoneySummary.PeekTouUsagePriceSummary > ZeroValue ||
                           manager.MeterDataUsageInMoneySummary.OffPeekTouUsagePriceSummary > ZeroValue ||
                           manager.MeterDataUsageInMoneySummary.FlatRateUsagePriceSummary > ZeroValue;

        // Act
        manager.Clear();

        // Assert
        hasDataBefore.Should().BeTrue("summaries should have data before clear");
        manager.MeterDataUsageInMoneySummary.PeekUsage.Should().Be(ZeroValue);
        manager.MeterDataUsageInMoneySummary.OffPeekUsage.Should().Be(ZeroValue);
        manager.MeterDataUsageInMoneySummary.PeekTouUsagePriceSummary.Should().Be(ZeroValue);
        manager.MeterDataUsageInMoneySummary.OffPeekTouUsagePriceSummary.Should().Be(ZeroValue);
        manager.MeterDataUsageInMoneySummary.FlatRateUsagePriceSummary.Should().Be(ZeroValue);
    }

    [Fact]
    public void Clear_ShouldAllowReAddingDataWithCorrectSummaries()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };
        manager.AddRange(readings);

        // Act
        manager.Clear();
        manager.AddRange(readings);

        // Assert - After re-adding, summaries should have values again
        var hasData = manager.MeterDataUsageInKwSummary.PeekUsage > ZeroValue ||
                     manager.MeterDataUsageInKwSummary.OffPeekUsage > ZeroValue ||
                     manager.MeterDataUsageInKwSummary.Holiday > ZeroValue;
        hasData.Should().BeTrue("summaries should be recalculated after re-adding data");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void AddRange_WithLeapYearDate_ShouldAddSuccessfully()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, February, LeapYearDay, NoonHour, MinuteForQuarterOne) // Leap year
        };

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddRange_WithYearEndAndYearStart_ShouldAddSuccessfully()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2023, December, LastDayOfJanuary, LateNightHour, LateNightMinute),
            CreateReading(Year2024, January, FirstDayOfMonth, MidnightHour, MinuteForQuarterOne)
        };

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddRange_WithDifferentRateValues_ShouldAddSuccessfully()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            new MeterDataReading(new DateTime(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne, 0), 0, 0, 0),
            new MeterDataReading(new DateTime(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterTwo, 0), LargeTestValue1, LargeTestValue2, LargeTestValue3),
            new MeterDataReading(new DateTime(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterThree, 0), SmallTestValue, MediumTestValue1, MediumTestValue2)
        };

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddRange_WithLargeNumberOfReadings_ShouldAddSuccessfully()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>();

        // Generate 1 year worth of 15-minute readings (35,040 readings)
        var startDate = new DateTime(Year2024, January, FirstDayOfMonth, MidnightHour, MinuteForQuarterOne, 0);
        var totalReadings = (DaysInLeapYear - 1) * HoursPerDay * QuartersPerHour;
        for (int i = 0; i < totalReadings; i++)
        {
            readings.Add(new MeterDataReading(
                startDate.AddMinutes(i * MinutesPerQuarter),
                DefaultPeekUsage,
                DefaultOffPeekUsage,
                DefaultHolidayUsage
            ));
        }

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region GetMonthsInRange Tests

    [Fact]
    public void GetMonthsInRange_WithSingleMonth_ShouldReturnOneMonth()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, March, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };
        manager.AddRange(readings);

        // Act
        var result = manager.GetMonthsInRange(Year2024, March, Year2024, March);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public void GetMonthsInRange_WithMultipleMonthsSameYear_ShouldReturnAllMonths()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, February, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, March, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, April, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };
        manager.AddRange(readings);

        // Act
        var result = manager.GetMonthsInRange(Year2024, January, Year2024, April);

        // Assert
        result.Should().HaveCount(4);
    }

    [Fact]
    public void GetMonthsInRange_WithMultipleYears_ShouldReturnAllMonths()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2023, November, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2023, December, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, February, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };
        manager.AddRange(readings);

        // Act - Nov 2023 to Feb 2024 = 4 months
        var result = manager.GetMonthsInRange(Year2023, November, Year2024, February);

        // Assert
        result.Should().HaveCount(4);
    }

    [Fact]
    public void GetMonthsInRange_WithMissingMonths_ShouldReturnOnlyExistingMonths()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            // Skip February
            CreateReading(Year2024, March, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            // Skip April
            CreateReading(Year2024, May, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };
        manager.AddRange(readings);

        // Act - Jan to May (5 months range, but only 3 have data)
        var result = manager.GetMonthsInRange(Year2024, January, Year2024, May);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public void GetMonthsInRange_WithInvalidRange_ShouldReturnEmptyList()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, March, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };
        manager.AddRange(readings);

        // Act - End before start
        var result = manager.GetMonthsInRange(Year2024, May, Year2024, March);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetMonthsInRange_WithNoData_ShouldReturnEmptyList()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);

        // Act
        var result = manager.GetMonthsInRange(Year2024, January, Year2024, December);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetMonthsInRange_WithFullYear_ShouldReturn12Months()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>();
        for (int month = January; month <= MonthsInYear; month++)
        {
            readings.Add(CreateReading(Year2024, month, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne));
        }
        manager.AddRange(readings);

        // Act
        var result = manager.GetMonthsInRange(Year2024, January, Year2024, December);

        // Assert
        result.Should().HaveCount(MonthsInYear);
    }

    [Fact]
    public void GetMonthsInRange_AcrossYearBoundary_ShouldReturnCorrectMonths()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2023, October, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2023, November, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2023, December, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, February, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, March, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };
        manager.AddRange(readings);

        // Act - Oct 2023 to Mar 2024 = 6 months
        var result = manager.GetMonthsInRange(Year2023, October, Year2024, March);

        // Assert
        result.Should().HaveCount(6);
    }

    [Fact]
    public void GetMonthsInRange_ShouldReturnMonthsInOrder()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, January, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, February, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, March, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };
        manager.AddRange(readings);

        // Act
        var result = manager.GetMonthsInRange(Year2024, January, Year2024, March);

        // Assert
        result.Should().HaveCount(3);
        // Verify they are MeterDataManagerMonth instances
        result.Should().AllBeOfType<MeterDataManagerMonth>();
    }

    [Fact]
    public void GetMonthsInRange_ReturnedMonths_ShouldHaveCorrectData()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(Year2024, March, MidMonth, MorningHour + QuartersPerHour, MinuteForQuarterOne),
            CreateReading(Year2024, March, Day16, MorningHour + QuartersPerHour, MinuteForQuarterOne)
        };
        manager.AddRange(readings);

        // Act
        var result = manager.GetMonthsInRange(Year2024, March, Year2024, March);

        // Assert
        result.Should().HaveCount(1);
        var marchData = result[0];
        marchData.Should().NotBeNull();
        marchData.MeterDataUsageInKwSummary.Should().NotBeNull();
        marchData.MeterDataUsageInMoneySummary.Should().NotBeNull();
    }

    #endregion

    #region SolarProductionDataSummary.Calculate Tests

    [Fact]
    public void Calculate_WhenSolarExceedsHolidayUsage_ShouldSetCorrectValues()
    {
        // Arrange
        var summary = new SolarProductionDataSummary(DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var meterUsage = new MeterDataUsageInKwSummary { Holiday = MeterUsage10Kw, PeekUsage = MeterUsage5Kw, OffPeekUsage = MeterUsage5Kw };

        // Act
        summary.Calculate(SolarProduction15Kw, meterUsage);

        // Assert
        summary.SolarProductionInKw.Should().Be(SolarProduction15Kw);
        summary.AmountOfSavedTouHolidayKw.Should().Be(MeterUsage10Kw);
        summary.SolarLostInKw.Should().Be(SolarLost5Kw);
        summary.AmountOfSavedTouPeekKw.Should().Be(ZeroValue);
        summary.AmountOfSavedTouOffPeekKw.Should().Be(ZeroValue);
        summary.AmountOfSavedFlatRateKw.Should().Be(MeterUsage10Kw);
    }

    [Fact]
    public void Calculate_WhenSolarLessThanHolidayUsage_ShouldSetCorrectValues()
    {
        // Arrange
        var summary = new SolarProductionDataSummary(DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var meterUsage = new MeterDataUsageInKwSummary { Holiday = MeterUsage20Kw, PeekUsage = MeterUsage10Kw, OffPeekUsage = MeterUsage10Kw };

        // Act
        summary.Calculate(SolarProduction10Kw, meterUsage);

        // Assert
        summary.SolarProductionInKw.Should().Be(SolarProduction10Kw);
        summary.AmountOfSavedTouHolidayKw.Should().Be(SolarProduction10Kw);
        summary.SolarLostInKw.Should().Be(ZeroValue);
        summary.AmountOfSavedTouPeekKw.Should().Be(ZeroValue);
        summary.AmountOfSavedTouOffPeekKw.Should().Be(ZeroValue);
        summary.AmountOfSavedFlatRateKw.Should().Be(SolarProduction10Kw);
    }

    [Fact]
    public void Calculate_WhenSolarExceedsPeekUsage_ShouldSetCorrectValues()
    {
        // Arrange
        var summary = new SolarProductionDataSummary(DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var meterUsage = new MeterDataUsageInKwSummary { PeekUsage = MeterUsage10Kw, OffPeekUsage = MeterUsage20Kw };

        // Act
        summary.Calculate(SolarProduction20Kw, meterUsage);

        // Assert
        summary.SolarProductionInKw.Should().Be(SolarProduction20Kw);
        summary.AmountOfSavedTouPeekKw.Should().Be(MeterUsage10Kw);
        summary.AmountOfSavedTouOffPeekKw.Should().Be(MeterUsage10Kw);
        summary.AmountOfSavedTouHolidayKw.Should().Be(ZeroValue);
        summary.SolarLostInKw.Should().Be(ZeroValue);
        summary.AmountOfSavedFlatRateKw.Should().Be(SolarProduction20Kw);
    }

    [Fact]
    public void Calculate_WhenSolarLessThanPeekUsage_ShouldSetCorrectValues()
    {
        // Arrange
        var summary = new SolarProductionDataSummary(DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var meterUsage = new MeterDataUsageInKwSummary { PeekUsage = MeterUsage20Kw, OffPeekUsage = MeterUsage20Kw };

        // Act
        summary.Calculate(SolarProduction10Kw, meterUsage);

        // Assert
        summary.SolarProductionInKw.Should().Be(SolarProduction10Kw);
        summary.AmountOfSavedTouPeekKw.Should().Be(SolarProduction10Kw);
        summary.AmountOfSavedTouOffPeekKw.Should().Be(ZeroValue);
        summary.AmountOfSavedTouHolidayKw.Should().Be(ZeroValue);
        summary.SolarLostInKw.Should().Be(ZeroValue);
        summary.AmountOfSavedFlatRateKw.Should().Be(SolarProduction10Kw);
    }

    [Fact]
    public void Calculate_WhenSolarExceedsTotalUsage_ShouldSetCorrectValues()
    {
        // Arrange
        var summary = new SolarProductionDataSummary(DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var meterUsage = new MeterDataUsageInKwSummary { PeekUsage = MeterUsage10Kw, OffPeekUsage = MeterUsage15Kw };

        // Act
        summary.Calculate(SolarProduction30Kw, meterUsage);

        // Assert
        summary.SolarProductionInKw.Should().Be(SolarProduction30Kw);
        summary.AmountOfSavedFlatRateKw.Should().Be(SolarProduction25Kw);
        summary.SolarLostInKw.Should().Be(SolarLost5Kw);
    }

    [Fact]
    public void Calculate_WithZeroSolarProduction_ShouldResetAllValues()
    {
        // Arrange
        var summary = new SolarProductionDataSummary(DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var meterUsage = new MeterDataUsageInKwSummary { PeekUsage = MeterUsage10Kw, OffPeekUsage = MeterUsage20Kw };

        // Act
        summary.Calculate(ZeroValue, meterUsage);

        // Assert
        summary.SolarProductionInKw.Should().Be(ZeroValue);
        summary.AmountOfSavedTouPeekKw.Should().Be(ZeroValue);
        summary.AmountOfSavedTouOffPeekKw.Should().Be(ZeroValue);
        summary.AmountOfSavedTouHolidayKw.Should().Be(ZeroValue);
        summary.SolarLostInKw.Should().Be(ZeroValue);
        summary.AmountOfSavedFlatRateKw.Should().Be(ZeroValue);
    }

    [Fact]
    public void Calculate_CalledMultipleTimes_ShouldResetPreviousValues()
    {
        // Arrange
        var summary = new SolarProductionDataSummary(DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var meterUsage1 = new MeterDataUsageInKwSummary { PeekUsage = MeterUsage10Kw, OffPeekUsage = MeterUsage20Kw };
        var meterUsage2 = new MeterDataUsageInKwSummary { Holiday = MeterUsage5Kw, PeekUsage = MeterUsage2Kw, OffPeekUsage = MeterUsage3Kw };

        // Act - First calculation
        summary.Calculate(SolarProduction20Kw, meterUsage1);

        // Verify first calculation
        summary.AmountOfSavedTouPeekKw.Should().Be(MeterUsage10Kw);
        summary.AmountOfSavedTouOffPeekKw.Should().Be(MeterUsage10Kw);

        // Act - Second calculation with different data
        summary.Calculate(SolarProduction8Kw, meterUsage2);

        // Assert - Values should be reset and recalculated
        summary.SolarProductionInKw.Should().Be(SolarProduction8Kw);
        summary.AmountOfSavedTouHolidayKw.Should().Be(MeterUsage5Kw);
        summary.AmountOfSavedTouPeekKw.Should().Be(ZeroValue); // Should be reset
        summary.AmountOfSavedTouOffPeekKw.Should().Be(ZeroValue); // Should be reset
        summary.SolarLostInKw.Should().Be(SolarLost3Kw);
        summary.AmountOfSavedFlatRateKw.Should().Be(MeterUsage5Kw);
    }

    [Fact]
    public void Calculate_WhenSolarEqualsPeekUsage_ShouldSetCorrectValues()
    {
        // Arrange
        var summary = new SolarProductionDataSummary(DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var meterUsage = new MeterDataUsageInKwSummary { PeekUsage = MeterUsage15Kw, OffPeekUsage = MeterUsage15Kw };

        // Act
        summary.Calculate(SolarProduction15Kw, meterUsage);

        // Assert
        summary.SolarProductionInKw.Should().Be(SolarProduction15Kw);
        summary.AmountOfSavedTouPeekKw.Should().Be(SolarProduction15Kw);
        summary.AmountOfSavedTouOffPeekKw.Should().Be(ZeroValue);
        summary.AmountOfSavedFlatRateKw.Should().Be(SolarProduction15Kw);
        summary.SolarLostInKw.Should().Be(ZeroValue);
    }

    [Fact]
    public void Calculate_WhenSolarEqualsHolidayUsage_ShouldSetCorrectValues()
    {
        // Arrange
        var summary = new SolarProductionDataSummary(DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var meterUsage = new MeterDataUsageInKwSummary { Holiday = SolarProduction12Kw, PeekUsage = MeterUsage6Kw, OffPeekUsage = MeterUsage6Kw };

        // Act
        summary.Calculate(SolarProduction12Kw, meterUsage);

        // Assert
        summary.SolarProductionInKw.Should().Be(SolarProduction12Kw);
        summary.AmountOfSavedTouHolidayKw.Should().Be(SolarProduction12Kw);
        summary.AmountOfSavedFlatRateKw.Should().Be(SolarProduction12Kw);
        summary.SolarLostInKw.Should().Be(ZeroValue);
    }

    [Fact]
    public void Calculate_WhenSolarEqualsTotalUsage_ShouldSetCorrectValues()
    {
        // Arrange
        var summary = new SolarProductionDataSummary(DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var meterUsage = new MeterDataUsageInKwSummary { PeekUsage = MeterUsage10Kw, OffPeekUsage = MeterUsage15Kw };

        // Act
        summary.Calculate(SolarProduction25Kw, meterUsage);

        // Assert
        summary.SolarProductionInKw.Should().Be(SolarProduction25Kw);
        summary.AmountOfSavedFlatRateKw.Should().Be(SolarProduction25Kw);
        summary.SolarLostInKw.Should().Be(ZeroValue);
    }

    #endregion

    #region Helper Methods

    private static MeterDataReading CreateReading(int year, int month, int day, int hour, int minute)
    {
        var periodStart = new DateTime(year, month, day, hour, minute, 0);
        return new MeterDataReading(periodStart, DefaultPeekUsage, DefaultOffPeekUsage, DefaultHolidayUsage);
    }

    #endregion
}
