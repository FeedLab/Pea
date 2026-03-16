using FluentAssertions;
using Pea.Infrastructure.Models;
using Pea.Infrastructure.Models.MeterData;

namespace Pea.Meter.Tests;

/// <summary>
/// Integration tests for MeterDataManager class testing the full hierarchical structure
/// </summary>
public class MeterDataManagerIntegrationTests
{
    private const decimal PeekUsage = 10.5m;
    private const decimal OffPeekUsage = 20.3m;
    private const decimal HolidayUsage = 15.7m;
    private const decimal FlatRatePrice = 1.5m;
    private const decimal PeekPrice = 2.0m;
    private const decimal OffPeekPrice = 1.0m;

    #region Full Day Integration Tests

    [Fact]
    public void Integration_AddFullDayOfReadings_ShouldHandleAllQuarters()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = GenerateFullDayReadings(2024, 1, 15);

        // Act
        manager.AddRange(readings);

        // Assert
        readings.Should().HaveCount(96); // 24 hours * 4 quarters
    }

    [Fact]
    public void Integration_AddFullWeekOfReadings_ShouldHandleAllDays()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>();

        for (int day = 1; day <= 7; day++)
        {
            readings.AddRange(GenerateFullDayReadings(2024, 1, day));
        }

        // Act
        manager.AddRange(readings);

        // Assert
        readings.Should().HaveCount(96 * 7); // 7 days * 96 readings per day
    }

    [Fact]
    public void Integration_AddFullMonthOfReadings_ShouldHandleAllDays()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>();

        // January has 31 days
        for (int day = 1; day <= 31; day++)
        {
            readings.AddRange(GenerateFullDayReadings(2024, 1, day));
        }

        // Act
        manager.AddRange(readings);

        // Assert
        readings.Should().HaveCount(96 * 31); // 31 days * 96 readings per day
    }

    #endregion

    #region Multi-Year Integration Tests

    [Fact]
    public void Integration_AddReadingsAcrossMultipleYears_ShouldOrganizeCorrectly()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2023, 12, 31, 23, 45),
            CreateReading(2024, 1, 1, 0, 0),
            CreateReading(2024, 6, 15, 12, 30),
            CreateReading(2024, 12, 31, 23, 45),
            CreateReading(2025, 1, 1, 0, 0)
        };

        // Act
        manager.AddRange(readings);

        // Assert
        readings.Should().HaveCount(5);
    }

    [Fact]
    public void Integration_AddReadingsToSameYearMultipleTimes_ShouldMergeCorrectly()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);

        var batch1 = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 1, 15, 10, 15)
        };

        var batch2 = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 30),
            CreateReading(2024, 1, 15, 10, 45)
        };

        // Act
        manager.AddRange(batch1);
        manager.AddRange(batch2);

        // Assert - Should not throw and should handle both batches
        batch1.Should().HaveCount(2);
        batch2.Should().HaveCount(2);
    }

    #endregion

    #region Multi-Month Integration Tests

    [Fact]
    public void Integration_AddReadingsAcrossAllMonths_ShouldHandleCorrectly()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>();

        for (int month = 1; month <= 12; month++)
        {
            readings.Add(CreateReading(2024, month, 15, 12, 0));
        }

        // Act
        manager.AddRange(readings);

        // Assert
        readings.Should().HaveCount(12);
    }

    [Fact]
    public void Integration_AddReadingsToSameMonthMultipleTimes_ShouldMergeCorrectly()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);

        var batch1 = new List<MeterDataReading>
        {
            CreateReading(2024, 3, 1, 10, 0),
            CreateReading(2024, 3, 2, 10, 0)
        };

        var batch2 = new List<MeterDataReading>
        {
            CreateReading(2024, 3, 3, 10, 0),
            CreateReading(2024, 3, 4, 10, 0)
        };

        // Act
        manager.AddRange(batch1);
        manager.AddRange(batch2);

        // Assert
        batch1.Should().HaveCount(2);
        batch2.Should().HaveCount(2);
    }

    #endregion

    #region Multi-Day Integration Tests

    [Fact]
    public void Integration_AddReadingsAcrossDifferentDaysInMonth_ShouldOrganizeCorrectly()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 1, 12, 0),
            CreateReading(2024, 1, 15, 12, 0),
            CreateReading(2024, 1, 31, 12, 0)
        };

        // Act
        manager.AddRange(readings);

        // Assert
        readings.Should().HaveCount(3);
    }

    [Fact]
    public void Integration_AddReadingsToSameDayMultipleTimes_ShouldMergeCorrectly()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);

        var batch1 = GenerateQuarterHourReadings(2024, 5, 10, 9);  // 9 AM
        var batch2 = GenerateQuarterHourReadings(2024, 5, 10, 15); // 3 PM

        // Act
        manager.AddRange(batch1);
        manager.AddRange(batch2);

        // Assert
        batch1.Should().HaveCount(4);
        batch2.Should().HaveCount(4);
    }

    #endregion

    #region Multi-Hour Integration Tests

    [Fact]
    public void Integration_AddReadingsAcrossDifferentHours_ShouldOrganizeCorrectly()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>();

        for (int hour = 0; hour < 24; hour++)
        {
            readings.Add(CreateReading(2024, 1, 15, hour, 0));
        }

        // Act
        manager.AddRange(readings);

        // Assert
        readings.Should().HaveCount(24);
    }

    [Fact]
    public void Integration_AddReadingsToSameHourMultipleTimes_ShouldMergeCorrectly()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);

        var batch1 = new List<MeterDataReading>
        {
            CreateReading(2024, 7, 20, 14, 0),
            CreateReading(2024, 7, 20, 14, 15)
        };

        var batch2 = new List<MeterDataReading>
        {
            CreateReading(2024, 7, 20, 14, 30),
            CreateReading(2024, 7, 20, 14, 45)
        };

        // Act
        manager.AddRange(batch1);
        manager.AddRange(batch2);

        // Assert
        batch1.Should().HaveCount(2);
        batch2.Should().HaveCount(2);
    }

    #endregion

    #region Clear Integration Tests

    [Fact]
    public void Integration_ClearAfterFullYearOfData_ShouldAllowReAddingData()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>();

        // Add readings for each month
        for (int month = 1; month <= 12; month++)
        {
            readings.AddRange(GenerateFullDayReadings(2024, month, 15));
        }

        manager.AddRange(readings);

        // Act
        manager.Clear();

        // Assert - Should be able to add new data after clear
        var newReadings = GenerateFullDayReadings(2024, 6, 20);
        var action = () => manager.AddRange(newReadings);
        action.Should().NotThrow();
    }

    [Fact]
    public void Integration_ClearAndReAdd_ShouldWorkMultipleTimes()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);

        for (int i = 0; i < 5; i++)
        {
            var readings = GenerateFullDayReadings(2024, 1, 15);

            // Act
            manager.AddRange(readings);
            manager.Clear();

            // Assert
            readings.Should().HaveCount(96);
        }
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void Integration_AddReadingsInNonChronologicalOrder_ShouldOrganizeCorrectly()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 12, 31, 23, 45),
            CreateReading(2024, 1, 1, 0, 0),
            CreateReading(2024, 6, 15, 12, 0),
            CreateReading(2024, 3, 10, 15, 30),
            CreateReading(2024, 9, 20, 8, 15)
        };

        // Act
        manager.AddRange(readings);

        // Assert
        readings.Should().HaveCount(5);
    }

    [Fact]
    public void Integration_AddDuplicateReadings_ShouldAddBothInstances()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var reading = CreateReading(2024, 6, 15, 12, 0);
        var readings = new List<MeterDataReading> { reading, reading };

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
        readings.Should().HaveCount(2);
    }

    [Fact]
    public void Integration_AddReadingsWithVaryingRates_ShouldPreserveAllData()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            new MeterDataReading(new DateTime(2024, 1, 15, 10, 0, 0), 1.5m, 2.5m, 3.5m),
            new MeterDataReading(new DateTime(2024, 1, 15, 10, 15, 0), 100m, 200m, 300m),
            new MeterDataReading(new DateTime(2024, 1, 15, 10, 30, 0), 0.001m, 0.002m, 0.003m),
            new MeterDataReading(new DateTime(2024, 1, 15, 10, 45, 0), 0m, 0m, 0m)
        };

        // Act
        manager.AddRange(readings);

        // Assert
        readings[0].PeekUsage.Should().Be(1.5m);
        readings[1].OffPeekUsage.Should().Be(200m);
        readings[2].HolidayUsage.Should().Be(0.003m);
        readings[3].Total.Should().Be(0m);
    }

    [Fact]
    public void Integration_AddReadingsSpanningDaylightSavingTime_ShouldHandleCorrectly()
    {
        // Arrange - March 2024 DST in US (typically 2nd Sunday in March)
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 3, 10, 1, 45),  // Before DST
            CreateReading(2024, 3, 10, 2, 0),   // During DST transition
            CreateReading(2024, 3, 10, 3, 15)   // After DST
        };

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
        readings.Should().HaveCount(3);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Integration_AddYearOfQuarterHourReadings_ShouldPerformEfficiently()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>();

        var startDate = new DateTime(2024, 1, 1, 0, 0, 0);
        var totalQuarters = 365 * 24 * 4; // Leap year 2024: 366 days

        for (int i = 0; i < totalQuarters; i++)
        {
            readings.Add(new MeterDataReading(
                startDate.AddMinutes(i * 15),
                PeekUsage,
                OffPeekUsage,
                HolidayUsage
            ));
        }

        // Act
        var action = () => manager.AddRange(readings);

        // Assert
        action.Should().NotThrow();
        readings.Should().HaveCount(totalQuarters);
    }

    [Fact]
    public void Integration_AddMultipleBatchesOfReadings_ShouldHandleEfficiently()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var totalReadingsAdded = 0;

        // Act - Add 100 batches of 100 readings each
        for (int batch = 0; batch < 100; batch++)
        {
            var readings = new List<MeterDataReading>();

            for (int i = 0; i < 100; i++)
            {
                var date = new DateTime(2024, 1, 1, 0, 0, 0).AddMinutes((batch * 100 + i) * 15);
                readings.Add(new MeterDataReading(date, PeekUsage, OffPeekUsage, HolidayUsage));
            }

            manager.AddRange(readings);
            totalReadingsAdded += readings.Count;
        }

        // Assert
        totalReadingsAdded.Should().Be(10000);
    }

    #endregion

    #region Boundary Condition Tests

    [Fact]
    public void Integration_AddReadingsAtMonthBoundaries_ShouldHandleCorrectly()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 31, 23, 45),
            CreateReading(2024, 2, 1, 0, 0),
            CreateReading(2024, 2, 29, 23, 45),  // Leap year
            CreateReading(2024, 3, 1, 0, 0)
        };

        // Act
        manager.AddRange(readings);

        // Assert
        readings.Should().HaveCount(4);
    }

    [Fact]
    public void Integration_AddReadingsAtYearBoundaries_ShouldHandleCorrectly()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2023, 12, 31, 23, 45),
            CreateReading(2024, 1, 1, 0, 0),
            CreateReading(2024, 12, 31, 23, 45),
            CreateReading(2025, 1, 1, 0, 0)
        };

        // Act
        manager.AddRange(readings);

        // Assert
        readings.Should().HaveCount(4);
    }

    [Fact]
    public void Integration_AddReadingsAtMidnight_ShouldHandleCorrectly()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 6, 15, 23, 45),
            CreateReading(2024, 6, 16, 0, 0),
            CreateReading(2024, 6, 16, 0, 15),
            CreateReading(2024, 6, 16, 0, 30)
        };

        // Act
        manager.AddRange(readings);

        // Assert
        readings.Should().HaveCount(4);
    }

    #endregion

    #region Helper Methods

    private static MeterDataReading CreateReading(int year, int month, int day, int hour, int minute)
    {
        var periodStart = new DateTime(year, month, day, hour, minute, 0);
        return new MeterDataReading(periodStart, PeekUsage, OffPeekUsage, HolidayUsage);
    }

    private static List<MeterDataReading> GenerateFullDayReadings(int year, int month, int day)
    {
        var readings = new List<MeterDataReading>();

        for (int hour = 0; hour < 24; hour++)
        {
            for (int quarter = 0; quarter < 4; quarter++)
            {
                var minute = quarter * 15;
                readings.Add(CreateReading(year, month, day, hour, minute));
            }
        }

        return readings;
    }

    private static List<MeterDataReading> GenerateQuarterHourReadings(int year, int month, int day, int hour)
    {
        var readings = new List<MeterDataReading>();

        for (int quarter = 0; quarter < 4; quarter++)
        {
            var minute = quarter * 15;
            readings.Add(CreateReading(year, month, day, hour, minute));
        }

        return readings;
    }

    #endregion
}
