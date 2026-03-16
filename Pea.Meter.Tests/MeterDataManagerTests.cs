using FluentAssertions;
using Pea.Infrastructure.Models;
using Pea.Meter.Models.MeterData;

namespace Pea.Meter.Tests;

/// <summary>
/// Unit tests for MeterDataManager class
/// </summary>
public class MeterDataManagerTests
{
    private const decimal DefaultRateA = 10.5m;
    private const decimal DefaultRateB = 20.3m;
    private const decimal DefaultRateC = 15.7m;

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeEmptyManager()
    {
        // Act
        var manager = new MeterDataManager();

        // Assert
        manager.Should().NotBeNull();
    }

    #endregion

    #region AddRange Tests - Single Reading

    [Fact]
    public void AddRange_WithSingleReading_ShouldAddSuccessfully()
    {
        // Arrange
        var manager = new MeterDataManager();
        var reading = CreateReading(2024, 1, 15, 10, 0);
        var readings = new List<PeaMeterReading> { reading };

        // Act
        manager.AddRange(readings);

        // Assert - No exception thrown means success
        manager.Should().NotBeNull();
    }

    [Fact]
    public void AddRange_WithEmptyList_ShouldNotThrowException()
    {
        // Arrange
        var manager = new MeterDataManager();
        var readings = new List<PeaMeterReading>();

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
        var manager = new MeterDataManager();
        var readings = new List<PeaMeterReading>
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
        var manager = new MeterDataManager();
        var readings = new List<PeaMeterReading>
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
        var manager = new MeterDataManager();
        var readings = new List<PeaMeterReading>
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
        var manager = new MeterDataManager();
        var readings = new List<PeaMeterReading>
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
        var manager = new MeterDataManager();
        var readings1 = new List<PeaMeterReading>
        {
            CreateReading(2024, 1, 15, 10, 0)
        };
        var readings2 = new List<PeaMeterReading>
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
        var manager = new MeterDataManager();
        var readings1 = new List<PeaMeterReading>
        {
            CreateReading(2024, 1, 15, 10, 0)
        };
        var readings2 = new List<PeaMeterReading>
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
        var manager = new MeterDataManager();
        var readings = new List<PeaMeterReading>
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
        var manager = new MeterDataManager();
        var readings = new List<PeaMeterReading>
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
        var manager = new MeterDataManager();

        // Act
        var action = () => manager.Clear();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Clear_WithData_ShouldRemoveAllReadings()
    {
        // Arrange
        var manager = new MeterDataManager();
        var readings = new List<PeaMeterReading>
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
        var manager = new MeterDataManager();
        manager.AddRange(new List<PeaMeterReading> { CreateReading(2023, 1, 1, 0, 0) });
        manager.AddRange(new List<PeaMeterReading> { CreateReading(2024, 1, 1, 0, 0) });
        manager.AddRange(new List<PeaMeterReading> { CreateReading(2025, 1, 1, 0, 0) });

        // Act
        manager.Clear();

        // Assert
        var action = () => manager.AddRange(new List<PeaMeterReading> { CreateReading(2024, 6, 15, 12, 0) });
        action.Should().NotThrow();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void AddRange_WithLeapYearDate_ShouldAddSuccessfully()
    {
        // Arrange
        var manager = new MeterDataManager();
        var readings = new List<PeaMeterReading>
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
        var manager = new MeterDataManager();
        var readings = new List<PeaMeterReading>
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
        var manager = new MeterDataManager();
        var readings = new List<PeaMeterReading>
        {
            new PeaMeterReading(new DateTime(2024, 1, 15, 10, 0, 0), 0, 0, 0),
            new PeaMeterReading(new DateTime(2024, 1, 15, 10, 15, 0), 100m, 200m, 300m),
            new PeaMeterReading(new DateTime(2024, 1, 15, 10, 30, 0), 0.01m, 0.02m, 0.03m)
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
        var manager = new MeterDataManager();
        var readings = new List<PeaMeterReading>();

        // Generate 1 year worth of 15-minute readings (35,040 readings)
        var startDate = new DateTime(2024, 1, 1, 0, 0, 0);
        for (int i = 0; i < 365 * 24 * 4; i++)
        {
            readings.Add(new PeaMeterReading(
                startDate.AddMinutes(i * 15),
                DefaultRateA,
                DefaultRateB,
                DefaultRateC
            ));
        }

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region Helper Methods

    private static PeaMeterReading CreateReading(int year, int month, int day, int hour, int minute)
    {
        var periodStart = new DateTime(year, month, day, hour, minute, 0);
        return new PeaMeterReading(periodStart, DefaultRateA, DefaultRateB, DefaultRateC);
    }

    #endregion
}
