using Glink.Runtime.Application.Contracts.PlaybackCenter;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Glink.WebApi.Controllers
{
    /// <summary>
    /// 元数据 接口
    /// </summary>
    ///[ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("[controller]/[action]")]
    public class MetaDataController : Controller
    {
        private readonly IPlaybackCenter service;
        private readonly ILogger<MetaDataController> logger;

        public MetaDataController(
            IPlaybackCenter service,
            ILogger<MetaDataController> logger)
        {
            this.service = service;
            this.logger = logger;
        }

        /// <summary>
        /// 开始回放
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="speed"></param>
        [HttpPost]
        public void Start([FromBody][Required] IList<MetaDataConsumerInfo> infos)
        {
            try
            {                
                service.Start(infos, null, null);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(MetaDataController.Start));
            }
        }

        /// <summary>
        /// 结束回放
        /// </summary>
        [HttpPost]
        public void Stop([FromBody][Required] IList<MetaDataConsumerInfo> infos)
        {
            try
            {
                service.Stop(infos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(MetaDataController.Stop));
            }
        }
    }
}
