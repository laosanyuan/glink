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

        [Params(200000)]
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
            MA5CompactCalculateEngine ma = new MA5CompactCalculateEngine();
            foreach (var data in datas)
            {
                // 股票筛选
                var securities = ma.XDimensionDataFilter<object>(data);
            }
        }

        [Benchmark(Description = "筛选时间")]
        public void TestTimeFilter()
        {
            MA5CompactCalculateEngine ma = new MA5CompactCalculateEngine();
            foreach (var data in datas)
            {
                // 时间筛选
                var times = ma.YDimensionDataFilter(data);
            }
        }

        [Benchmark(Description = "筛选价格")]
        public void TestPriceFilter()
        {
            MA5CompactCalculateEngine ma = new MA5CompactCalculateEngine();
            foreach (var data in datas)
            {
                // 时间筛选
                var times = ma.YDimensionDataFilter(data);
                if (times.Item2 != 0)
                {
                    // 收盘价格筛选
                    var closes = ma.ZDimensionDataFilter(times.Item2, data);
                }
            }
        }

        [Benchmark(Description = "计算MA5")]
        public void Calculate()
        {
            MA5CompactCalculateEngine ma = new MA5CompactCalculateEngine();
            foreach (var data in datas)
            {
                // 时间筛选
                var time = ma.YDimensionDataFilter(data);
                if (time.Item2 != 0)
                {
                    // 收盘价格筛选
                    var close = ma.ZDimensionDataFilter(time.Item2, data);
                    var result = ma.Calculate(time.Item1, close);
                }
            }

        }

        [Benchmark(Description = "执行")]
        public void TestExecute()
        {
            MA5CompactCalculateEngine ma = new MA5CompactCalculateEngine();
            foreach (var data in datas)
            {
                var result = ma.Execute(data);
            }
        }
    }
}
