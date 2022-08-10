using Glink.Runtime.Application.Contracts.CalculationManager;
using Glink.Runtime.Application.Contracts.DataCenter;
using Glink.Runtime.DataCenter;
using System.Buffers;

namespace Glink.WebApi.BackgroundServices
{
    /// <summary>
    /// 原始数据消费 后台服务
    /// </summary>
    public class RawDataConsumerBackgroundService : BackgroundService
    {
        private readonly ICalculationPiplineManager calculationPiplineManager;
        private readonly IDataConsumer<RawDataCenter> dataConsumer;
        private readonly ILogger<RawDataConsumerBackgroundService> logger;

        public RawDataConsumerBackgroundService(
            ICalculationPiplineManager calculationPiplineManager,
            IDataConsumer<RawDataCenter> dataConsumer,
            ILogger<RawDataConsumerBackgroundService> logger)
        {
            this.calculationPiplineManager = calculationPiplineManager;
            this.dataConsumer = dataConsumer;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Action<ReadOnlySequence<byte>> handler = async data => await Distributor(data);
                await Task.Factory.StartNew(() => dataConsumer.Consume(handler, stoppingToken));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(ExecuteAsync));
            }
        }

        private async Task Distributor(ReadOnlySequence<byte> data)
        {
            var piplines = calculationPiplineManager.GetAll().Where(t => t.ReceivedRawData);
            foreach (var pipline in piplines)
            {
                await pipline.DistribueAsync(data);
            }
        }
    }
}
