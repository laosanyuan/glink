namespace Glink.Runtime.Application.Contracts.MessageCenter
{
    /// <summary>
    /// 消息发布者
    /// </summary>
    public interface IMessagePublisher<T>
    {
        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="id">发布者标识</param>
        /// <param name="data">发布数据</param>
        /// <returns></returns>
        public Task Publish(string id, byte[] data);
    }
}
