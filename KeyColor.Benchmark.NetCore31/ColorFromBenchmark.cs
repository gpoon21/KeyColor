using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System;
using KeyColor.Standard;

namespace KeyColor.Benchmark.NetCore31 {
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [ShortRunJob]
    [RankColumn]
    public class ColorFromBenchmark {
        private const string _stringKey = "TestKey";
        private readonly byte[] _byteArray = new byte[] { 1, 2, 3, 4, 5 };
        private readonly TestStruct _structData = new TestStruct(42, 1.25);

        [Benchmark(Description = "ColorFrom.String")]
        public GeneratedColor BenchmarkString() {
            return ColorFrom.String(_stringKey);
        }

        [Benchmark(Description = "ColorFrom.Key<struct>")]
        public GeneratedColor BenchmarkStruct() {
            return ColorFrom.Key(_structData);
        }

        [Benchmark(Description = "ColorFrom.ByteArray")]
        public GeneratedColor BenchmarkByteArray() {
            return ColorFrom.ByteArray(_byteArray);
        }

        [Benchmark(Description = "ColorFrom.Rng")]
        public GeneratedColor BenchmarkRng() {
            return ColorFrom.Rng();
        }

        // Test struct for benchmarking
        private struct TestStruct {
            public TestStruct(int val1, double val2) {
                Val1 = val1;
                Val2 = val2;
            }

            public int Val1 { get; }
            public double Val2 { get; }
        }
    }
}