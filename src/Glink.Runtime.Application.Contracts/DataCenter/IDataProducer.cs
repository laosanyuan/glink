namespace Glink.Runtime.Application.Contracts.DataCenter
{
    /// <summary>
    /// 数据生产者
    /// </summary>
    /// <typeparam name="T">数据中心类型</typeparam>
    public interface IDataProducer<T>
    {
        /// <summary>
        /// 生产数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ValueTask Produce(byte[] data);
    }
}
