using BenchmarkDotNet.Attributes;
using Glink.Components.MA;
using System.Buffers;

namespace Glink.Components.BenchmarkTests
{
    public class TestKlineMinute
    {
        [Params(100000)]
        public int DataCount;

        [Benchmark]
        public void TestExecute()
        {
            // 15.58,
            byte[] bytes = new byte[] {208,36,141,0,44,41,92,143,194,245,40,47,64,44,41,92,143,194,245,40,47,64,44,
                41,92,143,194,245,40,47,64,44,41,92,143,194,245,40,47,64};
            KLineMinuteEngine ma = new KLineMinuteEngine();
            ma.Execute(new ReadOnlySequence<byte>(bytes));
        }
    }
}
