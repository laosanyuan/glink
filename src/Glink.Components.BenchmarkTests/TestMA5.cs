using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Glink.Components.MA;
using System.Buffers;

namespace Glink.Components.BenchmarkTests
{
    [SimpleJob(RuntimeMoniker.Net60, baseline: true)]
    [MemoryDiagnoser]
    [RPlotExporter]
    public class TestMA5
    {
        public readonly List<ReadOnlySequence<byte>> datas = new List<ReadOnlySequence<byte>>();

        [Params(100000)]
        public int DataCount;

        [GlobalSetup]
        public void Setup()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "600418.csv");

            using (var sr = new StreamReader(path))
            {
                string line;
                sr.ReadLine();
                int count = 0;
                while ((line = sr.ReadLine()!) != null)
                {
                    var data = new ReadOnlySequence<byte>(System.Text.Encoding.Default.GetBytes(line));
                    datas.Add(data);
                    count++;
                    if (count >= DataCount)
                    {
                        break;
                    }
                }
            }
        }

        [Benchmark(Description = "筛选股票")]
        public void TestSecurityFilter()
        {
            MA5CalculateEngine ma = new MA5CalculateEngine();
            foreach (var data in datas)
            {
                // 股票筛选
                var securities = ma.XDimensionDataFilter(data);
            }
        }

        [Benchmark(Description = "筛选时间")]
        public void TestTimeFilter()
        {
            MA5CalculateEngine ma = new MA5CalculateEngine();
            foreach (var data in datas)
            {
                // 股票筛选
                var securities = ma.XDimensionDataFilter(data);
                if (securities.Item1 != null)
                {
                    // 时间筛选
                    var times = ma.YDimensionDataFilter(securities.Item1, securities.Item2, data);
                }
            }
        }

        [Benchmark(Description = "筛选价格")]
        public void TestPriceFilter()
        {
            MA5CalculateEngine ma = new MA5CalculateEngine();
            foreach (var data in datas)
            {
                // 股票筛选
                var securities = ma.XDimensionDataFilter(data);
                if (securities.Item1 != null)
                {
                    // 时间筛选
                    var times = ma.YDimensionDataFilter(securities.Item1, securities.Item2, data);
                    if (times.Item1 != null)
                    {
                        // 收盘价格筛选
                        var closes = ma.ZDimensionDataFilter(times.Item2, data);
                    }
                }
            }
        }

        [Benchmark(Description = "计算MA5")]
        public void Calculate()
        {
            MA5CalculateEngine ma = new MA5CalculateEngine();
            foreach (var data in datas)
            {

                // 股票筛选
                var securities = ma.XDimensionDataFilter(data);
                if (securities.Item1 != null)
                {
                    // 时间筛选
                    var times = ma.YDimensionDataFilter(securities.Item1, securities.Item2, data);
                    if (times.Item1 != null)
                    {
                        // 收盘价格筛选
                        var closes = ma.ZDimensionDataFilter(times.Item2, data);
                        var calculateResult = ma.Calculate(times.Item1, closes);
                    }
                }
            }
        }

        [Benchmark(Description = "执行")]
        public void TestExecute()
        {
            MA5CalculateEngine ma = new MA5CalculateEngine();
            foreach (var data in datas)
            {
                var result = ma.Execute(data);
            }
        }
    }
}
