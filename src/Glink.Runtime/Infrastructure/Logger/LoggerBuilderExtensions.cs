using Microsoft.Extensions.Hosting;
using Serilog;

namespace Glink.Runtime.Infrastructure.Logger
{
    /// <summary>
    /// 基础设施--日志
    /// </summary>
    public static class LoggerBuilderExtensions
    {
        /// <summary>
        /// 从配置文件加载日志
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IHostBuilder UseLog(
            this IHostBuilder app)
        {
            app.UseSerilog((hostContext, loggerConfiguration) =>
            {
                var logConfig = loggerConfiguration.ReadFrom.Configuration(hostContext.Configuration);
            });

            return app;
        }
    }
}
