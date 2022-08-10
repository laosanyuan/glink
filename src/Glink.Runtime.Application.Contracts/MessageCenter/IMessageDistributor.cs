using System.Buffers;

namespace Glink.Runtime.Application.Contracts.MessageCenter
{
    /// <summary>
    /// 消息分发器
    /// </summary>
    public interface IMessageDistributor
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 分发
        /// </summary>
        /// <param name="data"></param>
        public Task DistribueAsync(ReadOnlySequence<byte> data);
    }
}
