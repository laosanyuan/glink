using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Glink.Demo.Sdk.Grpc
{
    public class DemoRpcServerService : Greeter.GreeterBase
    {
        private readonly IViewModelLocator viewModelLocator;

        public DemoRpcServerService(IViewModelLocator viewModelLocator)
        {
            this.viewModelLocator = viewModelLocator;
        }

        public override async Task<HelloReply> SayHello(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken))
            {
                try
                {
                    var id = request.Id;
                    var data = request.Data.ToByteArray();
                    viewModelLocator.UpdateData((id, data));                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return new HelloReply();
        }
    }
}
