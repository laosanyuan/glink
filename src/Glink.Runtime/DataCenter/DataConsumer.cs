using Glink.Runtime.Application.Contracts.DataCenter;
using Microsoft.Extensions.Logging;
using System.Buffers;

namespace Glink.Runtime.DataCenter.MetaData
{
    public class DataConsumer<T> : IDataConsumer<T> where T : IDataCenter<byte[]>
    {
        private readonly T dataCenter;
        private readonly ILogger<DataConsumer<T>> logger;

        public DataConsumer(
            T dataCenter,
            ILogger<DataConsumer<T>> logger)
        {
            this.dataCenter = dataCenter;
            this.logger = logger;
        }

        public void Consume(Action<ReadOnlySequence<byte>> handler, CancellationToken stoppingToken, int concurrency = 1)
        {
            var factory = new TaskFactory(stoppingToken);
            for (var index = 0; index < concurrency; index++)
            {
                var task = factory.StartNew(async () =>
                {
                    while (await dataCenter.WaitToReadAsync(stoppingToken))
                    {
                        var result = dataCenter.TryRead();
                        if (result.Item1)
                        {
                            try
                            {
                                var sequence = new ReadOnlySequence<byte>(result.Item2);
                                handler?.Invoke(sequence);
                            }
                            catch (Exception ex)
                            {
                                logger?.LogError(ex, nameof(Consume));
                            }
                        }
                    }
                });
            }
        }
    }
}
