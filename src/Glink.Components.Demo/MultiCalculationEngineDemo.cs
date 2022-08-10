using Glink.Component.Abstractions;
using System.Buffers;

namespace Glink.Components.Demo
{
    /// <summary>
    /// 组合计算算子示例
    /// </summary>
    public class MultiCalculationEngineDemo : ICalculationEngine
    {
        public long Id => -2;
        public int Type => 1;
        public long[] Topic => new long[] { -1 };
        public bool AllowPublish => true;
        public bool AllowPush => true;
        public string Name => "MultiCalculationEngineDemo";
        public bool ReceivedRawData => false;

        public T2 Calculate<T1, T2>(T1 value)
        {
            throw new NotImplementedException();
        }

        public bool DataFilter(ReadOnlySequence<byte> data)
        {
            return true;
        }

        public byte[] Execute(ReadOnlySequence<byte> data)
        {
            var value = System.Text.Encoding.UTF8.GetString(data.ToArray()).Split(",");
            value[0] = Id.ToString();
            var result = value.Aggregate(string.Empty, (total, next) =>
            {
                return total + next + ",";
            });
            return System.Text.Encoding.UTF8.GetBytes(result);
        }

        public (T, int[]) XDimensionDataFilter<T>(ReadOnlySequence<byte> data)
        {
            throw new NotImplementedException();
        }

        public (T2, int[]) YDimensionDataFilter<T1, T2>(T1 xValue, int[] yIndexs, ReadOnlySequence<byte> data)
        {
            throw new NotImplementedException();
        }

        public (T3, int[]) ZDimensionDataFilter<T1, T2, T3>(T1 xValue, T2 yValue, int[] zIndexs, ReadOnlySequence<byte> data)
        {
            throw new NotImplementedException();
        }
    }
}