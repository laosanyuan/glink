using Glink.Component.Abstractions;
using System.Buffers;

namespace Glink.Components.MA
{
    /// <summary>
    /// MA5加倍计算
    /// </summary>
    public class MA5DoubleCalculateEngine : ICalculationEngine
    {
        const byte comma = 44;              // 逗号
        const byte dot = 46;                // 小数点
        const byte cemicolon = 59;          // 分号

        private int[] _securitys;           // 支持筛选的股票代码
        private int[] _shortTimes;          // 用来比较的时间
        private int[] _timeIndexs;          // 时间索引
        private int[] _timeInts;            // 最后一次保存的当前时间
        private double[][] _historyPrices;  // 历史k线收盘价收盘价
        private int[] _pricePointers;       // 历史价格变更游动下标
        private bool[] _isCalculateReady;   // 累计数据达到计算所用数据量

        /// <summary>
        /// 标识
        /// </summary>
        public long Id { get; set; } = 5;
        /// <summary>
        /// 类型
        /// 独立算子：0，组合算子：1
        /// </summary>
        public int Type { get; set; } = 0;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name => "MA5Double";
        /// <summary>
        /// 需要订阅的算子标识
        /// </summary>
        public long[] Topic { get; set; } = null!;
        /// <summary>
        /// 计算结果是否允许发布至订阅中心
        /// </summary>
        public bool AllowPublish { get; set; } = false;
        /// <summary>
        /// 是否推送给业务系统
        /// </summary>
        public bool AllowPush { get; set; } = true;
        /// <summary>
        /// 是否接收原始数据
        /// </summary>
        public bool ReceivedRawData => true;

        public MA5DoubleCalculateEngine()
        {
            _securitys = new int[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            _shortTimes = new int[_securitys.Length];
            _timeIndexs = new int[_securitys.Length];
            _timeInts = new int[_securitys.Length];
            this._historyPrices = new double[_securitys.Length][];
            for (int i = 0; i < _historyPrices.Length; i++)
            {
                _historyPrices[i] = new double[5];
            }
            _pricePointers = new int[_securitys.Length];
            _isCalculateReady = new bool[_securitys.Length];
        }

        /// <summary>
        /// 执行算子
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] Execute(ReadOnlySequence<byte> data)
        {
            // 股票筛选
            var securities = XDimensionDataFilter(data);
            if (securities.Item1 != null)
            {
                // 时间筛选
                var times = YDimensionDataFilter(securities.Item1, securities.Item2, data);
                if (times.Item1 != null)
                {
                    // 收盘价格筛选
                    var closes = ZDimensionDataFilter(times.Item2, data);
                    var calculateResult = Calculate(times.Item1, closes);
                    if (calculateResult.Item1 != null)
                    {
                        // 序列化数据
                        byte[] result = new byte[18 * calculateResult.Item1.Length + calculateResult.Item1.Length - 1];
                        int position = 0;
                        for (int i = 0; i < calculateResult.Item1.Length; i++)
                        {
                            var security = BitConverter.GetBytes(calculateResult.Item1[i]);
                            security.CopyTo(result, position);
                            position += 4;
                            // 每项数据之间以逗号间隔
                            result[position] = comma;
                            position++;
                            var time = BitConverter.GetBytes(calculateResult.Item2[i]);
                            time.CopyTo(result, position);
                            position += 4;
                            result[position] = comma;
                            position++;
                            var ma5 = BitConverter.GetBytes(calculateResult.Item3[i]);
                            ma5.CopyTo(result, position);
                            position += 8;
                            if (i < calculateResult.Item1.Length - 1)
                            {
                                // 每组数据之间以分号间隔
                                result[position] = cemicolon;
                            }
                        }
                        return result;
                    }
                }
            }
            return default!;
        }

        /// <summary>
        /// 数据过滤器
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool DataFilter(ReadOnlySequence<byte> data)
        {
            return true;
        }

        /// <summary>
        /// 股票筛选器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public (T, int[]) XDimensionDataFilter<T>(ReadOnlySequence<byte> data)
        {
            if (typeof(T) == typeof(int[]))
            {
                return ((T)Convert.ChangeType(new int[] { 1 }, typeof(T)), new int[] { 1 });
            }
            return default;
        }

        /// <summary>
        /// 股票筛选器
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public (int[], int[]) XDimensionDataFilter(ReadOnlySequence<byte> data)
        {
            return (new int[] { 1 }, new int[] { 0 });
        }

        /// <summary>
        /// 时间筛选器
        /// </summary>
        /// <param name="xValue">股票数据</param>
        /// <param name="yIndexs">时间筛选切片起始索引</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public (int[][], int[]) YDimensionDataFilter(int[] xValue, int[] yIndexs, ReadOnlySequence<byte> data)
        {
            int[][] securityAndTimes = new int[2][];
            securityAndTimes[0] = new int[yIndexs.Length];  // 股票名称
            securityAndTimes[1] = new int[yIndexs.Length];  // 时间
            int[] indexs = new int[yIndexs.Length];         // z索引

            int resultIndex = 0;
            for (int i = 0; i < xValue.Length; i++)
            {
                var timeInt = 0;
                bool isValid = false;
                int securityIndex = this._securitys.First(s => s == xValue[i]);
                for (int j = yIndexs[i]; j < yIndexs[i] + 9; j++)
                {
                    timeInt = (timeInt << 3) + (timeInt << 1) + data.FirstSpan[j] - 48;

                    if (j == yIndexs[i] + 5 && timeInt != _shortTimes[securityIndex])
                    {
                        // 秒级时间发生变化时，保存当前缓存的时间和索引
                        this._shortTimes[securityIndex] = timeInt;
                        isValid = true;
                    }
                }
                this._timeInts[securityIndex] = timeInt;
                if (isValid)
                {
                    securityAndTimes[0][resultIndex] = xValue[i];
                    securityAndTimes[1][resultIndex] = this._timeInts[securityIndex];
                    indexs[resultIndex] = this._timeIndexs[securityIndex];
                    resultIndex++;
                }
                this._timeIndexs[securityIndex] = yIndexs[i] + 10;
            }

            if (resultIndex != 0)
            {
                return (securityAndTimes, indexs);
            }
            return default;
        }

        /// <summary>
        /// 时间筛选器
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="xValue">股票数据</param>
        /// <param name="yIndexs">时间筛选切片起始索引</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public (T2, int[]) YDimensionDataFilter<T1, T2>(T1 xValue, int[] yIndexs, ReadOnlySequence<byte> data)
        {
            return default;
        }

        /// <summary>
        /// 收盘价格筛选器
        /// </summary>
        /// <param name="zIndexs">价格筛选切片起始索引</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public double[] ZDimensionDataFilter(int[] zIndexs, ReadOnlySequence<byte> data)
        {
            double[] closes = new double[zIndexs.Count(t => t != 0)];   // 长度为实际长度

            for (int i = 0; i < zIndexs.Length; i++)
            {
                if (zIndexs[i] == 0)
                {
                    // 数组后续数据无效
                    break;
                }
                int commaCount = 0;
                var price = 0.0;
                bool dotFlag = false;
                double divisor = 1.0;

                for (int j = i; j < data.Length; j++)
                {
                    var item = data.FirstSpan[j];
                    if (commaCount < 3)
                    {
                        if (item == comma)
                        {
                            commaCount++;
                        }
                        continue;
                    }
                    else if (commaCount == 3)
                    {
                        if (item == comma)
                        {
                            closes[i] = price;
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
                closes[i] = price;
            }
            return closes;
        }

        /// <summary>
        /// 收盘价格筛选器
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="xValue">股票维度筛选数据</param>
        /// <param name="yValue">时间维度筛选数据</param>
        /// <param name="zIndexs">价格维度筛选数据</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public (T3, int[]) ZDimensionDataFilter<T1, T2, T3>(T1 xValue, T2 yValue, int[] zIndexs, ReadOnlySequence<byte> data)
        {
            return default;
        }

        /// <summary>
        /// 计算MA5
        /// </summary>
        /// <param name="yResult">股票、时间数据</param>
        /// <param name="zResult">价格数据</param>
        /// <returns></returns>
        public (int[], int[], double[]) Calculate(int[][] yResult, double[] zResult)
        {
            int[] securitys = new int[zResult.Length];
            int[] times = new int[zResult.Length];
            double[] ma5s = new double[zResult.Length];
            int resultIndex = 0;
            for (int i = 0; i < zResult.Length; i++)
            {
                int securityIndex = this._securitys.First(t => t == yResult[0][i]);

                // 按股票在对应数组中滑动更新，数据总是为最近五根
                this._historyPrices[securityIndex][this._pricePointers[securityIndex]] = zResult[i];
                if (!this._isCalculateReady[securityIndex] && this._pricePointers[securityIndex] == 4)
                {
                    this._isCalculateReady[securityIndex] = true;
                }
                if (this._pricePointers[securityIndex] == 4)
                {
                    this._pricePointers[securityIndex] = 0;
                }
                else
                {
                    this._pricePointers[securityIndex]++;
                }

                if (this._isCalculateReady[securityIndex])
                {
                    ma5s[resultIndex] = this._historyPrices[securityIndex].Average() * 2;
                    securitys[resultIndex] = yResult[0][i];
                    times[resultIndex] = yResult[1][i];
                    resultIndex++;
                }
            }

            if (resultIndex != 0)
            {
                return (securitys, times, ma5s);
            }
            return default;
        }

        /// <summary>
        /// 计算MA5
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="value">计算入参</param>
        /// <returns></returns>
        public T2 Calculate<T1, T2>(T1 value)
        {
            return default!;
        }
    }
}
