using CdrProcessor;
using Xunit;

namespace CdrProcessor.Tests;

public class PipelineTests
{
    private static CallRecord[] BuildRecords(int count)
    {
        string[] countries = { "KZ", "US", "DE", "XX" };
        var records = new CallRecord[count];
        for (int i = 0; i < count; i++)
            records[i] = new CallRecord($"R{i:D4}", countries[i % 4], (i % 40) * 0.37, i % 3 == 0);
        return records;
    }

    [Fact]
    public void Parallel_MatchesSequential_Over100Runs()
    {
        var records = BuildRecords(1000);
        var snapshot = (CallRecord[])records.Clone();
        decimal expected = CallProcessor.ProcessCallsSequential(records);

        for (int run = 0; run < 100; run++)
            Assert.Equal(expected, CallProcessor.ProcessCallsParallel(records));

        Assert.Equal(snapshot, records);   // input unchanged
    }

    [Fact]
    public void Parallel_EmptyArray_ReturnsZero() =>
        Assert.Equal(0m, CallProcessor.ProcessCallsParallel(Array.Empty<CallRecord>()));

    [Fact]
    public void Parallel_OddLength_Throws() =>
        Assert.Throws<ArgumentException>(() => CallProcessor.ProcessCallsParallel(BuildRecords(3)));

    [Fact]
    public void Parallel_Null_Throws() =>
        Assert.Throws<ArgumentNullException>(() => CallProcessor.ProcessCallsParallel(null!));

    [Fact]
    public void Parallel_WorkerFailure_IsRethrownOnCallingThread()
    {
        var records = BuildRecords(4);
        records[3] = default;   // invalid -> worker throws
        Assert.Throws<AggregateException>(() => CallProcessor.ProcessCallsParallel(records));
    }
}