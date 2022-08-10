using Glink.Runtime.Application.Contracts.DataCenter;
using System.Threading.Channels;

namespace Glink.Runtime.DataCenter
{
    /// <summary>
    /// 元数据中心
    /// 多生产者--多消费者,顺序消费
    /// </summary>
    public class MetaDataCenter : IDataCenter<byte[]>
    {
        private readonly Channel<byte[]> metaDataPool;

        public MetaDataCenter()
        {
            metaDataPool = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true
            });
        }

        public ValueTask WriteAsync(byte[] item, CancellationToken cancellationToken = default)
        {
            if (item == null || !item.Any())
            {
                return new ValueTask(Task.CompletedTask);
            }
            return metaDataPool.Writer.WriteAsync(item);
        }

        public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
        {
            return metaDataPool.Reader.WaitToReadAsync(cancellationToken);
        }

        public (bool, byte[]) TryRead()
        {
            var result = metaDataPool.Reader.TryRead(out var item);
            return (result, item);
        }
    }
}
