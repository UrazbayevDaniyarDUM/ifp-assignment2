using CdrProcessor;
using Xunit;

namespace CdrProcessor.Tests;

public class TariffTests
{
    [Theory]
    [InlineData("KZ", false, 4.0, 60.00)]
    [InlineData("KZ", true, 0.5, 50.00)]
    [InlineData("US", true, 10.0, 1200.00)]
    [InlineData("DE", false, 3.0, 135.00)]
    [InlineData("XX", false, 2.0, 90.00)]
    [InlineData("KZ", true, 1.0, 45.00)]
    public void CalculateCost_ReturnsExpectedTariff(string country, bool roaming, double minutes, double expected)
    {
        var record = new CallRecord("T1", country, minutes, roaming);
        Assert.Equal((decimal)expected, CallPricing.CalculateCost(in record));
    }
}