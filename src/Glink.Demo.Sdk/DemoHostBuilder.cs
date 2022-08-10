using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Glink.Demo.Sdk
{
    public static class DemoHostBuilder
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
