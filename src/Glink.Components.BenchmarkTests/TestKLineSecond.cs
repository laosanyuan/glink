using BenchmarkDotNet.Attributes;
using Glink.Components.MA;
using System.Buffers;

namespace Glink.Components.BenchmarkTests
{
    public class TestKLineSecond
    {
        public readonly List<ReadOnlySequence<byte>> datas = new List<ReadOnlySequence<byte>>();

        [Params(100000)]
        public int DataCount;

        [GlobalSetup]
        public void Setup()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "600418.csv");

            using var sr = new StreamReader(path);
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

        [Benchmark]
        public void TestExecute()
        {
            KLineSecondEngine ma = new KLineSecondEngine();
            foreach (var data in datas)
            {
                ma.Execute(data);
            }
        }
    }
}
