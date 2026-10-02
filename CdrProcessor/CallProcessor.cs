namespace CdrProcessor;

public static class CallProcessor
{
    // Pure-core baseline: just applies CalculateCost to each record.
    public static decimal ProcessCallsSequential(CallRecord[] records)
    {
        ArgumentNullException.ThrowIfNull(records);

        decimal total = 0m;
        foreach (var record in records)
            total += CallPricing.CalculateCost(in record);
        return total;
    }

    public static decimal ProcessCallsParallel(CallRecord[] records)
    {
        ArgumentNullException.ThrowIfNull(records);
        if (records.Length % 2 != 0)
            throw new ArgumentException("Array length must be even.", nameof(records));
        if (records.Length == 0)
            return 0m;

        int mid = records.Length / 2;
        CallRecord[] leftInput = records[..mid];   // array range, not list pattern
        CallRecord[] rightInput = records[mid..];

        // Independent output arrays, allocated BEFORE starting any worker
        decimal[] leftOutput = new decimal[leftInput.Length];
        decimal[] rightOutput = new decimal[rightInput.Length];

        // Each variable is written by exactly one worker thread
        Exception? leftError = null;
        Exception? rightError = null;

        var leftThread = new Thread(() =>
        {
            try { FillCosts(leftInput, leftOutput); }
            catch (Exception ex) { leftError = ex; }
        });
        var rightThread = new Thread(() =>
        {
            try { FillCosts(rightInput, rightOutput); }
            catch (Exception ex) { rightError = ex; }
        });

        leftThread.Start();
        rightThread.Start();
        leftThread.Join();     // Join gives a happens-before edge:
        rightThread.Join();    // all worker writes are visible after it

        if (leftError is not null || rightError is not null)
        {
            var errors = new List<Exception>();
            if (leftError is not null) errors.Add(leftError);
            if (rightError is not null) errors.Add(rightError);
            throw new AggregateException("Worker failed; partial totals discarded.", errors);
        }

        decimal total = 0m;
        foreach (var c in leftOutput) total += c;
        foreach (var c in rightOutput) total += c;
        return total;
    }

    // IMPURE: mutates its output array (but only its own one)
    private static void FillCosts(CallRecord[] input, decimal[] output)
    {
        for (int i = 0; i < input.Length; i++)
            output[i] = CallPricing.CalculateCost(in input[i]);
    }
}