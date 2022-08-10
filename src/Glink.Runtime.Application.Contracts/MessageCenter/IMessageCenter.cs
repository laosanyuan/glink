namespace Glink.Runtime.Application.Contracts
{
    /// <summary>
    /// 消息中心
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMessageCenter<T>
    {
        /// <summary>
        /// 添加订阅关系
        /// 注意sub 去重
        /// </summary>
        /// <param name="pubId"></param>
        /// <param name="sub"></param>
        public void Add(string pubId, T sub);

        /// <summary>
        /// 获取订阅关系
        /// </summary>
        /// <param name="pubId"></param>
        /// <returns></returns>
        public IEnumerable<T> Get(string pubId);

        /// <summary>
        /// 删除订阅关系
        /// </summary>
        /// <param name="pubId"></param>
        /// <param name="sub"></param>
        public void Remove(string pubId, T sub);
    }
}
