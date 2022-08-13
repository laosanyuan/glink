using Glink.Component.Abstractions;
using System.Buffers;

namespace Glink.Components.MA
{
    public class MA5CompactCalculateEngine : ICalculationEngine
    {
        const byte comma = 44;              // 逗号
        const byte dot = 46;                // 小数点

        private int _shortTime;                             // 用来比较的时间
        private int _timeInt;                               // 最后一次保存的当前时间
        private double[] _historyPrices = new double[5];    // 历史k线收盘价收盘价
        private int _pricePointer;                          // 历史价格变更游动下标
        private bool _isCalculateReady;                     // 累计数据达到计算所用数据量


        public long Id => 6;

        public int Type => 0;

        public string Name => "MA5";

        public long[] Topic => null!;

        public bool ReceivedRawData => true;

        public bool AllowPublish => true;

        public bool AllowPush => false;

        public T2 Calculate<T1, T2>(T1 value)
        {
            throw new NotImplementedException();
        }

        public (int, int, double) Calculate(int time, double zResult)
        {
            // 按股票在对应数组中滑动更新，数据总是为最近五根
            this._historyPrices[this._pricePointer] = zResult;
            if (!this._isCalculateReady && this._pricePointer == 4)
            {
                this._isCalculateReady = true;
            }
            if (this._pricePointer == 4)
            {
                this._pricePointer = 0;
            }
            else
            {
                this._pricePointer++;
            }

            if (this._isCalculateReady)
            {
                var ma5 = this._historyPrices.Average();
                return (1, time, ma5);
            }

            return default;
        }

        public bool DataFilter(ReadOnlySequence<byte> data)
        {
            return true;
        }

        public byte[] Execute(ReadOnlySequence<byte> data)
        {
            var timeResult = YDimensionDataFilter(data);
            if (timeResult.Item2 != 0)
            {
                var close = ZDimensionDataFilter(timeResult.Item2, data);
                var result = Calculate(timeResult.Item1, close);
                if (result.Item1 == 1)
                {
                    // 序列化数据
                    byte[] bytes = new byte[18];
                    int position = 0;

                    var security = BitConverter.GetBytes(result.Item1);
                    security.CopyTo(bytes, position);
                    position += 4;
                    // 每项数据之间以逗号间隔
                    bytes[position] = comma;
                    position++;
                    var time = BitConverter.GetBytes(result.Item2);
                    time.CopyTo(bytes, position);
                    position += 4;
                    bytes[position] = comma;
                    position++;
                    var ma5 = BitConverter.GetBytes(result.Item3);
                    ma5.CopyTo(bytes, position);
                    return bytes;
                }
            }
            return default!;
        }

        public (T, int[]) XDimensionDataFilter<T>(ReadOnlySequence<byte> data)
        {
            return default;
        }

        public (T2, int[]) YDimensionDataFilter<T1, T2>(T1 xValue, int[] yIndexs, ReadOnlySequence<byte> data)
        {
            throw new NotImplementedException();
        }

        public (int, int) YDimensionDataFilter(ReadOnlySequence<byte> data)
        {
            var timeInt = 0;
            bool isValid = false;
            for (int i = 0; i < 9; i++)
            {
                timeInt = (timeInt << 3) + (timeInt << 1) + data.FirstSpan[i] - 48;

                if (i == 5 && timeInt != _shortTime)
                {
                    // 秒级时间发生变化时，保存当前缓存的时间和索引
                    this._shortTime = timeInt;
                    isValid = true;
                }
            }
            var tmpTime = this._timeInt;
            this._timeInt = timeInt;
            if (isValid)
            {
                return (tmpTime, 15);
            }
            return default;
        }

        public (T3, int[]) ZDimensionDataFilter<T1, T2, T3>(T1 xValue, T2 yValue, int[] zIndexs, ReadOnlySequence<byte> data)
        {
            throw new NotImplementedException();
        }

        public double ZDimensionDataFilter(int zIndex, ReadOnlySequence<byte> data)
        {
            int commaCount = 0;
            var price = 0.0;
            bool dotFlag = false;
            double divisor = 1.0;
            double close = 0.0;
            for (int j = zIndex; j < data.Length; j++)
            {
                var item = data.FirstSpan[j];
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
                        close = price;
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
            return close;
        }
    }
}
