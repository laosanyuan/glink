using Glink.Component.Abstractions;
using Glink.Runtime.Application.Contracts;
using Glink.Runtime.Application.Contracts.CalculationManager;
using Glink.Runtime.Application.Contracts.MessageCenter;
using Glink.Runtime.Application.Contracts.Pipline;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime.Loader;

namespace Glink.Runtime.CalculationManager
{
    public class CalculationPiplineManager : ICalculationPiplineManager
    {
        private readonly ConcurrentDictionary<string, CalculationPipline> piplinePool;
        private readonly ConcurrentDictionary<string, CalculationContext> contextPool;
        private readonly ILogger<CalculationPiplineManager> logger;
        private readonly IMessageSubscriber<ICalculationPipline> subscriber;
        private readonly IMessagePublisher<ICalculationPipline> publisher;
        private readonly ILogger<CalculationPipline> piplineLogger;

        public CalculationPiplineManager(
            ILogger<CalculationPiplineManager> logger,
            IMessageSubscriber<ICalculationPipline> subscriber,
            IMessagePublisher<ICalculationPipline> publisher,
            ILogger<CalculationPipline> piplineLogger)
        {
            piplinePool = new ConcurrentDictionary<string, CalculationPipline>();
            contextPool = new ConcurrentDictionary<string, CalculationContext>();
            this.logger = logger;
            this.subscriber = subscriber;
            this.publisher = publisher;
            this.piplineLogger = piplineLogger;
        }

        public async Task Add(CalculationPiplineInfo info)
        {
            try
            {
                if (piplinePool.Any(t => t.Key == info.Id))
                {
                    logger.LogInformation("Id：{1} is existed", info.Id);
                    return;
                }

                var bytes = await File.ReadAllBytesAsync(info.Path);
                var stream = new MemoryStream(bytes);
                var context = contextPool.AddOrUpdate(info.Id, id => new CalculationContext(), (id, context) => context);
                var assembly = context.LoadFromStream(stream);
                var instance = assembly.CreateInstance(info.ClassName);

                if (instance is not ICalculationEngine)
                {
                    logger.LogInformation("{path} is not ICalculationEngine", info.Path);
                    return;
                }

                var engine = instance as ICalculationEngine;
                var id = engine?.Id;
                if (id != Convert.ToInt64(info.Id))
                {
                    logger.LogInformation("info.Id：{1} is equal to CalculationEngine.Id：{2}", info.Id, id);
                    return;
                }
                var pipline = new CalculationPipline(engine, subscriber, publisher, piplineLogger);
                piplinePool.AddOrUpdate(info.Id, id => pipline, (id, pip) => pipline);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(Add));
            }
        }

        public IEnumerable<ICalculationPipline> GetAll()
        {
            return piplinePool.Values;
        }

        public void Remove(CalculationPiplineInfo info)
        {
            var piplinePair = piplinePool.Where(t => t.Key == info.Id).FirstOrDefault();
            if (piplinePair.Value == default)
            {
                return;
            }
            piplinePool.TryRemove(piplinePair);
            piplinePair.Value.Stop();
            piplinePair.Value.StopPush();

            var contextPair = contextPool.Where(t => t.Key == info.Id).FirstOrDefault();
            if (contextPair.Value == default)
            {
                return;
            }
            contextPool.TryRemove(contextPair);
            contextPair.Value.Unload();
        }

        public void Sync(CalculationPiplineInfo info)
        {
            var piplinePair = piplinePool.Where(t => t.Key == info.Id).FirstOrDefault();
            if (piplinePair.Value == default)
            {
                return;
            }

            piplinePair.Value.Status = info.Status;
        }

        public async Task Update(CalculationPiplineInfo info)
        {
            Remove(info);
            await Add(info);
        }
    }

    /// <summary>
    /// 算子 隔离域
    /// </summary>
    public class CalculationContext : AssemblyLoadContext
    {
        public CalculationContext() : base(true)
        {

        }
    }
}
