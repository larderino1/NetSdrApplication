using BenchmarkDotNet.Running;

namespace NetSdrBenchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run<BytesConversionBenchmark>();
        }
    }
}
