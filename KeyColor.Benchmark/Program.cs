using BenchmarkDotNet.Running;
using System;

namespace KeyColor.Benchmark;

public class Program {
    public static void Main(string[] args) {
        Console.WriteLine($"Running benchmarks on {GetFrameworkVersion()}");
        BenchmarkRunner.Run<ColorFromBenchmark>();
    }

    private static string GetFrameworkVersion()
    {
#if NET9_0
        return ".NET 9.0";
#elif NET8_0
        return ".NET 8.0";
#elif NETSTANDARD2_0
        return ".NET Standard 2.0";
#else
        return "Unknown Framework";
#endif
    }
}