using Glink.Runtime.Application.Contracts.Pipline;

namespace Glink.Runtime.Application.Contracts.PlaybackCenter
{
    /// <summary>
    /// 推送中心(元数据消费中心)
    /// </summary>
    public interface IPlaybackCenter
    {
        /// <summary>
        /// 开始倍速推送
        /// </summary>
        /// <param name="infos"></param>
        public void Start(
            IList<MetaDataConsumerInfo> infos,
            Dictionary<string, Func<byte[], MetaDataSerializerInfo>> serilizerHandlerPairs,
            Dictionary<string, Action<byte[]>> pushHandlerPairs);

        /// <summary>
        /// 结束倍速推送
        /// </summary>
        /// <param name="infos"></param>
        public void Stop(IList<MetaDataConsumerInfo> infos);
    }
}
