using Glink.Runtime.Application.Contracts.MessageCenter;

namespace Glink.Runtime.Application.Contracts.Pipline
{
    /// <summary>
    /// 计算管道
    /// </summary>
    public interface ICalculationPipline : IMessageDistributor
    {
        /// <summary>
        /// 是否允许推送给业务方
        /// </summary>
        public bool AllowPush { get; }

        /// <summary>
        /// 类型
        /// </summary>
        public long Type { get; }

        /// <summary>
        /// 是否接收原始数据
        /// True: 接收; False: 不接收
        /// </summary>
        public bool ReceivedRawData { get; }

        /// <summary>
        /// 运行状态
        /// </summary>
        public CalculationPiplineStatus Status { get; set; }

        /// <summary>
        /// 设置
        /// </summary>
        public CalculationPiplineSetting Setting { get; }

        /// <summary>
        /// 开始消费数据 （消费模式）
        /// </summary>
        public void Start();

        /// <summary>
        /// 停止接收数据
        /// </summary>
        public void Stop();

        /// <summary>
        /// 更新设置
        /// </summary>
        /// <param name="Setting"></param>
        public void Update(CalculationPiplineSetting Setting);

        /// <summary>
        /// 推送给业务方
        /// </summary>
        /// <param name="serilizer">数据序列化处理器</param>
        /// <param name="handler">推送处理器</param>
        /// <param name="setting">推送设置</param>
        public void StartPush(Func<byte[], MetaDataSerializerInfo> serilizer, Action<byte[]> handler, CalculationPiplineSetting setting);

        /// <summary>
        /// 停止推送
        /// </summary>
        public void StopPush();
    }
}
