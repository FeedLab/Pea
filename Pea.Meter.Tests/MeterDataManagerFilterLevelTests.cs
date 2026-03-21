using FluentAssertions;
using Pea.Infrastructure.Models;
using Pea.Infrastructure.Models.MeterData;

namespace Pea.Meter.Tests;

/// <summary>
/// Tests for MeterDataManager.GetReadings() method using FilterLevel enum
/// </summary>
public class MeterDataManagerFilterLevelTests
{
    private const decimal PeekUsage = 10.5m;
    private const decimal OffPeekUsage = 20.3m;
    private const decimal HolidayUsage = 15.7m;
    private const decimal FlatRatePrice = 1.5m;
    private const decimal PeekPrice = 2.0m;
    private const decimal OffPeekPrice = 1.0m;

    private const int QuartersPerHour = 4;
    private const int HoursPerDay = 24;
    private const int ReadingsPerDay = QuartersPerHour * HoursPerDay; // 96
    private const int MinutesPerQuarter = 15;
    private const int DaysInLeapYear = 366;

    #region FilterLevel.None Tests

    [Fact]
    public void GetReadings_WithFilterLevelNone_ShouldReturnAllReadings()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 2, 20, 14, 30),
            CreateReading(2025, 6, 10, 8, 45)
        };
        manager.AddRange(readings);

        // Act
        var result = manager.GetReadings(new DateTime(2024, 1, 1), FilterLevel.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(readings);
    }

    [Fact]
    public void GetReadings_WithDefaultFilterLevel_ShouldReturnAllReadings()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 2, 20, 14, 30)
        };
        manager.AddRange(readings);

        // Act
        var result = manager.GetReadings(new DateTime(2024, 1, 1));

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(readings);
    }

    #endregion

    #region FilterLevel.Year Tests

    [Fact]
    public void GetReadings_WithFilterLevelYear_ShouldReturnYearReadings()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings2024 = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 6, 20, 14, 30),
            CreateReading(2024, 12, 25, 8, 45)
        };
        var readings2025 = new List<MeterDataReading>
        {
            CreateReading(2025, 1, 1, 0, 0)
        };

        manager.AddRange(readings2024);
        manager.AddRange(readings2025);

        // Act
        var result = manager.GetReadings(new DateTime(2024, 1, 1), FilterLevel.Year);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(readings2024);
        result.All(r => r.PeriodStart.Year == 2024).Should().BeTrue();
    }

    [Fact]
    public void GetReadings_WithFilterLevelYear_NonExistentYear_ShouldReturnAllReadings()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0)
        };
        manager.AddRange(readings);

        // Act
        var result = manager.GetReadings(new DateTime(2023, 1, 1), FilterLevel.Year);

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(readings);
    }

    #endregion

    #region FilterLevel.Month Tests

    [Fact]
    public void GetReadings_WithFilterLevelMonth_ShouldReturnMonthReadings()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readingsJan = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 5, 10, 0),
            CreateReading(2024, 1, 15, 14, 30),
            CreateReading(2024, 1, 25, 8, 45)
        };
        var readingsFeb = new List<MeterDataReading>
        {
            CreateReading(2024, 2, 10, 12, 0)
        };

        manager.AddRange(readingsJan);
        manager.AddRange(readingsFeb);

        // Act
        var result = manager.GetReadings(new DateTime(2024, 1, 1), FilterLevel.Month);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(readingsJan);
        result.All(r => r.PeriodStart.Year == 2024 && r.PeriodStart.Month == 1).Should().BeTrue();
    }

    [Fact]
    public void GetReadings_WithFilterLevelMonth_NonExistentMonth_ShouldReturnYearReadings()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 2, 20, 14, 30)
        };
        manager.AddRange(readings);

        // Act
        var result = manager.GetReadings(new DateTime(2024, 12, 1), FilterLevel.Month);

        // Assert
        result.Should().HaveCount(2);
        result.All(r => r.PeriodStart.Year == 2024).Should().BeTrue();
    }

    #endregion

    #region FilterLevel.Day Tests

    [Fact]
    public void GetReadings_WithFilterLevelDay_ShouldReturnDayReadings()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readingsDay15 = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 0, 0),
            CreateReading(2024, 1, 15, 12, 30),
            CreateReading(2024, 1, 15, 23, 45)
        };
        var readingsDay20 = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 20, 10, 0)
        };

        manager.AddRange(readingsDay15);
        manager.AddRange(readingsDay20);

        // Act
        var result = manager.GetReadings(new DateTime(2024, 1, 15), FilterLevel.Day);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(readingsDay15);
        result.All(r => r.PeriodStart.Date == new DateTime(2024, 1, 15)).Should().BeTrue();
    }

    [Fact]
    public void GetReadings_WithFilterLevelDay_NonExistentDay_ShouldReturnMonthReadings()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 1, 20, 14, 30)
        };
        manager.AddRange(readings);

        // Act
        var result = manager.GetReadings(new DateTime(2024, 1, 25), FilterLevel.Day);

        // Assert
        result.Should().HaveCount(2);
        result.All(r => r.PeriodStart.Month == 1).Should().BeTrue();
    }

    #endregion

    #region FilterLevel.Hour Tests

    [Fact]
    public void GetReadings_WithFilterLevelHour_ShouldReturnHourReadings()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readingsHour10 = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 1, 15, 10, 15),
            CreateReading(2024, 1, 15, 10, 30),
            CreateReading(2024, 1, 15, 10, 45)
        };
        var readingsHour14 = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 14, 0)
        };

        manager.AddRange(readingsHour10);
        manager.AddRange(readingsHour14);

        // Act
        var result = manager.GetReadings(new DateTime(2024, 1, 15, 10, 0, 0), FilterLevel.Hour);

        // Assert
        result.Should().HaveCount(4);
        result.Should().BeEquivalentTo(readingsHour10);
        result.All(r => r.PeriodStart.Hour == 10).Should().BeTrue();
    }

    [Fact]
    public void GetReadings_WithFilterLevelHour_NonExistentHour_ShouldReturnDayReadings()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 1, 15, 14, 30)
        };
        manager.AddRange(readings);

        // Act
        var result = manager.GetReadings(new DateTime(2024, 1, 15, 18, 0, 0), FilterLevel.Hour);

        // Assert
        result.Should().HaveCount(2);
        result.All(r => r.PeriodStart.Day == 15).Should().BeTrue();
    }

    [Fact]
    public void GetReadings_WithFilterLevelHour_Midnight_ShouldWork()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readingsMidnight = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 0, 0),
            CreateReading(2024, 1, 15, 0, 15),
            CreateReading(2024, 1, 15, 0, 30),
            CreateReading(2024, 1, 15, 0, 45)
        };
        manager.AddRange(readingsMidnight);

        // Act
        var result = manager.GetReadings(new DateTime(2024, 1, 15, 0, 0, 0), FilterLevel.Hour);

        // Assert
        result.Should().HaveCount(4);
        result.All(r => r.PeriodStart.Hour == 0).Should().BeTrue();
    }

    #endregion

    #region FilterLevel.Quarter Tests

    [Fact]
    public void GetReadings_WithFilterLevelQuarter_ShouldReturnQuarterReadings()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readingsQ0 = new List<MeterDataReading> { CreateReading(2024, 1, 15, 10, 0) };
        var readingsQ1 = new List<MeterDataReading> { CreateReading(2024, 1, 15, 10, 15) };
        var readingsQ2 = new List<MeterDataReading> { CreateReading(2024, 1, 15, 10, 30) };
        var readingsQ3 = new List<MeterDataReading> { CreateReading(2024, 1, 15, 10, 45) };

        manager.AddRange(readingsQ0);
        manager.AddRange(readingsQ1);
        manager.AddRange(readingsQ2);
        manager.AddRange(readingsQ3);

        // Act & Assert
        var resultQ0 = manager.GetReadings(new DateTime(2024, 1, 15, 10, 0, 0), FilterLevel.Quarter);
        resultQ0.Should().ContainSingle();
        resultQ0[0].PeriodStart.Minute.Should().Be(0);

        var resultQ1 = manager.GetReadings(new DateTime(2024, 1, 15, 10, 15, 0), FilterLevel.Quarter);
        resultQ1.Should().ContainSingle();
        resultQ1[0].PeriodStart.Minute.Should().Be(15);

        var resultQ2 = manager.GetReadings(new DateTime(2024, 1, 15, 10, 30, 0), FilterLevel.Quarter);
        resultQ2.Should().ContainSingle();
        resultQ2[0].PeriodStart.Minute.Should().Be(30);

        var resultQ3 = manager.GetReadings(new DateTime(2024, 1, 15, 10, 45, 0), FilterLevel.Quarter);
        resultQ3.Should().ContainSingle();
        resultQ3[0].PeriodStart.Minute.Should().Be(45);
    }

    [Fact]
    public void GetReadings_WithFilterLevelQuarter_NonExistentQuarter_ShouldReturnHourReadings()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 1, 15, 10, 15)
        };
        manager.AddRange(readings);

        // Act - Quarter 2 (minute 30) doesn't exist
        var result = manager.GetReadings(new DateTime(2024, 1, 15, 10, 30, 0), FilterLevel.Quarter);

        // Assert
        result.Should().HaveCount(2);
        result.All(r => r.PeriodStart.Hour == 10).Should().BeTrue();
    }

    [Fact]
    public void GetReadings_WithFilterLevelQuarter_CalculatesQuarterFromMinutes()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        manager.AddRange(new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 1, 15, 10, 7),   // Should be quarter 0 (0-14 min)
            CreateReading(2024, 1, 15, 10, 14),  // Should be quarter 0 (0-14 min)
            CreateReading(2024, 1, 15, 10, 15),  // Quarter 1
            CreateReading(2024, 1, 15, 10, 29),  // Quarter 1 (15-29 min)
            CreateReading(2024, 1, 15, 10, 30),  // Quarter 2
            CreateReading(2024, 1, 15, 10, 45)   // Quarter 3
        });

        // Act & Assert - Quarter 0 (0-14 minutes)
        var resultQ0 = manager.GetReadings(new DateTime(2024, 1, 15, 10, 5, 0), FilterLevel.Quarter);
        resultQ0.Should().HaveCount(3); // Minutes 0, 7, 14
        resultQ0.All(r => r.PeriodStart.Minute < 15).Should().BeTrue();

        // Quarter 1 (15-29 minutes)
        var resultQ1 = manager.GetReadings(new DateTime(2024, 1, 15, 10, 20, 0), FilterLevel.Quarter);
        resultQ1.Should().HaveCount(2); // Minutes 15, 29
        resultQ1.All(r => r.PeriodStart.Minute >= 15 && r.PeriodStart.Minute < 30).Should().BeTrue();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetReadings_WithEmptyManager_ShouldReturnEmptyList()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);

        // Act
        var result = manager.GetReadings(new DateTime(2024, 1, 1), FilterLevel.Year);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetReadings_WithLeapYearDate_ShouldWork()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 2, 29, 12, 0),
            CreateReading(2024, 2, 29, 12, 15)
        };
        manager.AddRange(readings);

        // Act
        var result = manager.GetReadings(new DateTime(2024, 2, 29), FilterLevel.Day);

        // Assert
        result.Should().HaveCount(2);
        result.All(r => r.PeriodStart.Day == 29 && r.PeriodStart.Month == 2).Should().BeTrue();
    }

    [Fact]
    public void GetReadings_AfterClear_ShouldReturnEmpty()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        manager.AddRange(new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0)
        });

        // Act
        manager.Clear();
        var result = manager.GetReadings(new DateTime(2024, 1, 15), FilterLevel.Day);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void GetReadings_WithLargeDataset_ShouldFilterEfficiently()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var allReadings = new List<MeterDataReading>();

        // Add full year of data
        var startDate = new DateTime(2024, 1, 1, 0, 0, 0);
        var totalReadings = DaysInLeapYear * HoursPerDay * QuartersPerHour;
        for (int i = 0; i < totalReadings; i++)
        {
            allReadings.Add(new MeterDataReading(
                startDate.AddMinutes(i * MinutesPerQuarter),
                PeekUsage, OffPeekUsage, HolidayUsage
            ));
        }
        manager.AddRange(allReadings);

        // Act - Get specific day
        var result = manager.GetReadings(new DateTime(2024, 6, 15), FilterLevel.Day);

        // Assert
        result.Should().HaveCount(ReadingsPerDay);
        result.All(r => r.PeriodStart.Date == new DateTime(2024, 6, 15)).Should().BeTrue();
    }

    #endregion

    #region Multiple Readings Per Quarter

    [Fact]
    public void GetReadings_WithDuplicateTimestamps_ShouldReturnAllInstances()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 1, 15, 10, 0),
            CreateReading(2024, 1, 15, 10, 0)
        };
        manager.AddRange(readings);

        // Act
        var result = manager.GetReadings(new DateTime(2024, 1, 15, 10, 0, 0), FilterLevel.Quarter);

        // Assert
        result.Should().HaveCount(3);
    }

    #endregion

    #region Data Integrity

    [Fact]
    public void GetReadings_ShouldPreserveReadingData()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);
        var reading = new MeterDataReading(new DateTime(2024, 1, 15, 10, 0, 0), 1.5m, 2.5m, 3.5m);
        manager.AddRange(new List<MeterDataReading> { reading });

        // Act
        var result = manager.GetReadings(new DateTime(2024, 1, 15, 10, 0, 0), FilterLevel.Quarter);

        // Assert
        result.Should().ContainSingle();
        result[0].PeekUsage.Should().Be(1.5m);
        result[0].OffPeekUsage.Should().Be(2.5m);
        result[0].HolidayUsage.Should().Be(3.5m);
    }

    [Fact]
    public void GetReadings_ShouldAlwaysReturnNonNull()
    {
        // Arrange
        var manager = new MeterDataManager(new List<MeterDataReading>(), FlatRatePrice, PeekPrice, OffPeekPrice);

        // Act & Assert
        manager.GetReadings(new DateTime(2024, 1, 1), FilterLevel.None).Should().NotBeNull();
        manager.GetReadings(new DateTime(2024, 1, 1), FilterLevel.Year).Should().NotBeNull();
        manager.GetReadings(new DateTime(2024, 1, 1), FilterLevel.Month).Should().NotBeNull();
        manager.GetReadings(new DateTime(2024, 1, 1), FilterLevel.Day).Should().NotBeNull();
        manager.GetReadings(new DateTime(2024, 1, 1), FilterLevel.Hour).Should().NotBeNull();
        manager.GetReadings(new DateTime(2024, 1, 1), FilterLevel.Quarter).Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private static MeterDataReading CreateReading(int year, int month, int day, int hour, int minute)
    {
        var periodStart = new DateTime(year, month, day, hour, minute, 0);
        return new MeterDataReading(periodStart, PeekUsage, OffPeekUsage, HolidayUsage);
    }

    #endregion
}
