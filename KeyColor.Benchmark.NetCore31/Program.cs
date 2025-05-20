using BenchmarkDotNet.Running;
using System;

namespace KeyColor.Benchmark.NetCore31
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Running .NET Core 3.1 benchmarks");
            BenchmarkRunner.Run<ColorFromBenchmark>();
        }
    }
} 