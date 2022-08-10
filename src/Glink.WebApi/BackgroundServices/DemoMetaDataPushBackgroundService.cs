using Glink.Demo;
using Google.Protobuf;
using Grpc.Core;
using System.Threading.Channels;

namespace Glink.WebApi.BackgroundServices
{
    /// <summary>
    /// 演示程序-元数据推送后台服务
    /// </summary>
    public class DemoMetaDataPushBackgroundService : BackgroundService
    {
        private readonly Channel<(string, byte[])> metaData;
        private readonly Greeter.GreeterClient client;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="client"></param>
        public DemoMetaDataPushBackgroundService(
            Channel<(string, byte[])> metaData,
            Greeter.GreeterClient client)
        {
            this.metaData = metaData;
            this.client = client;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var caller = await Connect(stoppingToken);
            await foreach (var request in metaData.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await caller.RequestStream.WriteAsync(new HelloRequest
                    {
                        Id = request.Item1,
                        Data = ByteString.CopyFrom(request.Item2)
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
        }

        private async Task<AsyncClientStreamingCall<HelloRequest, HelloReply>> Connect(CancellationToken stoppingToken)
        {
            while(true)
            {
                try
                {
                    var caller = client.SayHello(cancellationToken: stoppingToken);
                    await caller.RequestStream.WriteAsync(new HelloRequest
                    {
                        Id = "-1",
                        Data = ByteString.CopyFrom(Array.Empty<byte>())
                    });
                    Console.WriteLine("Connect Suucess!");
                    return caller;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
