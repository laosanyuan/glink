namespace Glink.Runtime.Application.Contracts.DataCenter
{
    /// <summary>
    /// 数据中心
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public interface IDataCenter<T>
    {
        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ValueTask WriteAsync(T item, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 阻塞读取
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public (bool, T) TryRead();
    }
}
