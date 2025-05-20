using BenchmarkDotNet.Running;

namespace KeyColor.Benchmark;

public class Program {
    public static void Main(string[] args) {
        BenchmarkRunner.Run<ColorFromBenchmark>();
    }
}