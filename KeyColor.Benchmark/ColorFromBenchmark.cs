using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using System;
using KeyColor;

namespace KeyColor.Benchmark;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[ShortRunJob(RuntimeMoniker.Net90)]
[ShortRunJob(RuntimeMoniker.Net80)]
public class ColorFromBenchmark {
    private const string _stringKey = "TestKey";
    private readonly byte[] _byteArray = [1, 2, 3, 4, 5];
    private readonly TestStruct _structData = new(42, 1.25);

    [Benchmark(Description = "ColorFrom.String")]
    public GeneratedColor BenchmarkString() {
        return ColorFrom.String(_stringKey);
    }

    [Benchmark(Description = "ColorFrom.Key<struct>")]
    public GeneratedColor BenchmarkStruct() {
        return ColorFrom.Key(_structData);
    }

    [Benchmark(Description = "ColorFrom.Span")]
    public GeneratedColor BenchmarkSpan() {
        ReadOnlySpan<byte> span = _byteArray.AsSpan();
        return ColorFrom.Span(span);
    }

    [Benchmark(Description = "ColorFrom.Rng")]
    public GeneratedColor BenchmarkRng() {
        return ColorFrom.Rng();
    }

    // Test struct for benchmarking
    private readonly struct TestStruct {
        public TestStruct(int val1, double val2) {
            Val1 = val1;
            Val2 = val2;
        }

        public int Val1 { get; }
        public double Val2 { get; }
    }
}