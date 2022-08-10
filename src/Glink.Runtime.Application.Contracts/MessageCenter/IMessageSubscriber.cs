namespace Glink.Runtime.Application.Contracts.MessageCenter
{
    /// <summary>
    /// 消息订阅者
    /// </summary>
    public interface IMessageSubscriber<T>
    {
        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="pubIds">发布者标识</param>
        /// <param name="sub">订阅者实例</param>
        public void Subscribe(IEnumerable<string> pubIds, T sub);

        /// <summary>
        /// 解订阅
        /// </summary>
        /// <param name="sub">订阅者实例</param>
        /// <param name="pubIds">消息发布者标识</param>
        public void UnSubscribe(IEnumerable<string> pubIds, T sub);
    }
}
