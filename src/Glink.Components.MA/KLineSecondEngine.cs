using Glink.Component.Abstractions;
using System.Buffers;

namespace Glink.Components.MA
{
    public class KLineSecondEngine : ICalculationEngine
    {
        const byte comma = 44;              // 逗号
        const byte dot = 46;                // 小数点

        private double[] _kData = new double[4];    // k线数据 => high,low,open,close
        private int _time;                          // k线时间

        public long Id => 3;

        public int Type => 0;

        public string Name => "秒K线";

        public long[] Topic { get; set; } = null!;

        public bool ReceivedRawData => true;

        public bool AllowPublish => true;

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

                // 获取时间
                var time = 0;
                for (int i = 0; i < 6; i++)
                {
                    time = time * 10 + data.FirstSpan[i] - 48;
                }

                // 获取现价
                bool dotFlag = false;
                int commaCount = 0;
                double divisor = 1.0;
                double price = 0.0;
                for (int i = 10; i < data.Length; i++)
                {
                    var item = data.FirstSpan[i];
                    if (commaCount < 2)
                    {
                        if (item == comma)
                        {
                            commaCount++;
                        }
                        continue;
                    }
                    else if (commaCount == 2)
                    {
                        if (item == comma)
                        {
                            break;
                        }
                        else if (item == dot)
                        {
                            dotFlag = true;
                            continue;
                        }
                        if (dotFlag)
                        {
                            divisor *= 10;
                            price += ((item - 48) / divisor);
                        }
                        else
                        {
                            price = (price * 10) + (item - 48);
                        }
                    }
                }

                byte[] result = default!;

                if (time == _time)
                {
                    // 当前k线未计算完成
                    if (price > _kData[0])
                    {
                        _kData[0] = price;
                    }
                    if (price < _kData[1])
                    {
                        _kData[1] = price;
                    }
                    _kData[3] = price;
                }
                else
                {
                    if (_time != 0)
                    {
                        // 有效k线数据
                        result = new byte[4 + 4 + 32];
                        var timeBytes = BitConverter.GetBytes(time * 1000);
                        timeBytes.CopyTo(result, 0);
                        int startIndex = timeBytes.Length;
                        for (int i = 0; i < 4; i++)
                        {
                            result[startIndex] = comma;
                            startIndex++;
                            var tmp = BitConverter.GetBytes(_kData[i]);
                            tmp.CopyTo(result, startIndex);
                            startIndex += 8;
                        }
                    }

                    // 新一根k线
                    _time = time;
                    for (int i = 0; i < 4; i++)
                    {
                        _kData[i] = price;
                    }
                }
                return result!;
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
