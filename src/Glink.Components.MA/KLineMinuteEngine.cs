using Glink.Component.Abstractions;
using System.Buffers;

namespace Glink.Components.MA
{
    public class KLineMinuteEngine : ICalculationEngine
    {
        const byte comma = 44;              // 逗号
        private int _minute;
        private double[] _value = new double[4];

        public long Id => 4;

        public int Type => 1;

        public string Name => "分钟K线";

        public long[] Topic => new long[] { 3 };

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

        public byte[] Execute(ReadOnlySequence<byte> data)
        {
            try
            {
                if (data.Length <= 0)
                {
                    return default!;
                }
                // 合并k线
                int time = BitConverter.ToInt32(data.Slice(0, 4).FirstSpan);
                var high = BitConverter.ToDouble(data.Slice(5, 8).FirstSpan);
                var low = BitConverter.ToDouble(data.Slice(14, 8).FirstSpan);
                var close = BitConverter.ToDouble(data.Slice(32, 8).FirstSpan);
                if (time / 100_000 != _minute)
                {
                    _minute = time / 100_000;
                    _value[2] = BitConverter.ToDouble(data.Slice(23, 8).FirstSpan);
                    _value[0] = high;
                    _value[1] = low;
                }
                else
                {
                    if (high > _value[0])
                    {
                        _value[0] = high;
                    }
                    if (low < _value[1])
                    {
                        _value[1] = low;
                    }
                }
                _value[3] = close;

                // 序列化数据
                // 时间，hloc
                byte[] result = new byte[4 + 4 + 32];
                var timeBytes = BitConverter.GetBytes(_minute * 100_000);
                timeBytes.CopyTo(result, 0);
                int startIndex = timeBytes.Length;
                for (int i = 0; i < 4; i++)
                {
                    result[startIndex] = comma;
                    startIndex++;
                    var tmp = BitConverter.GetBytes(_value[i]);
                    tmp.CopyTo(result, startIndex);
                    startIndex += 8;
                }
                return result;
            }
            catch (Exception)
            {
                return default!;
            }
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
