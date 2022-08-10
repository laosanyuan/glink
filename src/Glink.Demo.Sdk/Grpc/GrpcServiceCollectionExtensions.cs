using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Glink.Demo.Sdk.Grpc
{
    /// <summary>
    /// Grpc 
    /// </summary>
    public static class GrpcServiceCollectionExtensions
    {
        public static IServiceCollection AddGrpcServer(this IServiceCollection services, string address)
        {
            services.AddGrpc();
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            ConfigGrpcKestrel(services, address);
            return services;
        }

        private static void ConfigGrpcKestrel(IServiceCollection services, string address)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                var uri = new Uri(address);
                options.ListenAnyIP(uri.Port, o => o.Protocols =
                        HttpProtocols.Http2);
            });
        }
    }
}
