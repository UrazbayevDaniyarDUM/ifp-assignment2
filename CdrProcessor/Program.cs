using CdrProcessor;

string[] countries = { "KZ", "US", "DE", "XX" };
var records = new CallRecord[1000];
for (int i = 0; i < records.Length; i++)
{
    records[i] = new CallRecord(
        recordId: $"R{i:D4}",
        destinationCountry: countries[i % countries.Length],
        durationMinutes: (i % 40) * 0.37,
        isRoaming: i % 3 == 0);
}

decimal sequential = CallProcessor.ProcessCallsSequential(records);
decimal parallel = CallProcessor.ProcessCallsParallel(records);

Console.WriteLine($"Sequential: {sequential:F2} KZT");
Console.WriteLine($"Parallel:   {parallel:F2} KZT");
Console.WriteLine($"Equal: {sequential == parallel}");