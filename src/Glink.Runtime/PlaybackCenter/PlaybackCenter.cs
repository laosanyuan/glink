using Glink.Runtime.Application.Contracts.CalculationManager;
using Glink.Runtime.Application.Contracts.Pipline;
using Glink.Runtime.Application.Contracts.PlaybackCenter;
using Microsoft.Extensions.Logging;

namespace Glink.Runtime.PlaybackCenter
{
    /// <summary>
    /// 推送中心
    /// </summary>
    public class PlaybackCenter : IPlaybackCenter
    {
        private readonly ICalculationPiplineManager piplineManager;
        private readonly ILogger<PlaybackCenter> logger;

        public PlaybackCenter(
            ICalculationPiplineManager piplineManager,
            ILogger<PlaybackCenter> logger)
        {
            this.piplineManager = piplineManager;
            this.logger = logger;
        }

        /// <summary>
        /// 开始推送
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="speed"></param>
        public void Start(
            IList<MetaDataConsumerInfo> infos,
            Dictionary<string, Func<byte[], MetaDataSerializerInfo>> serializerHandlerPairs,
            Dictionary<string, Action<byte[]>> pushHandlerPairs)
        {
            var piplines = piplineManager.GetAll();
            foreach (var info in infos)
            {
                var pipline = piplines.FirstOrDefault(t => t.Id == info.PiplineId);
                if (pipline == null)
                {
                    logger.LogInformation("{1} is not running", info.PiplineId);
                    continue;
                }

                if (serializerHandlerPairs == null
                    || !serializerHandlerPairs.TryGetValue(info.PiplineId, out var serializerHandler))
                {
                    serializerHandler = data =>
                   {
                       var dataStr = System.Text.Encoding.Default.GetString(data);
                       var value = dataStr.Split(",");
                       var time = DateTime.ParseExact(value[1], "HHmmssfff", System.Globalization.CultureInfo.CurrentCulture);
                       return new MetaDataSerializerInfo { Time = time };
                   };
                }
                if (pushHandlerPairs == null
                    || !pushHandlerPairs.TryGetValue(info.PiplineId, out var pushHandler))
                {
                    pushHandler = data => Push(info.ReceiverType, data);
                }
                var setting = new CalculationPiplineSetting { Speed = info.Speed.Value, StartTime = info.StartTime, EndTime  =info.EndTime };
                pipline.StartPush(serializerHandler, pushHandler, setting);
                piplineManager.Sync(new CalculationPiplineInfo
                {
                    Id = info.PiplineId,
                    Status = CalculationPiplineStatus.Pushing
                });
            }
        }

        /// <summary>
        /// 结束推送
        /// </summary>
        public void Stop(IList<MetaDataConsumerInfo> infos)
        {
            var piplines = piplineManager.GetAll();
            foreach (var info in infos)
            {
                var pipline = piplines.FirstOrDefault(t => t.Id == info.PiplineId);
                if (pipline == null)
                {
                    logger.LogInformation("{1} is not running", info.PiplineId);
                    continue;
                }

                pipline.StopPush();
                piplineManager.Sync(new CalculationPiplineInfo
                {
                    Id = info.PiplineId,
                    Status = CalculationPiplineStatus.PushStoped
                });
            }
        }

        /// <summary>
        /// 推送数据
        /// </summary>
        /// <param name="types"></param>
        /// <param name="data"></param>
        private void Push(IList<MetaDataReceiverType> types, byte[] data)
        {
            foreach (var type in types)
            {
                switch (type)
                {
                    case MetaDataReceiverType.Console:
                        var dataStr = System.Text.Encoding.Default.GetString(data);
                        Console.WriteLine(dataStr);
                        break;
                }
            }
        }
    }
}
