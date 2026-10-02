namespace CdrProcessor;

public readonly record struct CallRecord
{
    public const double MaxDurationMinutes = 10_000.0;

    public string RecordId { get; }
    public string DestinationCountry { get; }
    public double DurationMinutes { get; }
    public bool IsRoaming { get; }

    public CallRecord(string recordId, string destinationCountry, double durationMinutes, bool isRoaming)
    {
        if (string.IsNullOrWhiteSpace(recordId))
            throw new ArgumentException("RecordId must not be null, empty or whitespace.", nameof(recordId));

        if (string.IsNullOrWhiteSpace(destinationCountry))
            throw new ArgumentException("DestinationCountry must not be null, empty or whitespace.", nameof(destinationCountry));

        if (double.IsNaN(durationMinutes) || double.IsInfinity(durationMinutes)
            || durationMinutes < 0.0 || durationMinutes > MaxDurationMinutes)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes),
                "Duration must be finite and within [0, 10000].");

        RecordId = recordId;
        DestinationCountry = destinationCountry;
        DurationMinutes = durationMinutes;
        IsRoaming = isRoaming;
    }
}