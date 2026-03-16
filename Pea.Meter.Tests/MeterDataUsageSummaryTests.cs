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
