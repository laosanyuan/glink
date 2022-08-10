using Glink.Runtime.Application.Contracts.DataCenter;
using System.Threading.Channels;

namespace Glink.Runtime.DataCenter
{
    /// <summary>
    /// 原始数据中心
    /// 多个生产者--一个消费者
    /// </summary>
    public class RawDataCenter : IDataCenter<byte[]>
    {
        private readonly Channel<byte[]> rawDataPool;

        public RawDataCenter()
        {
            rawDataPool = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
            {
                SingleReader = true
            });
        }

        public ValueTask WriteAsync(byte[] item, CancellationToken cancellationToken = default)
        {
            if (item == null || !item.Any())
            {
                return new ValueTask(Task.CompletedTask);
            }
            return rawDataPool.Writer.WriteAsync(item);
        }

        public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
        {
            return rawDataPool.Reader.WaitToReadAsync(cancellationToken);
        }

        public (bool, byte[]) TryRead()
        {
            var result = rawDataPool.Reader.TryRead(out var item);
            return (result, item);
        }
    }
}
