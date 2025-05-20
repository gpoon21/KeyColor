using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using System;

#if !NET8_0_OR_GREATER
using KeyColor.Standard;
#endif

namespace KeyColor.Benchmark;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[ShortRunJob(RuntimeMoniker.NetCoreApp31)] // For .NET Standard 2.0 compatibility testing
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

#if NET8_0_OR_GREATER
    [Benchmark(Description = "ColorFrom.Span (NET8+)")]
    public GeneratedColor BenchmarkSpanNet8() {
        ReadOnlySpan<byte> span = _byteArray.AsSpan();
        return ColorFrom.Span(span);
    }
#endif

#if !NET8_0_OR_GREATER
    [Benchmark(Description = "ColorFrom.Span (Standard)")]
    public GeneratedColor BenchmarkSpanStandard() {
        return ColorFrom.ByteArray(_byteArray);
    }
#endif

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