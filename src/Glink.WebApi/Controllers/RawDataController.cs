using Glink.Runtime.Application.Contracts.DataCenter;
using Glink.Runtime.DataCenter;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Glink.WebApi.Controllers
{
    /// <summary>
    /// 原始数据 相关接口
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class RawDataController : Controller
    {
        private readonly IDataProducer<RawDataCenter> rawDataProducer;

        public RawDataController(IDataProducer<RawDataCenter> rawDataProducer)
        {
            this.rawDataProducer = rawDataProducer;
        }

        /// <summary>
        /// 推送 历史数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task PushHistoricalData([Required] string filePath)
        {
            using (var sr = new StreamReader(filePath))
            {
                string line;
                sr.ReadLine();
                while ((line = sr.ReadLine()!) != null)
                {
                    var data = System.Text.Encoding.Default.GetBytes(line);
                    Thread.Sleep(PushSetting.DelayMilliSeconds);
                    await rawDataProducer.Produce(data);
                }
                return;
            }
        }

        /// <summary>
        /// 推送 实时数据
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task PushRealtimeData([FromBody] IList<string> datas)
        {
            foreach (var item in datas)
            {
                var data = System.Text.Encoding.Default.GetBytes(item);
                await rawDataProducer.Produce(data);
            }
        }
    }

    public static class PushSetting
    {
        public static int DelayMilliSeconds = 10;
    }
}
