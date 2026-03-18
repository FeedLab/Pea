using FluentAssertions;
using Pea.Infrastructure.Models.MeterData;

namespace Pea.Meter.Tests;

/// <summary>
/// Tests for MeterDataUsageInKwSummary and MeterDataUsageInMoneySummary classes
/// </summary>
public class MeterDataUsageSummaryTests
{
    // Test data constants for kW usage values
    private const decimal SmallPeekUsage = 100.5m;
    private const decimal SmallOffPeekUsage = 50.3m;
    private const decimal SmallTotalUsage = 150.8m;

    private const decimal MediumPeekUsage = 100m;
    private const decimal MediumOffPeekUsage = 50m;
    private const decimal NegativeOffPeekUsage = -20m;
    private const decimal NetUsage = 80m;

    private const decimal LargePeekUsage = 999999.99m;
    private const decimal LargeOffPeekUsage = 888888.88m;
    private const decimal LargeTotalUsage = 1888888.87m;

    private const decimal TinyPeekUsage = 0.001m;
    private const decimal TinyOffPeekUsage = 0.002m;
    private const decimal TinyTotalUsage = 0.003m;

    private const decimal HolidayUsageValue = 25.5m;
    private const decimal HolidayUsageValueRound = 25m;
    private const decimal MediumTotalUsageWithHoliday = 150m;

    // Test data constants for money values
    private const decimal FlatRatePrice = 1.5m;
    private const decimal PeekPrice = 2.0m;
    private const decimal OffPeekPrice = 1.0m;

    private const decimal MediumPeekMoney = 200.75m;
    private const decimal MediumOffPeekMoney = 100.25m;
    private const decimal MediumTotalMoney = 301.00m;

    private const decimal PrecisionPeekMoney = 123.456m;
    private const decimal PrecisionOffPeekMoney = 234.567m;
    private const decimal PrecisionTotalMoney = 358.023m;

    private const decimal LargePeekMoney = 10000.50m;
    private const decimal LargeOffPeekMoney = 5000.25m;
    private const decimal LargeTotalMoney = 15000.75m;

    private const decimal PeekTouPrice = 500.50m;
    private const decimal OffPeekTouPrice = 300.25m;
    private const decimal FlatRatePrice450 = 450.75m;
    private const decimal TotalTouPrice = 800.75m;

    private const decimal PeekTouPriceRound = 500m;
    private const decimal OffPeekTouPriceRound = 300m;
    private const decimal FlatRatePriceRound = 450m;

    // Test data constants for reading values
    private const decimal ReadingPeek1 = 10m;
    private const decimal ReadingOffPeek1 = 5m;
    private const decimal ReadingHoliday1 = 2m;

    private const decimal ReadingPeek2 = 20m;
    private const decimal ReadingOffPeek2 = 10m;
    private const decimal ReadingHoliday2 = 3m;

    private const decimal ExpectedPeekTouPrice60 = 60m;
    private const decimal ExpectedOffPeekTouPrice15 = 15m;
    private const decimal ExpectedFlatRatePrice7_5 = 7.5m;

    private const decimal PresetPeekTouPrice100 = 100m;
    private const decimal PresetOffPeekTouPrice50 = 50m;
    private const decimal PresetFlatRatePrice25 = 25m;

    // Test data constants for decimal precision calculation
    private const decimal DecimalFlatRatePrice = 1.25m;
    private const decimal DecimalPeekPrice = 2.35m;
    private const decimal DecimalOffPeekPrice = 1.15m;

    private const decimal DecimalPeekUsage = 10.5m;
    private const decimal DecimalOffPeekUsage = 5.3m;
    private const decimal DecimalHolidayUsage = 2.7m;

    private const decimal ExpectedDecimalPeekPrice = 24.675m;
    private const decimal ExpectedDecimalOffPeekPrice = 6.095m;
    private const decimal ExpectedDecimalFlatRatePrice = 3.375m;

    private const decimal Zero = 0m;

    #region MeterDataUsageInKwSummary Tests

    [Fact]
    public void MeterDataUsageInKwSummary_Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = SmallPeekUsage,
            OffPeekUsage = SmallOffPeekUsage
        };

        // Assert
        summary.PeekUsage.Should().Be(SmallPeekUsage);
        summary.OffPeekUsage.Should().Be(SmallOffPeekUsage);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_TotalUsage_ShouldSumPeekAndOffPeek()
    {
        // Arrange
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = SmallPeekUsage,
            OffPeekUsage = SmallOffPeekUsage
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(SmallTotalUsage);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_TotalUsage_WithZeroValues_ShouldReturnZero()
    {
        // Arrange
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = Zero,
            OffPeekUsage = Zero
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(Zero);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_TotalUsage_WithNegativeValues_ShouldCalculateCorrectly()
    {
        // Arrange
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = MediumPeekUsage,
            OffPeekUsage = NegativeOffPeekUsage
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(NetUsage);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_TotalUsage_WithLargeValues_ShouldCalculateCorrectly()
    {
        // Arrange
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = LargePeekUsage,
            OffPeekUsage = LargeOffPeekUsage
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(LargeTotalUsage);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_TotalUsage_WithDecimalPrecision_ShouldMaintainAccuracy()
    {
        // Arrange
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = TinyPeekUsage,
            OffPeekUsage = TinyOffPeekUsage
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(TinyTotalUsage);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_Holiday_ShouldStoreValue()
    {
        // Arrange & Act
        var summary = new MeterDataUsageInKwSummary
        {
            Holiday = HolidayUsageValue
        };

        // Assert
        summary.Holiday.Should().Be(HolidayUsageValue);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_TotalUsage_ShouldNotIncludeHoliday()
    {
        // Arrange
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = MediumPeekUsage,
            OffPeekUsage = MediumOffPeekUsage,
            Holiday = HolidayUsageValueRound
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        // TotalUsage should only be PeekUsage + OffPeekUsage, not including Holiday
        total.Should().Be(MediumTotalUsageWithHoliday);
    }

    [Fact]
    public void MeterDataUsageInKwSummary_Reset_ShouldClearAllProperties()
    {
        // Arrange
        var summary = new MeterDataUsageInKwSummary
        {
            PeekUsage = MediumPeekUsage,
            OffPeekUsage = MediumOffPeekUsage,
            Holiday = HolidayUsageValueRound
        };

        // Act
        summary.Reset();

        // Assert
        summary.PeekUsage.Should().Be(Zero);
        summary.OffPeekUsage.Should().Be(Zero);
        summary.Holiday.Should().Be(Zero);
    }

    #endregion

    #region MeterDataUsageInMoneySummary Tests

    [Fact]
    public void MeterDataUsageInMoneySummary_Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice)
        {
            PeekUsage = MediumPeekMoney,
            OffPeekUsage = MediumOffPeekMoney
        };

        // Assert
        summary.PeekUsage.Should().Be(MediumPeekMoney);
        summary.OffPeekUsage.Should().Be(MediumOffPeekMoney);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_TotalUsage_ShouldSumPeekAndOffPeek()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice)
        {
            PeekUsage = MediumPeekMoney,
            OffPeekUsage = MediumOffPeekMoney
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(MediumTotalMoney);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_TotalUsage_WithZeroValues_ShouldReturnZero()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice)
        {
            PeekUsage = Zero,
            OffPeekUsage = Zero
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(Zero);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_TotalUsage_WithCurrencyPrecision_ShouldCalculateCorrectly()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice)
        {
            PeekUsage = PrecisionPeekMoney,
            OffPeekUsage = PrecisionOffPeekMoney
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(PrecisionTotalMoney);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_TotalUsage_WithLargeMonetaryValues_ShouldCalculateCorrectly()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice)
        {
            PeekUsage = LargePeekMoney,
            OffPeekUsage = LargeOffPeekMoney
        };

        // Act
        var total = summary.TotalUsage;

        // Assert
        total.Should().Be(LargeTotalMoney);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_PeekTouUsagePriceSummary_ShouldStoreValue()
    {
        // Arrange & Act
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice)
        {
            PeekTouUsagePriceSummary = PeekTouPrice
        };

        // Assert
        summary.PeekTouUsagePriceSummary.Should().Be(PeekTouPrice);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_OffPeekTouUsagePriceSummary_ShouldStoreValue()
    {
        // Arrange & Act
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice)
        {
            OffPeekTouUsagePriceSummary = OffPeekTouPrice
        };

        // Assert
        summary.OffPeekTouUsagePriceSummary.Should().Be(OffPeekTouPrice);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_FlatRateUsagePriceSummary_ShouldStoreValue()
    {
        // Arrange & Act
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice)
        {
            FlatRateUsagePriceSummary = FlatRatePrice450
        };

        // Assert
        summary.FlatRateUsagePriceSummary.Should().Be(FlatRatePrice450);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_TotalTouUsagePriceSummary_ShouldSumPeekAndOffPeekTou()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice)
        {
            PeekTouUsagePriceSummary = PeekTouPrice,
            OffPeekTouUsagePriceSummary = OffPeekTouPrice,
            FlatRateUsagePriceSummary = FlatRatePrice450
        };

        // Act
        var total = summary.TotalTouUsagePriceSummary;

        // Assert
        total.Should().Be(TotalTouPrice);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_TotalTouUsagePriceSummary_WithZeroValues_ShouldReturnZero()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice)
        {
            PeekTouUsagePriceSummary = Zero,
            OffPeekTouUsagePriceSummary = Zero
        };

        // Act
        var total = summary.TotalTouUsagePriceSummary;

        // Assert
        total.Should().Be(Zero);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_Reset_ShouldClearAllProperties()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice)
        {
            PeekUsage = MediumPeekUsage,
            OffPeekUsage = MediumOffPeekUsage,
            PeekTouUsagePriceSummary = PeekTouPriceRound,
            OffPeekTouUsagePriceSummary = OffPeekTouPriceRound,
            FlatRateUsagePriceSummary = FlatRatePriceRound
        };

        // Act
        summary.Reset();

        // Assert
        summary.PeekUsage.Should().Be(Zero);
        summary.OffPeekUsage.Should().Be(Zero);
        summary.PeekTouUsagePriceSummary.Should().Be(Zero);
        summary.OffPeekTouUsagePriceSummary.Should().Be(Zero);
        summary.FlatRateUsagePriceSummary.Should().Be(Zero);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_Calculate_ShouldCalculatePricesCorrectly()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            new MeterDataReading(DateTime.Now, ReadingPeek1, ReadingOffPeek1, ReadingHoliday1), // Peek: 10, OffPeek: 5, Holiday: 2
            new MeterDataReading(DateTime.Now, ReadingPeek2, ReadingOffPeek2, ReadingHoliday2) // Peek: 20, OffPeek: 10, Holiday: 3
        };

        // Act
        summary.Calculate(readings);

        // Assert
        // PeekUsage: (10 + 20) * 2.0 = 60
        summary.PeekTouUsagePriceSummary.Should().Be(ExpectedPeekTouPrice60);
        // OffPeekUsage: (5 + 10) * 1.0 = 15
        summary.OffPeekTouUsagePriceSummary.Should().Be(ExpectedOffPeekTouPrice15);
        // HolidayUsage: (2 + 3) * 1.5 = 7.5
        summary.FlatRateUsagePriceSummary.Should().Be(ExpectedFlatRatePrice7_5);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_Calculate_WithEmptyList_ShouldNotChange()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice)
        {
            PeekTouUsagePriceSummary = PresetPeekTouPrice100,
            OffPeekTouUsagePriceSummary = PresetOffPeekTouPrice50,
            FlatRateUsagePriceSummary = PresetFlatRatePrice25
        };
        var emptyReadings = new List<MeterDataReading>();

        // Act
        summary.Calculate(emptyReadings);

        // Assert
        summary.PeekTouUsagePriceSummary.Should().Be(PresetPeekTouPrice100);
        summary.OffPeekTouUsagePriceSummary.Should().Be(PresetOffPeekTouPrice50);
        summary.FlatRateUsagePriceSummary.Should().Be(PresetFlatRatePrice25);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_Calculate_CalledMultipleTimes_ShouldAccumulate()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings1 = new List<MeterDataReading>
        {
            new MeterDataReading(DateTime.Now, ReadingPeek1, ReadingOffPeek1, ReadingHoliday1)
        };
        var readings2 = new List<MeterDataReading>
        {
            new MeterDataReading(DateTime.Now, ReadingPeek2, ReadingOffPeek2, ReadingHoliday2)
        };

        // Act
        summary.Calculate(readings1);
        summary.Calculate(readings2);

        // Assert
        // First: 10 * 2.0 = 20, Second: 20 * 2.0 = 40, Total: 60
        summary.PeekTouUsagePriceSummary.Should().Be(ExpectedPeekTouPrice60);
        // First: 5 * 1.0 = 5, Second: 10 * 1.0 = 10, Total: 15
        summary.OffPeekTouUsagePriceSummary.Should().Be(ExpectedOffPeekTouPrice15);
        // First: 2 * 1.5 = 3, Second: 3 * 1.5 = 4.5, Total: 7.5
        summary.FlatRateUsagePriceSummary.Should().Be(ExpectedFlatRatePrice7_5);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_Calculate_WithZeroReadings_ShouldResultInZero()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            new MeterDataReading(DateTime.Now, Zero, Zero, Zero)
        };

        // Act
        summary.Calculate(readings);

        // Assert
        summary.PeekTouUsagePriceSummary.Should().Be(Zero);
        summary.OffPeekTouUsagePriceSummary.Should().Be(Zero);
        summary.FlatRateUsagePriceSummary.Should().Be(Zero);
    }

    [Fact]
    public void MeterDataUsageInMoneySummary_Calculate_WithDecimalPrices_ShouldCalculateAccurately()
    {
        // Arrange
        var summary = new MeterDataUsageInMoneySummary(DecimalFlatRatePrice, DecimalPeekPrice, DecimalOffPeekPrice);
        var readings = new List<MeterDataReading>
        {
            new MeterDataReading(DateTime.Now, DecimalPeekUsage, DecimalOffPeekUsage, DecimalHolidayUsage)
        };

        // Act
        summary.Calculate(readings);

        // Assert
        // PeekUsage: 10.5 * 2.35 = 24.675
        summary.PeekTouUsagePriceSummary.Should().Be(ExpectedDecimalPeekPrice);
        // OffPeekUsage: 5.3 * 1.15 = 6.095
        summary.OffPeekTouUsagePriceSummary.Should().Be(ExpectedDecimalOffPeekPrice);
        // HolidayUsage: 2.7 * 1.25 = 3.375
        summary.FlatRateUsagePriceSummary.Should().Be(ExpectedDecimalFlatRatePrice);
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public void MeterDataUsageInKwSummary_And_MeterDataUsageInMoneySummary_ShouldHaveSameStructure()
    {
        // Arrange
        var kwSummary = new MeterDataUsageInKwSummary { PeekUsage = MediumPeekUsage, OffPeekUsage = MediumOffPeekUsage };
        var moneySummary = new MeterDataUsageInMoneySummary(FlatRatePrice, PeekPrice, OffPeekPrice) { PeekUsage = MediumPeekUsage, OffPeekUsage = MediumOffPeekUsage };

        // Act & Assert
        kwSummary.TotalUsage.Should().Be(moneySummary.TotalUsage);
        kwSummary.PeekUsage.Should().Be(moneySummary.PeekUsage);
        kwSummary.OffPeekUsage.Should().Be(moneySummary.OffPeekUsage);
    }

    #endregion
}
