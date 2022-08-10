using Glink.Component.Abstractions;
using System.Buffers;

namespace Glink.Components.MA
{
    public class MA5FormatEngine : ICalculationEngine
    {
        public long Id => 2;

        public int Type => 1;

        public string Name => "MA5Format";

        public long[] Topic => new long[] { 1 };

        public bool ReceivedRawData => false;

        public bool AllowPublish => false;

        public bool AllowPush => true;

        public T2 Calculate<T1, T2>(T1 value)
        {
            throw new NotImplementedException();
        }

        public bool DataFilter(ReadOnlySequence<byte> data)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// Id,time,....
        public byte[] Execute(ReadOnlySequence<byte> data)
        {
            var code = BitConverter.ToInt32(data.Slice(0, 4).ToArray());
            var time = BitConverter.ToInt32(data.Slice(5, 4).ToArray());
            var ma5 = BitConverter.ToDouble(data.Slice(10, 8).ToArray());
            var result = Id + "," + time + "," + ma5 + "," + code;

            return System.Text.Encoding.Default.GetBytes(result);
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
