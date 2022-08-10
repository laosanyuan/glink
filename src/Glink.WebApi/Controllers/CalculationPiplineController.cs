using Glink.Runtime.Application.Contracts.CalculationManager;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Glink.Controllers
{
    /// <summary>
    /// 算子管道 相关接口
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class CalculationPiplineController : ControllerBase
    {
        private readonly ILogger<CalculationPiplineController> logger;
        private readonly ICalculationPiplineManager manager;
        private readonly IConfiguration configuration;

        public CalculationPiplineController(
            ILogger<CalculationPiplineController> logger,
            ICalculationPiplineManager manager,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.manager = manager;
            this.configuration = configuration;
        }

        /// <summary>
        /// 初始化,加载本地配置的默认动态库
        /// </summary>
        [HttpPost]
        public void Init()
        {
            try
            {
                var section = configuration.GetSection("CalculationInfos:Infos");
                if (section == null)
                {
                    return;
                }

                var infos = section.Get<IList<CalculationPiplineInfo>>();
                if (infos == null)
                {
                    return;
                }
                foreach (var info in infos)
                {
                    manager.Add(info);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(Init));
            }
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="infos"></param>
        [HttpPost]
        public async Task AddBatch([FromBody][Required] IList<CalculationPiplineInfo> infos)
        {
            try
            {
                foreach (var info in infos)
                {
                    await manager.Add(info);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(AddBatch));
            }
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        [HttpPost]
        public void RemoveBatch(IList<CalculationPiplineInfo> infos)
        {
            try
            {
                foreach (var info in infos)
                {
                    manager.Remove(info);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(RemoveBatch));
            }
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="infos"></param>
        [HttpPost]
        public async Task UpdateBatch(IList<CalculationPiplineInfo> infos)
        {
            try
            {
                foreach (var info in infos)
                {
                    await manager.Update(info);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(UpdateBatch));
            }
        }

        /// <summary>
        /// 查询所有
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IList<CalculationPiplineInfo> GetAll()
        {
            try
            {
                return manager.GetAll().Select(t => new CalculationPiplineInfo
                {
                    Id = t.Id.ToString(),
                    Type = t.Type,
                    AllowPush = t.AllowPush,
                    Name = t.Name,
                    Status = t.Status
                }).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(GetAll));
            }

            return new List<CalculationPiplineInfo>();
        }
    }
}