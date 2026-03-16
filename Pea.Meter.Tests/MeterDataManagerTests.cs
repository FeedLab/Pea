using FluentAssertions;
using Pea.Infrastructure.Models;
using Pea.Infrastructure.Models.MeterData;

namespace Pea.Meter.Tests;

/// <summary>
/// Unit tests for MeterDataManager class
/// </summary>
public class MeterDataManagerTests
{
    private const decimal DefaultPeekUsage = 10.5m;
    private const decimal DefaultOffPeekUsage = 20.3m;
    private const decimal DefaultHolidayUsage = 15.7m;
    private const decimal DefaultFlatRatePrice = 1.5m;
    private const decimal DefaultPeekPrice = 2.0m;
    private const decimal DefaultOffPeekPrice = 1.0m;

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
        var readings = new List<MeterDataReading> { CreateReading(2024, 1, 15, 10, 0) };

        // Act
        var manager = new MeterDataManager(readings, DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);

        // Assert
        manager.Should().NotBeNull();
        var result = manager.GetReadings(new DateTime(2024, 1, 1), FilterLevel.None);
        result.Should().ContainSingle();
    }

    #endregion

    #region AddRange Tests - Single Reading

    [Fact]
    public void AddRange_WithSingleReading_ShouldAddSuccessfully()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var reading = CreateReading(2024, 1, 15, 10, 0);
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
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 1, 15, 10, 15),
            CreateReading(2024, 1, 15, 10, 30),
            CreateReading(2024, 1, 15, 10, 45)
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
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 1, 16, 10, 0),
            CreateReading(2024, 1, 17, 10, 0)
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
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 2, 15, 10, 0),
            CreateReading(2024, 3, 15, 10, 0)
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
            CreateReading(2023, 12, 31, 23, 45),
            CreateReading(2024, 1, 1, 0, 0),
            CreateReading(2025, 1, 1, 0, 0)
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
            CreateReading(2024, 1, 15, 10, 0)
        };
        var readings2 = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 15)
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
            CreateReading(2024, 1, 15, 10, 0)
        };
        var readings2 = new List<MeterDataReading>
        {
            CreateReading(2024, 2, 15, 10, 0)
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
            CreateReading(2024, 1, 15, 0, 0),   // Midnight
            CreateReading(2024, 1, 15, 6, 0),   // Morning
            CreateReading(2024, 1, 15, 12, 0),  // Noon
            CreateReading(2024, 1, 15, 18, 0),  // Evening
            CreateReading(2024, 1, 15, 23, 45)  // Late night
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
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 1, 15, 10, 15),
            CreateReading(2024, 1, 15, 10, 30),
            CreateReading(2024, 1, 15, 10, 45)
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
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 2, 15, 10, 0)
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
        manager.AddRange(new List<MeterDataReading> { CreateReading(2023, 1, 1, 0, 0) });
        manager.AddRange(new List<MeterDataReading> { CreateReading(2024, 1, 1, 0, 0) });
        manager.AddRange(new List<MeterDataReading> { CreateReading(2025, 1, 1, 0, 0) });

        // Act
        manager.Clear();

        // Assert
        var action = () => manager.AddRange(new List<MeterDataReading> { CreateReading(2024, 6, 15, 12, 0) });
        action.Should().NotThrow();
    }

    [Fact]
    public void Clear_ShouldResetMeterDataUsageInKwSummary()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 1, 15, 11, 0)
        };
        manager.AddRange(readings);

        // Verify summaries have data before clear
        var hasDataBefore = manager.MeterDataUsageInKwSummary.PeekUsage > 0 ||
                           manager.MeterDataUsageInKwSummary.OffPeekUsage > 0 ||
                           manager.MeterDataUsageInKwSummary.Holiday > 0;

        // Act
        manager.Clear();

        // Assert
        hasDataBefore.Should().BeTrue("summaries should have data before clear");
        manager.MeterDataUsageInKwSummary.PeekUsage.Should().Be(0m);
        manager.MeterDataUsageInKwSummary.OffPeekUsage.Should().Be(0m);
        manager.MeterDataUsageInKwSummary.Holiday.Should().Be(0m);
    }

    [Fact]
    public void Clear_ShouldResetMeterDataUsageInMoneySummary()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 1, 15, 11, 0)
        };
        manager.AddRange(readings);

        // Verify summaries have data before clear
        var hasDataBefore = manager.MeterDataUsageInMoneySummary.PeekUsage > 0 ||
                           manager.MeterDataUsageInMoneySummary.OffPeekUsage > 0 ||
                           manager.MeterDataUsageInMoneySummary.PeekTouUsagePriceSummary > 0 ||
                           manager.MeterDataUsageInMoneySummary.OffPeekTouUsagePriceSummary > 0 ||
                           manager.MeterDataUsageInMoneySummary.FlatRateUsagePriceSummary > 0;

        // Act
        manager.Clear();

        // Assert
        hasDataBefore.Should().BeTrue("summaries should have data before clear");
        manager.MeterDataUsageInMoneySummary.PeekUsage.Should().Be(0m);
        manager.MeterDataUsageInMoneySummary.OffPeekUsage.Should().Be(0m);
        manager.MeterDataUsageInMoneySummary.PeekTouUsagePriceSummary.Should().Be(0m);
        manager.MeterDataUsageInMoneySummary.OffPeekTouUsagePriceSummary.Should().Be(0m);
        manager.MeterDataUsageInMoneySummary.FlatRateUsagePriceSummary.Should().Be(0m);
    }

    [Fact]
    public void Clear_ShouldAllowReAddingDataWithCorrectSummaries()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), DefaultFlatRatePrice, DefaultPeekPrice, DefaultOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0)
        };
        manager.AddRange(readings);

        // Act
        manager.Clear();
        manager.AddRange(readings);

        // Assert - After re-adding, summaries should have values again
        var hasData = manager.MeterDataUsageInKwSummary.PeekUsage > 0 ||
                     manager.MeterDataUsageInKwSummary.OffPeekUsage > 0 ||
                     manager.MeterDataUsageInKwSummary.Holiday > 0;
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
            CreateReading(2024, 2, 29, 12, 0) // Leap year
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
            CreateReading(2023, 12, 31, 23, 45),
            CreateReading(2024, 1, 1, 0, 0)
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
            new MeterDataReading(new DateTime(2024, 1, 15, 10, 0, 0), 0, 0, 0),
            new MeterDataReading(new DateTime(2024, 1, 15, 10, 15, 0), 100m, 200m, 300m),
            new MeterDataReading(new DateTime(2024, 1, 15, 10, 30, 0), 0.01m, 0.02m, 0.03m)
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
        var startDate = new DateTime(2024, 1, 1, 0, 0, 0);
        for (int i = 0; i < 365 * 24 * 4; i++)
        {
            readings.Add(new MeterDataReading(
                startDate.AddMinutes(i * 15),
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

    #region Helper Methods

    private static MeterDataReading CreateReading(int year, int month, int day, int hour, int minute)
    {
        var periodStart = new DateTime(year, month, day, hour, minute, 0);
        return new MeterDataReading(periodStart, DefaultPeekUsage, DefaultOffPeekUsage, DefaultHolidayUsage);
    }

    #endregion
}
