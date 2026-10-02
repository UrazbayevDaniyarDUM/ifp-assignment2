namespace CdrProcessor;

public static class CallPricing
{
    public static decimal CalculateCost(in CallRecord record)
    {
        return record switch
        {
            // 1. Validation (also catches default(CallRecord))
            _ when string.IsNullOrWhiteSpace(record.RecordId)
                => throw new ArgumentException("Invalid record: RecordId is missing."),
            _ when string.IsNullOrWhiteSpace(record.DestinationCountry)
                => throw new ArgumentException("Invalid record: country is missing."),
            _ when double.IsNaN(record.DurationMinutes)
                => throw new ArgumentException("Invalid record: duration is NaN."),
            _ when double.IsInfinity(record.DurationMinutes)
                   || record.DurationMinutes < 0.0
                   || record.DurationMinutes > CallRecord.MaxDurationMinutes
                => throw new ArgumentException("Invalid record: duration out of range."),

            // 2. Roaming + KZ + < 1 minute: flat fee
            { IsRoaming: true, DestinationCountry: "KZ", DurationMinutes: < 1.0 }
                => 50.00m,

            // 3. Non-roaming + KZ
            { IsRoaming: false, DestinationCountry: "KZ" }
                => Round(15.00m * (decimal)record.DurationMinutes),

            // 4. Roaming + >= 10 minutes
            { IsRoaming: true, DurationMinutes: >= 10.0 }
                => Round(120.00m * (decimal)record.DurationMinutes),

            // 5. Fallback
            _ => Round(45.00m * (decimal)record.DurationMinutes)
        };
    }

    private static decimal Round(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}