using Glink.Component.Abstractions;
using Glink.Runtime.Application.Contracts.MessageCenter;
using Glink.Runtime.Application.Contracts.Pipline;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Glink.Runtime.Application.Contracts
{
    /// <summary>
    /// 计算管道
    /// </summary>
    public class CalculationPipline : ICalculationPipline
    {
        private readonly ICalculationEngine engine;
        private readonly IMessageSubscriber<ICalculationPipline> subscriber;
        private readonly IMessagePublisher<ICalculationPipline> publisher;
        private readonly ILogger<CalculationPipline> logger;
        private readonly ConcurrentQueue<ReadOnlySequence<byte>> buffer = new ConcurrentQueue<ReadOnlySequence<byte>>();
        private readonly Channel<byte[]> metaDataPool = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
        {
            SingleReader = true
        });

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource pushTokenSource = new CancellationTokenSource();

        /// <summary>
        /// 标识
        /// </summary>
        public string Id => engine.Id.ToString();

        /// <summary>
        /// 类型
        /// </summary>
        public long Type => engine.Type;

        /// <summary>
        /// 是否接收原始数据
        /// </summary>
        public bool ReceivedRawData => engine.ReceivedRawData;

        /// <summary>
        /// 是否允许推送给业务方
        /// </summary>
        public bool AllowPush => engine.AllowPush;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => engine.Name;

        /// <summary>
        /// 是否开始消费
        /// </summary>
        public bool IsStarted { get; private set; } = false;

        /// <summary>
        /// 运行状态
        /// </summary>
        public CalculationPiplineStatus Status { get; set; } = CalculationPiplineStatus.Runing;
        
        /// <summary>
        /// 设置
        /// </summary>
        public CalculationPiplineSetting Setting { get; } = new CalculationPiplineSetting { Speed = 1};

        /// <summary>
        /// 
        /// </summary>
        public CalculationPipline(
            ICalculationEngine engine,
            IMessageSubscriber<ICalculationPipline> subscriber,
            IMessagePublisher<ICalculationPipline> publisher,
            ILogger<CalculationPipline> logger)
        {
            this.engine = engine;
            this.subscriber = subscriber;
            this.publisher = publisher;
            this.logger = logger;

            ValidateCalculationEngine(engine);
            Start();
        }

        /// <summary>
        /// 校验 算子引擎是否合法
        /// </summary>
        /// <param name="engine"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private void ValidateCalculationEngine(ICalculationEngine engine)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(ICalculationEngine));
            }

            if (engine.Id == 0)
            {
                throw new ArgumentException(nameof(ICalculationEngine));
            }
        }

        /// <summary>
        /// 订阅
        /// </summary>
        private void Subscribe()
        {
            if (engine == null)
            {
                return;
            }

            if (engine.Topic != null && engine.Topic.Any())
            {
                var pubIds = engine.Topic.Select(x => x.ToString());
                subscriber.Subscribe(pubIds, this);
            }
        }

        /// <summary>
        /// 开始消费数据 （消费模式）
        /// </summary>
        public void Start()
        {
            if (IsStarted)
            {
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();
            Subscribe();
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        if (!buffer.TryDequeue(out var data))
                        {
                            continue;
                        }
                        if (engine == null || data.IsEmpty)
                        {
                            continue;
                        }

                        var result = engine.Execute(data);
                        if (result == null
                            || result.Length == 0)
                        {
                            continue;
                        }

                        if (engine.AllowPublish)
                        {
                            publisher.Publish(engine.Id.ToString(), result);
                        }

                        if (engine.AllowPush)
                        {
                            metaDataPool.Writer.TryWrite(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, default);
                    }
                }
            }, cancellationTokenSource.Token);
            IsStarted = true;
        }

        /// <summary>
        /// 停止消费数据
        /// </summary>
        public void Stop()
        {
            cancellationTokenSource?.Cancel();

            if (engine.Topic != null && engine.Topic.Any())
            {
                var pubIds = engine.Topic.Select(x => x.ToString());
                subscriber?.UnSubscribe(pubIds, this);
            }

            IsStarted = false;
        }

        /// <summary>
        /// 接收数据(生产模式)
        /// </summary>
        private void Product(ReadOnlySequence<byte> data)
        {
            if (data.IsEmpty)
            {
                return;
            }

            var filter = engine.DataFilter(data);
            if (!filter)
            {
                return;
            }

            buffer.Enqueue(data);
        }

        /// <summary>
        /// 分发数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task DistribueAsync(ReadOnlySequence<byte> data)
        {
            Product(data);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 推送给业务方
        /// </summary>
        /// <param name="serilizer">序列化处理器</param>
        /// <param name="handler">推送处理器</param>
        /// <param name="setting"></param>
        public void StartPush(Func<byte[], MetaDataSerializerInfo> serilizer, Action<byte[]> handler, CalculationPiplineSetting setting)
        {
            StopPush();
            Update(setting);
            pushTokenSource = new CancellationTokenSource();

            var preDataTime = Setting.StartTime;
            Task.Factory.StartNew(async () =>
            {
                while (await metaDataPool.Reader.WaitToReadAsync(pushTokenSource.Token))
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        return;
                    }

                    if (metaDataPool.Reader.TryRead(out var data))
                    {
                        try
                        {
                            var value = serilizer?.Invoke(data);
                            if(value?.Time == null)
                            {
                                handler(data);
                                continue;
                            }

                            if (Setting.StartTime.HasValue)
                            {
                                if (value.Time < Setting.StartTime.Value)
                                {
                                    continue;
                                }
                            }

                            if (Setting.EndTime.HasValue)
                            {
                                if (value.Time > Setting.EndTime.Value)
                                {
                                    continue;
                                }
                            }

                            if (preDataTime.HasValue && value.Time < preDataTime)
                            {
                                continue;
                            }

                            if (preDataTime.HasValue)
                            {
                                var timeInterval = (int)((value.Time.Value - preDataTime.Value).TotalMilliseconds / Setting.Speed);
                                if (timeInterval > 0)
                                {
                                    Thread.Sleep(timeInterval);
                                }
                            }

                            preDataTime = value.Time;
                            handler(data);
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError(ex, nameof(Start));
                        }
                    }
                }
            });
        }

        public void StopPush()
        {
            pushTokenSource?.Cancel();
        }

        public void Update(CalculationPiplineSetting setting)
        {
            if(setting == null)
            {
                return;
            }

            Setting.StartTime = setting.StartTime;
            Setting.EndTime = setting.EndTime;
            Setting.Speed = setting.Speed;
        }
    }
}
