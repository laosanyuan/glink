using Glink.Runtime.Application.Contracts;
using Glink.Runtime.Application.Contracts.MessageCenter;
using System.Collections.Concurrent;

namespace Glink.Runtime.MessageCenter
{
    /// <summary>
    /// 消息中心
    /// </summary>
    public class MessageCenter<T> : IMessageCenter<T> where T : IMessageDistributor
    {
        /// <summary>
        /// 订阅-发布关系
        /// key 发布者标识
        /// value 消息订阅者标识集合
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, T>> pubSubRelations;

        public MessageCenter()
        {
            pubSubRelations = new ConcurrentDictionary<string, ConcurrentDictionary<string, T>>();
        }

        /// <summary>
        /// 添加订阅关系
        /// 注意sub 去重
        /// </summary>
        /// <param name="pubId"></param>
        /// <param name="sub"></param>
        public void Add(string pubId, T sub)
        {
            pubSubRelations.AddOrUpdate(pubId,
                pubId =>
                {
                    var bag = new ConcurrentDictionary<string, T>();
                    bag.TryAdd(sub.Id, sub);

                    return bag;
                },
                (pubId, bag) =>
                {
                    bag.TryAdd(sub.Id, sub);
                    return bag;
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pubId"></param>
        /// <returns></returns>
        public IEnumerable<T> Get(string pubId)
        {
            if (pubSubRelations.TryGetValue(pubId, out var bag))
            {
                return bag.Values.ToList();
            }

            return new List<T>();
        }

        public void Remove(string pubId, T sub)
        {
            pubSubRelations.AddOrUpdate(pubId,
                pubId =>
                {
                    var bag = new ConcurrentDictionary<string, T>();
                    return bag;
                },
                (pubId, bag) =>
                {
                    bag.TryRemove(sub.Id, out _);
                    return bag;
                });
        }
    }
}
