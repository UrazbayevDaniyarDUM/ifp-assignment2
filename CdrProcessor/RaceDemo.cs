namespace CdrProcessor;

internal static class RaceDemo
{
    private static int globalCallCounter = 0;

    // UNSAFE if called from two threads at once: ++ is read-modify-write.
    internal static void ProcessCalls(CallRecord[] records)
    {
        foreach (var record in records)
        {
            _ = CallPricing.CalculateCost(in record);
            globalCallCounter++;
        }
    }
}