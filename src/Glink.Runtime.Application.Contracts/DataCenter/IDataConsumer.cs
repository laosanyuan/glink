using System.Buffers;

namespace Glink.Runtime.Application.Contracts.DataCenter
{
    /// <summary>
    /// 数据消费者
    /// </summary>
    /// <typeparam name="T">数据中心类型</typeparam>
    public interface IDataConsumer<T>
    {
        /// <summary>
        /// 开始消费
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="stoppingToken"></param>
        /// <param name="concurrency">并发度, 默认：1</param>
        public void Consume(Action<ReadOnlySequence<byte>> handler, CancellationToken stoppingToken, int concurrency = 1);
    }
}
