using FluentAssertions;
using Pea.Infrastructure.Models.MeterData;

namespace Pea.Meter.Tests;

/// <summary>
/// Tests for MeterDataUsageInKwSummary and MeterDataUsageInMoneySummary classes
/// </summary>
public class MeterDataUsageSummaryTests
{
    #region MeterDataUsageInKwSummary Tests

    [Fact]
    public void MeterDataUsageInKwSummary_Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = 100.5m,
            OffPeekUsage = 50.3m
        };

        // Assert
        summary.PeekUsage.Should().Be(100.5m);
        summary.OffPeekUsage.Should().Be(50.3m);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_TotalUsage_ShouldSumPeekAndOffPeek()
    {
        // Arrange
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = 100.5m,
            OffPeekUsage = 50.3m
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(150.8m);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_TotalUsage_WithZeroValues_ShouldReturnZero()
    {
        // Arrange
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = 0m,
            OffPeekUsage = 0m
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(0m);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_TotalUsage_WithNegativeValues_ShouldCalculateCorrectly()
    {
        // Arrange
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = 100m,
            OffPeekUsage = -20m
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(80m);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_TotalUsage_WithLargeValues_ShouldCalculateCorrectly()
    {
        // Arrange
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = 999999.99m,
            OffPeekUsage = 888888.88m
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(1888888.87m);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_TotalUsage_WithDecimalPrecision_ShouldMaintainAccuracy()
    {
        // Arrange
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = 0.001m,
            OffPeekUsage = 0.002m
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(0.003m);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_Holiday_ShouldStoreValue()
    {
        // Arrange & Act
        var summary = new MeterDataUsageInKwSummary
        {
            Holiday = 25.5m
        };

        // Assert
        summary.Holiday.Should().Be(25.5m);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_TotalUsage_ShouldNotIncludeHoliday()
    {
        // Arrange
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = 100m,
            OffPeekUsage = 50m,
            Holiday = 25m
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        // TotalUsage should only be PeekUsage + OffPeekUsage, not including Holiday
        total.Should().Be(150m);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_Reset_ShouldClearAllProperties()
    {
        // Arrange
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = 100m,
            OffPeekUsage = 50m,
            Holiday = 25m
        };

        // Act
        summary.Reset();

        // Assert
        summary.PeekUsage.Should().Be(0m);
        summary.OffPeekUsage.Should().Be(0m);
        summary.Holiday.Should().Be(0m);
    }

    #endregion

    #region MeterDataUsageInMoneySummary Tests

    [Fact]
    public void MeterDataUsageInMoneySummary_Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var summary = new MeterDataUsageInMoneySummary
        {
            PeekUsage = 200.75m,
            OffPeekUsage = 100.25m
        };

        // Assert
        summary.PeekUsage.Should().Be(200.75m);
        summary.OffPeekUsage.Should().Be(100.25m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_TotalUsage_ShouldSumPeekAndOffPeek()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary
        {
            PeekUsage = 200.75m,
            OffPeekUsage = 100.25m
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(301.00m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_TotalUsage_WithZeroValues_ShouldReturnZero()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary
        {
            PeekUsage = 0m,
            OffPeekUsage = 0m
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(0m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_TotalUsage_WithCurrencyPrecision_ShouldCalculateCorrectly()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary
        {
            PeekUsage = 123.456m,
            OffPeekUsage = 234.567m
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(358.023m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_TotalUsage_WithLargeMonetaryValues_ShouldCalculateCorrectly()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary
        {
            PeekUsage = 10000.50m,
            OffPeekUsage = 5000.25m
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(15000.75m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_PeekTouUsagePriceSummary_ShouldStoreValue()
    {
        // Arrange & Act
        var summary = new MeterDataUsageInMoneySummary
        {
            PeekTouUsagePriceSummary = 500.50m
        };

        // Assert
        summary.PeekTouUsagePriceSummary.Should().Be(500.50m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_OffPeekTouUsagePriceSummary_ShouldStoreValue()
    {
        // Arrange & Act
        var summary = new MeterDataUsageInMoneySummary
        {
            OffPeekTouUsagePriceSummary = 300.25m
        };

        // Assert
        summary.OffPeekTouUsagePriceSummary.Should().Be(300.25m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_FlatRateUsagePriceSummary_ShouldStoreValue()
    {
        // Arrange & Act
        var summary = new MeterDataUsageInMoneySummary
        {
            FlatRateUsagePriceSummary = 450.75m
        };

        // Assert
        summary.FlatRateUsagePriceSummary.Should().Be(450.75m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_TotalTouUsagePriceSummary_ShouldSumPeekAndOffPeekTou()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary
        {
            PeekTouUsagePriceSummary = 500.50m,
            OffPeekTouUsagePriceSummary = 300.25m,
            FlatRateUsagePriceSummary = 450.75m
        };

        // Act
        var total = summary.TotalTouUsagePriceSummary;

        // Assert
        total.Should().Be(800.75m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_TotalTouUsagePriceSummary_WithZeroValues_ShouldReturnZero()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary
        {
            PeekTouUsagePriceSummary = 0m,
            OffPeekTouUsagePriceSummary = 0m
        };

        // Act
        var total = summary.TotalTouUsagePriceSummary;

        // Assert
        total.Should().Be(0m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_Reset_ShouldClearAllProperties()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary
        {
            PeekUsage = 100m,
            OffPeekUsage = 50m,
            PeekTouUsagePriceSummary = 500m,
            OffPeekTouUsagePriceSummary = 300m,
            FlatRateUsagePriceSummary = 450m
        };

        // Act
        summary.Reset();

        // Assert
        summary.PeekUsage.Should().Be(0m);
        summary.OffPeekUsage.Should().Be(0m);
        summary.PeekTouUsagePriceSummary.Should().Be(0m);
        summary.OffPeekTouUsagePriceSummary.Should().Be(0m);
        summary.FlatRateUsagePriceSummary.Should().Be(0m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_Calculate_ShouldCalculatePricesCorrectly()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary();
        var readings = new List<MeterDataReading>
        {
            new MeterDataReading(DateTime.Now, 10m, 5m, 2m), // Peek: 10, OffPeek: 5, Holiday: 2
            new MeterDataReading(DateTime.Now, 20m, 10m, 3m) // Peek: 20, OffPeek: 10, Holiday: 3
        };
        const decimal flatRatePrice = 1.5m;
        const decimal peekPrice = 2.0m;
        const decimal offPeekPrice = 1.0m;

        // Act
        summary.Calculate(readings, flatRatePrice, peekPrice, offPeekPrice);

        // Assert
        // PeekUsage: (10 + 20) * 2.0 = 60
        summary.PeekTouUsagePriceSummary.Should().Be(60m);
        // OffPeekUsage: (5 + 10) * 1.0 = 15
        summary.OffPeekTouUsagePriceSummary.Should().Be(15m);
        // HolidayUsage: (2 + 3) * 1.5 = 7.5
        summary.FlatRateUsagePriceSummary.Should().Be(7.5m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_Calculate_WithEmptyList_ShouldNotChange()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary
        {
            PeekTouUsagePriceSummary = 100m,
            OffPeekTouUsagePriceSummary = 50m,
            FlatRateUsagePriceSummary = 25m
        };
        var emptyReadings = new List<MeterDataReading>();

        // Act
        summary.Calculate(emptyReadings, 1.5m, 2.0m, 1.0m);

        // Assert
        summary.PeekTouUsagePriceSummary.Should().Be(100m);
        summary.OffPeekTouUsagePriceSummary.Should().Be(50m);
        summary.FlatRateUsagePriceSummary.Should().Be(25m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_Calculate_CalledMultipleTimes_ShouldAccumulate()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary();
        var readings1 = new List<MeterDataReading>
        {
            new MeterDataReading(DateTime.Now, 10m, 5m, 2m)
        };
        var readings2 = new List<MeterDataReading>
        {
            new MeterDataReading(DateTime.Now, 20m, 10m, 3m)
        };
        const decimal flatRatePrice = 1.5m;
        const decimal peekPrice = 2.0m;
        const decimal offPeekPrice = 1.0m;

        // Act
        summary.Calculate(readings1, flatRatePrice, peekPrice, offPeekPrice);
        summary.Calculate(readings2, flatRatePrice, peekPrice, offPeekPrice);

        // Assert
        // First: 10 * 2.0 = 20, Second: 20 * 2.0 = 40, Total: 60
        summary.PeekTouUsagePriceSummary.Should().Be(60m);
        // First: 5 * 1.0 = 5, Second: 10 * 1.0 = 10, Total: 15
        summary.OffPeekTouUsagePriceSummary.Should().Be(15m);
        // First: 2 * 1.5 = 3, Second: 3 * 1.5 = 4.5, Total: 7.5
        summary.FlatRateUsagePriceSummary.Should().Be(7.5m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_Calculate_WithZeroReadings_ShouldResultInZero()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary();
        var readings = new List<MeterDataReading>
        {
            new MeterDataReading(DateTime.Now, 0m, 0m, 0m)
        };

        // Act
        summary.Calculate(readings, 1.5m, 2.0m, 1.0m);

        // Assert
        summary.PeekTouUsagePriceSummary.Should().Be(0m);
        summary.OffPeekTouUsagePriceSummary.Should().Be(0m);
        summary.FlatRateUsagePriceSummary.Should().Be(0m);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_Calculate_WithDecimalPrices_ShouldCalculateAccurately()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary();
        var readings = new List<MeterDataReading>
        {
            new MeterDataReading(DateTime.Now, 10.5m, 5.3m, 2.7m)
        };
        const decimal flatRatePrice = 1.25m;
        const decimal peekPrice = 2.35m;
        const decimal offPeekPrice = 1.15m;

        // Act
        summary.Calculate(readings, flatRatePrice, peekPrice, offPeekPrice);

        // Assert
        // PeekUsage: 10.5 * 2.35 = 24.675
        summary.PeekTouUsagePriceSummary.Should().Be(24.675m);
        // OffPeekUsage: 5.3 * 1.15 = 6.095
        summary.OffPeekTouUsagePriceSummary.Should().Be(6.095m);
        // HolidayUsage: 2.7 * 1.25 = 3.375
        summary.FlatRateUsagePriceSummary.Should().Be(3.375m);
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public void MeterDataUsageInKwSummary_And_MeterDataUsageInMoneySummary_ShouldHaveSameStructure()
    {
        // Arrange
        var kwSummary = new MeterDataUsageInKwSummary { PeekUsage = 100m, OffPeekUsage = 50m };
        var moneySummary = new MeterDataUsageInMoneySummary { PeekUsage = 100m, OffPeekUsage = 50m };

        // Act & Assert
        kwSummary.TotalUsage.Should().Be(moneySummary.TotalUsage);
        kwSummary.PeekUsage.Should().Be(moneySummary.PeekUsage);
        kwSummary.OffPeekUsage.Should().Be(moneySummary.OffPeekUsage);
    }

    #endregion
}
