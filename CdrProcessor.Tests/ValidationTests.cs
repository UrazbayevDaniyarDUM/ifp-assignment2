using CdrProcessor;
using Xunit;

namespace CdrProcessor.Tests;

public class ValidationTests
{
    [Theory]
    [InlineData("", "KZ", 1.0)]
    [InlineData("   ", "KZ", 1.0)]
    [InlineData("R1", "", 1.0)]
    [InlineData("R1", "  ", 1.0)]
    [InlineData("R1", "KZ", -0.1)]
    [InlineData("R1", "KZ", double.NaN)]
    [InlineData("R1", "KZ", double.PositiveInfinity)]
    [InlineData("R1", "KZ", double.NegativeInfinity)]
    [InlineData("R1", "KZ", 10_000.0001)]
    public void Constructor_RejectsInvalidInput(string id, string country, double minutes)
    {
        Assert.ThrowsAny<ArgumentException>(() => new CallRecord(id, country, minutes, false));
    }

    [Fact]
    public void Constructor_RejectsNullStrings()
    {
        Assert.ThrowsAny<ArgumentException>(() => new CallRecord(null!, "KZ", 1.0, false));
        Assert.ThrowsAny<ArgumentException>(() => new CallRecord("R1", null!, 1.0, false));
    }

    [Fact]
    public void CalculateCost_RejectsDefaultRecord()
    {
        var invalid = default(CallRecord);
        Assert.ThrowsAny<ArgumentException>(() => CallPricing.CalculateCost(in invalid));
    }

    [Fact]
    public void ZeroMinuteCalls_AreValid()
    {
        var roamingKz = new CallRecord("Z1", "KZ", 0.0, true);
        var plainKz = new CallRecord("Z2", "KZ", 0.0, false);
        var plainUs = new CallRecord("Z3", "US", 0.0, false);

        Assert.Equal(50.00m, CallPricing.CalculateCost(in roamingKz));
        Assert.Equal(0.00m, CallPricing.CalculateCost(in plainKz));
        Assert.Equal(0.00m, CallPricing.CalculateCost(in plainUs));
    }

    [Fact]
    public void Boundary_FlatFeeVersusPerMinute_AtOneMinute()
    {
        var below = new CallRecord("B1", "KZ", 0.9999999, true);
        var exact = new CallRecord("B2", "KZ", 1.0, true);

        Assert.Equal(50.00m, CallPricing.CalculateCost(in below));
        Assert.Equal(45.00m, CallPricing.CalculateCost(in exact));
    }

    [Fact]
    public void Boundary_RoamingRate_AtTenMinutes()
    {
        var below = new CallRecord("B3", "US", 9.9999999, true);
        var exact = new CallRecord("B4", "US", 10.0, true);

        Assert.Equal(450.00m, CallPricing.CalculateCost(in below));   // 45 * 9.9999999 rounded
        Assert.Equal(1200.00m, CallPricing.CalculateCost(in exact));
    }
}