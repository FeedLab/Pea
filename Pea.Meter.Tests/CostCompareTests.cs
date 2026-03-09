using FluentAssertions;
using Pea.Infrastructure.Models;
using Pea.Meter.Models;

namespace Pea.Meter.Tests;

public class CostCompareTests
{
    // Rate constants
    private const decimal FlatRatePrice = 1.5m;
    private const decimal PeekRatePrice = 2.0m;
    private const decimal OffPeekRatePrice = 1.0m;

    // Meter reading constants
    private const decimal RatePeek = 10m;
    private const decimal RateOffPeek = 5m;
    private const decimal RateHoliday = 5m;
    private const decimal RateTotal = RatePeek + RateOffPeek + RateHoliday; // 20kW

    // High usage scenario
    private const decimal HighUsageRateA = 100m;
    private const decimal HighUsageRateB = 200m;
    private const decimal HighUsageRateC = 300m;
    private const decimal HighUsageTotal = HighUsageRateA + HighUsageRateB + HighUsageRateC; // 600kW

    // Time boundary constants
    private const int PeekStartHour = 9;
    private const int PeekEndHour = 22;
    private const int MidnightHour = 0;
    private const int NoonHour = 12;
    private const int BoundaryMinute = 59;

    // Date constants for test setup
    private const int TestYear = 2024;
    private const int TestMonth = 1;
    private const int TestDay = 15; // Monday

    [Theory]
    [InlineData(9, DayOfWeek.Monday)]
    [InlineData(12, DayOfWeek.Tuesday)]
    [InlineData(15, DayOfWeek.Wednesday)]
    [InlineData(18, DayOfWeek.Thursday)]
    [InlineData(21, DayOfWeek.Friday)]
    public void Constructor_WeekdayBetween9And22_ShouldCalculatePeekCost(int hour, DayOfWeek dayOfWeek)
    {
        // Arrange
        var periodStart = GetDateTimeForDayAndHour(dayOfWeek, hour);
        var meterReading = new PeaMeterReading(periodStart, RatePeek, RateOffPeek, RateHoliday);

        // Act
        var costCompare = new CostCompare(meterReading, FlatRatePrice, PeekRatePrice, OffPeekRatePrice);

        // Assert
        var expectedTouCost = RateTotal * PeekRatePrice;
        var expectedFlatRateCost = RateTotal * FlatRatePrice;

        costCompare.TouCost.Should().Be(expectedTouCost);
        costCompare.KwCostAtPeek.Should().Be(RateTotal);
        costCompare.KwCostAtOffPeek.Should().Be(0m);
        costCompare.IsPeekPeriod.Should().BeTrue();
        costCompare.FlatRateCost.Should().Be(expectedFlatRateCost);
        costCompare.KwUsed.Should().Be(RateTotal);
        costCompare.MeterReading.Should().Be(meterReading);
    }

    [Theory]
    [InlineData(0, DayOfWeek.Monday)]
    [InlineData(3, DayOfWeek.Tuesday)]
    [InlineData(6, DayOfWeek.Wednesday)]
    [InlineData(8, DayOfWeek.Thursday)]
    public void Constructor_WeekdayBefore9_ShouldCalculateOffPeekCost(int hour, DayOfWeek dayOfWeek)
    {
        // Arrange
        var periodStart = GetDateTimeForDayAndHour(dayOfWeek, hour);
        var meterReading = new PeaMeterReading(periodStart, RatePeek, RateOffPeek, RateHoliday);

        // Act
        var costCompare = new CostCompare(meterReading, FlatRatePrice, PeekRatePrice, OffPeekRatePrice);

        // Assert
        var expectedTouCost = RateTotal * OffPeekRatePrice;
        var expectedFlatRateCost = RateTotal * FlatRatePrice;

        costCompare.TouCost.Should().Be(expectedTouCost);
        costCompare.KwCostAtPeek.Should().Be(0m);
        costCompare.KwCostAtOffPeek.Should().Be(RateTotal);
        costCompare.IsPeekPeriod.Should().BeFalse();
        costCompare.FlatRateCost.Should().Be(expectedFlatRateCost);
        costCompare.KwUsed.Should().Be(RateTotal);
        costCompare.MeterReading.Should().Be(meterReading);
    }

    [Theory]
    [InlineData(22, DayOfWeek.Monday)]
    [InlineData(23, DayOfWeek.Tuesday)]
    public void Constructor_WeekdayAfter22_ShouldCalculateOffPeekCost(int hour, DayOfWeek dayOfWeek)
    {
        // Arrange
        var periodStart = GetDateTimeForDayAndHour(dayOfWeek, hour);
        var meterReading = new PeaMeterReading(periodStart, RatePeek, RateOffPeek, RateHoliday);

        // Act
        var costCompare = new CostCompare(meterReading, FlatRatePrice, PeekRatePrice, OffPeekRatePrice);

        // Assert
        var expectedTouCost = RateTotal * OffPeekRatePrice;
        var expectedFlatRateCost = RateTotal * FlatRatePrice;

        costCompare.TouCost.Should().Be(expectedTouCost);
        costCompare.KwCostAtPeek.Should().Be(0m);
        costCompare.KwCostAtOffPeek.Should().Be(RateTotal);
        costCompare.IsPeekPeriod.Should().BeFalse();
        costCompare.FlatRateCost.Should().Be(expectedFlatRateCost);
        costCompare.KwUsed.Should().Be(RateTotal);
        costCompare.MeterReading.Should().Be(meterReading);
    }

    [Theory]
    [InlineData(0, DayOfWeek.Saturday)]
    [InlineData(9, DayOfWeek.Saturday)]
    [InlineData(12, DayOfWeek.Saturday)]
    [InlineData(15, DayOfWeek.Saturday)]
    [InlineData(22, DayOfWeek.Saturday)]
    [InlineData(0, DayOfWeek.Sunday)]
    [InlineData(9, DayOfWeek.Sunday)]
    [InlineData(12, DayOfWeek.Sunday)]
    [InlineData(15, DayOfWeek.Sunday)]
    [InlineData(22, DayOfWeek.Sunday)]
    public void Constructor_Weekend_ShouldCalculateOffPeekCost(int hour, DayOfWeek dayOfWeek)
    {
        // Arrange
        var periodStart = GetDateTimeForDayAndHour(dayOfWeek, hour);
        var meterReading = new PeaMeterReading(periodStart, RatePeek, RateOffPeek, RateHoliday);

        // Act
        var costCompare = new CostCompare(meterReading, FlatRatePrice, PeekRatePrice, OffPeekRatePrice);

        // Assert
        var expectedTouCost = RateTotal * OffPeekRatePrice;
        var expectedFlatRateCost = RateTotal * FlatRatePrice;

        costCompare.TouCost.Should().Be(expectedTouCost);
        costCompare.KwCostAtPeek.Should().Be(0m);
        costCompare.KwCostAtOffPeek.Should().Be(RateTotal);
        costCompare.IsPeekPeriod.Should().BeFalse();
        costCompare.FlatRateCost.Should().Be(expectedFlatRateCost);
        costCompare.KwUsed.Should().Be(RateTotal);
        costCompare.MeterReading.Should().Be(meterReading);
    }

    [Fact]
    public void Constructor_WithZeroUsage_ShouldCalculateZeroCosts()
    {
        // Arrange
        const decimal zeroUsage = 0m;
        var periodStart = GetDateTimeForDayAndHour(DayOfWeek.Monday, NoonHour);
        var meterReading = new PeaMeterReading(periodStart, zeroUsage, zeroUsage, zeroUsage);

        // Act
        var costCompare = new CostCompare(meterReading, FlatRatePrice, PeekRatePrice, OffPeekRatePrice);

        // Assert
        costCompare.TouCost.Should().Be(zeroUsage);
        costCompare.FlatRateCost.Should().Be(zeroUsage);
        costCompare.KwUsed.Should().Be(zeroUsage);
    }

    [Fact]
    public void Constructor_WithHighUsage_ShouldCalculateCorrectCosts()
    {
        // Arrange
        var periodStart = GetDateTimeForDayAndHour(DayOfWeek.Monday, NoonHour);
        var meterReading = new PeaMeterReading(periodStart, HighUsageRateA, HighUsageRateB, HighUsageRateC);

        // Act
        var costCompare = new CostCompare(meterReading, FlatRatePrice, PeekRatePrice, OffPeekRatePrice);

        // Assert
        var expectedTouCost = HighUsageTotal * PeekRatePrice;
        var expectedFlatRateCost = HighUsageTotal * FlatRatePrice;

        costCompare.KwUsed.Should().Be(HighUsageTotal);
        costCompare.TouCost.Should().Be(expectedTouCost);
        costCompare.FlatRateCost.Should().Be(expectedFlatRateCost);
        costCompare.KwCostAtPeek.Should().Be(HighUsageTotal);
    }

    [Theory]
    [InlineData(0.5, 1.0, 0.5)]
    [InlineData(2.5, 3.0, 1.5)]
    [InlineData(0.1, 0.2, 0.05)]
    public void Constructor_WithDifferentRates_ShouldCalculateCorrectly(decimal flatRate, decimal peek, decimal offPeek)
    {
        // Arrange
        var periodStart = GetDateTimeForDayAndHour(DayOfWeek.Monday, NoonHour);
        var meterReading = new PeaMeterReading(periodStart, RatePeek, RateOffPeek, RateHoliday);

        // Act
        var costCompare = new CostCompare(meterReading, flatRate, peek, offPeek);

        // Assert
        var expectedTouCost = RateTotal * peek;
        var expectedFlatRateCost = RateTotal * flatRate;

        costCompare.TouCost.Should().Be(expectedTouCost);
        costCompare.FlatRateCost.Should().Be(expectedFlatRateCost);
    }

    [Fact]
    public void Constructor_PeekPeriod_ShouldHaveCorrectPropertyValues()
    {
        // Arrange
        const int peekHour = 10;
        var periodStart = new DateTime(TestYear, TestMonth, TestDay, peekHour, MidnightHour, MidnightHour);
        var meterReading = new PeaMeterReading(periodStart, RatePeek, RateOffPeek, RateHoliday);

        // Act
        var costCompare = new CostCompare(meterReading, FlatRatePrice, PeekRatePrice, OffPeekRatePrice);

        // Assert
        costCompare.IsPeekPeriod.Should().BeTrue();
        costCompare.KwCostAtPeek.Should().Be(RateTotal);
        costCompare.KwCostAtOffPeek.Should().Be(0m);
    }

    [Fact]
    public void Constructor_OffPeekPeriod_ShouldHaveCorrectPropertyValues()
    {
        // Arrange
        const int offPeekHour = 5;
        var periodStart = new DateTime(TestYear, TestMonth, TestDay, offPeekHour, MidnightHour, MidnightHour);
        var meterReading = new PeaMeterReading(periodStart, RatePeek, RateOffPeek, RateHoliday);

        // Act
        var costCompare = new CostCompare(meterReading, FlatRatePrice, PeekRatePrice, OffPeekRatePrice);

        // Assert
        costCompare.IsPeekPeriod.Should().BeFalse();
        costCompare.KwCostAtPeek.Should().Be(0m);
        costCompare.KwCostAtOffPeek.Should().Be(RateTotal);
    }

    [Theory]
    [InlineData(8, 59, DayOfWeek.Monday)]  // 8:59 AM - just before peek
    [InlineData(21, 59, DayOfWeek.Monday)] // 9:59 PM - just before off-peek
    public void Constructor_BoundaryTimes_ShouldCalculateCorrectly(int hour, int minute, DayOfWeek dayOfWeek)
    {
        // Arrange
        const int hourBeforePeek = 8;
        var periodStart = GetDateTimeForDayAndHour(dayOfWeek, hour, minute);
        var meterReading = new PeaMeterReading(periodStart, RatePeek, RateOffPeek, RateHoliday);

        // Act
        var costCompare = new CostCompare(meterReading, FlatRatePrice, PeekRatePrice, OffPeekRatePrice);

        // Assert - 8:59 is off-peek, 21:59 is peek
        if (hour == hourBeforePeek)
        {
            var expectedTouCost = RateTotal * OffPeekRatePrice;
            costCompare.IsPeekPeriod.Should().BeFalse();
            costCompare.TouCost.Should().Be(expectedTouCost);
        }
        else
        {
            var expectedTouCost = RateTotal * PeekRatePrice;
            costCompare.IsPeekPeriod.Should().BeTrue();
            costCompare.TouCost.Should().Be(expectedTouCost);
        }
    }

    [Fact]
    public void Constructor_MeterReadingProperty_ShouldRetainOriginalReference()
    {
        // Arrange
        var periodStart = GetDateTimeForDayAndHour(DayOfWeek.Monday, NoonHour);
        var meterReading = new PeaMeterReading(periodStart, RatePeek, RateOffPeek, RateHoliday);

        // Act
        var costCompare = new CostCompare(meterReading, FlatRatePrice, PeekRatePrice, OffPeekRatePrice);

        // Assert
        costCompare.MeterReading.Should().BeSameAs(meterReading);
        costCompare.MeterReading.PeriodStart.Should().Be(periodStart);
    }

    private static DateTime GetDateTimeForDayAndHour(DayOfWeek dayOfWeek, int hour, int minute = MidnightHour)
    {
        // Start from a Monday (2024-01-15)
        const int daysInWeek = 7;
        var baseDate = new DateTime(TestYear, TestMonth, TestDay, MidnightHour, MidnightHour, MidnightHour);
        var daysToAdd = (int)dayOfWeek - (int)baseDate.DayOfWeek;
        if (daysToAdd < 0) daysToAdd += daysInWeek;

        return baseDate.AddDays(daysToAdd).AddHours(hour).AddMinutes(minute);
    }
}
