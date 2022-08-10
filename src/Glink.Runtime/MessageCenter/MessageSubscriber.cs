using Glink.Runtime.Application.Contracts;
using Glink.Runtime.Application.Contracts.MessageCenter;

namespace Glink.Runtime.MessageCenter
{
    public class MessageSubscriber<T> : IMessageSubscriber<T> where T : IMessageDistributor 
    {
        private readonly IMessageCenter<T> messageCenter;

        public MessageSubscriber(IMessageCenter<T> messageCenter)
        {
            this.messageCenter = messageCenter;
        }

        public void Subscribe(IEnumerable<string> pubIds, T sub)
        {
            foreach (var id in pubIds)
            {
                messageCenter.Add(id, sub);
            }
        }

        public void UnSubscribe(IEnumerable<string> pubIds, T sub)
        {
            foreach (var id in pubIds)
            {
                messageCenter.Remove(id, sub);
            }
        }
    }
}
