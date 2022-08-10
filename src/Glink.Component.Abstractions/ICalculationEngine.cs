using System.Buffers;

namespace Glink.Component.Abstractions
{
    /// <summary>
    /// 计算引擎
    /// </summary>
    public interface ICalculationEngine
    {
        /// <summary>
        /// 标识
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// 类型
        /// 独立算子: 0, 组合算子: 1
        /// </summary>
        public int Type { get;  }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get;  }

        /// <summary>
        /// 要订阅的算子标识
        /// 没有则为空
        /// </summary>
        public long[] Topic { get; }

        /// <summary>
        /// 是否接收原始数据
        /// True: 接收; False:不接收
        /// </summary>
        public bool ReceivedRawData { get; }

        /// <summary>
        /// 计算结果是否允许发布至订阅中心
        /// </summary>
        public bool AllowPublish { get; }

        /// <summary>
        /// 是否推送给业务系统
        /// </summary>
        public bool AllowPush { get; }

        /// <summary>
        /// 数据接收过滤器
        /// 用于判断是否接收发布的数据
        /// </summary>
        /// <returns></returns>
        public bool DataFilter(ReadOnlySequence<byte> data);

        /// <summary>
        /// X 维度数据筛选器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">原始数据</param>
        /// <returns></returns>
        public ValueTuple<T, int[]> XDimensionDataFilter<T>(ReadOnlySequence<byte> data);

        /// <summary>
        /// Y 维度数据筛选器
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="xValue">X 维度筛选结果</param>
        /// <param name="yIndexs">Y维度 筛选序列索引; 为空或者null时，表示无筛选数据</param>
        /// <param name="data">原始数据</param>
        /// <returns></returns>
        public ValueTuple<T2, int[]> YDimensionDataFilter<T1, T2>(T1 xValue, int[] yIndexs, ReadOnlySequence<byte> data);

        /// <summary>
        /// Z 维度数据筛选器
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="xValue">X 维度筛选结果</param>
        /// <param name="yValue">y 维度筛选结果</param>
        /// <param name="zIndexs">z维度 筛选序列索引; 为空或者null时，表示无筛选数据</param>
        /// <param name="data">原始数据</param>
        /// <returns></returns>
        public ValueTuple<T3, int[]> ZDimensionDataFilter<T1, T2, T3>(T1 xValue, T2 yValue, int[] zIndexs, ReadOnlySequence<byte> data);

        /// <summary>
        /// 函数计算
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public T2 Calculate<T1, T2>(T1 value);

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] Execute(ReadOnlySequence<byte> data);
    }
}
