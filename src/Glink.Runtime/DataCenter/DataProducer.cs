using Glink.Runtime.Application.Contracts.DataCenter;

namespace Glink.Runtime.DataCenter.MetaData
{
    /// <summary>
    /// 数据生产者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataProducer<T> : IDataProducer<T> where T : IDataCenter<byte[]>
    {
        private readonly T dataCenter;

        public DataProducer(T dataCenter)
        {
            this.dataCenter = dataCenter;
        }

        public async ValueTask Produce(byte[] data)
        {
            await dataCenter.WriteAsync(data);
        }
    }
}
