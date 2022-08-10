using Glink.Component.Abstractions;
using System.Buffers;

namespace Glink.Components.Demo
{
    /// <summary>
    /// 独立计算算子示例
    /// </summary>
    public class CalculationEngineDemo : ICalculationEngine
    {
        public long Id => -1;
        public int Type => 0;
        public long[] Topic { get; }
        public bool AllowPublish => true;
        public bool AllowPush => true;
        public string Name => "CalculationEngineDemo";
        public bool ReceivedRawData => true;

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
            var value = System.Text.Encoding.UTF8.GetString(data.ToArray());
            var result = Id + "," + value;
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
