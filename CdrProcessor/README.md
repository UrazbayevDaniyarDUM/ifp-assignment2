# Assignment 2 – Multithreaded Telecom Call Processing Pipeline
FP 3222, Astana IT University. C# 12 / .NET 8.

## Build / run / test
dotnet build
dotnet run --project CdrProcessor
dotnet test

## Architecture
- CallRecord: immutable readonly record struct with explicit constructor validation.
- CallPricing.CalculateCost: pure tariff function (one switch expression).
- CallProcessor: sequential baseline and two-thread partitioned pipeline.
- RaceDemo: the unsafe counter from Task 3 (not used by the pipeline).

## Pure vs impure
| Component | Pure? | Why |
|---|---|---|
| CalculateCost | Yes | Same input -> same output; no I/O, no global state, no mutation |
| ProcessCallsSequential | Effectively | Only local accumulator; no shared state |
| FillCosts / thread creation | No | Mutates output array, creates threads |
| Program.cs | No | Console I/O |

## Why readonly does not imply valid
readonly only prevents mutation after construction. default(CallRecord) skips
the constructor (null strings, 0 duration), so CalculateCost re-validates.
(`with` expressions also bypass the constructor.)

## Race (Task 3)
globalCallCounter++ = read, add, write. Interleaving: counter = 5; T1 reads 5,
T2 reads 5, T1 writes 6, T2 writes 6. Two increments happened, counter grew
by 1 (lost update). A sequential loop has one thread, so no interleaving.

## Partitioning rationale
Input is split via [..mid] and [mid..]. Each worker reads only its own input
and writes only its own decimal[]. No element is shared, so no locks are needed.
Join() guarantees both workers finished (and their writes are visible) before
the outputs are read. Worker exceptions are captured and rethrown as
AggregateException on the calling thread.

## Test results
(paste the output of `dotnet test`: "Passed! - Failed: 0, Passed: N")

## Limitations
- Threads are not necessarily faster than the sequential loop for small arrays.
- 100 passing runs are evidence, not a proof of race freedom.
- Only two threads, array length must be even.