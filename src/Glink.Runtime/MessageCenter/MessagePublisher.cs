using Glink.Runtime.Application.Contracts;
using Glink.Runtime.Application.Contracts.MessageCenter;
using Microsoft.Extensions.Logging;
using System.Buffers;

namespace Glink.Runtime.MessageCenter
{
    public class MessagePublisher<T> : IMessagePublisher<T> where T : IMessageDistributor
    {
        private readonly IMessageCenter<T> messageCenter;
        private readonly ILogger<MessagePublisher<T>> logger;

        public MessagePublisher(
            IMessageCenter<T> messageCenter,
            ILogger<MessagePublisher<T>> logger)
        {
            this.messageCenter = messageCenter;
            this.logger = logger;
        }

        public Task Publish(string id, byte[] data)
        {
            var subs = messageCenter.Get(id).DistinctBy(t => t.Id);
            var sequence = new ReadOnlySequence<byte>(data);
            foreach(var sub in subs)
            {
                try
                {
                    sub.DistribueAsync(sequence);
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, default);
                }
            }

            return Task.CompletedTask;
        }
    }
}
