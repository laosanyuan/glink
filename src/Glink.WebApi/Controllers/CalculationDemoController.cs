using Glink.Demo.Sdk.Grpc;
using Glink.Runtime.Application.Contracts.CalculationManager;
using Glink.Runtime.Application.Contracts.DataCenter;
using Glink.Runtime.Application.Contracts.Pipline;
using Glink.Runtime.Application.Contracts.PlaybackCenter;
using Glink.Runtime.DataCenter;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace Glink.WebApi.Controllers
{
    /// <summary>
    /// 算子演示 相关
    /// </summary>
    //[ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("[controller]/[action]")]
    public class CalculationDemoController : ControllerBase
    {
        private readonly IPlaybackCenter service;
        private readonly IConfiguration configuration;
        private readonly ICalculationPiplineManager manager;
        private readonly IDataProducer<RawDataCenter> rawDataProducer;
        private readonly Channel<(string, byte[])> metaPushChannel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configuration"></param>
        /// <param name="manager"></param>
        /// <param name="rawDataProducer"></param>
        /// <param name="metaPushChannel"></param>
        public CalculationDemoController(
            IPlaybackCenter service,
            IConfiguration configuration,
            ICalculationPiplineManager manager,
            IDataProducer<RawDataCenter> rawDataProducer,
            Channel<(string, byte[])> metaPushChannel)
        {
            this.service = service;
            this.configuration = configuration;
            this.manager = manager;
            this.rawDataProducer = rawDataProducer;
            this.metaPushChannel = metaPushChannel;
        }

        /// <summary>
        /// 一键演示
        /// </summary>
        [HttpGet]
        public async Task Auto()
        {
            Init();
            
            await AddMa5();
            PushData();
            Thread.Sleep(5000);

            await UpdateMd5();
            PushData();
            Thread.Sleep(4000);

            await AddSecondKline();
            await AddMinuteKline();
            PushData();
        }

        /// <summary>
        /// 初始化环境
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public void Init()
        {
            Console.WriteLine("启动演示程序！");  
        }

        private void PushData()
        {
            _ = Task.Factory.StartNew(async () => 
            {
                var dataPath = configuration["Demo:DataPath"];
                using (var sr = new StreamReader(dataPath))
                {
                    string line;
                    sr.ReadLine();
                    while ((line = sr.ReadLine()!) != null)
                    {
                        var data = System.Text.Encoding.Default.GetBytes(line);
                        await rawDataProducer.Produce(data);
                    }
                    return;
                }
            });        
        }

        /// <summary>
        /// 添加Md5 计算算子
        /// </summary>
        [HttpGet]
        public async Task AddMa5()
        {
            var id = "1";
            await StartPush(id);
            Console.WriteLine("开始计算MA5！");
        }

        /// <summary>
        /// 更新Md5 计算算子
        /// </summary>
        [HttpGet]
        public async Task UpdateMd5()
        {
            var id = "1";
            RemoveCalculationPipline(id);

            id = "5";
            await StartPush(id);
            Console.WriteLine("修改MA5算子！");
        }

        /// <summary>
        /// 添加秒K线 计算算子
        /// </summary>
        [HttpGet]
        public async Task AddSecondKline()
        {
            var id = "3";
            await StartPush(id);
            Console.WriteLine("开始计算秒K线！");
        }

        /// <summary>
        /// 添加分K线 计算算子
        /// </summary>
        [HttpGet]
        public async Task AddMinuteKline()
        {
            var id = "4";
            await StartPush(id);
            Console.WriteLine("开始计算分K线！");
        }

        /// <summary>
        /// 停止
        /// </summary>
        [HttpGet]
        public void Stop()
        {
            var id = "1";
            RemoveCalculationPipline(id);

            id = "5";
            RemoveCalculationPipline(id);

            id = "3";
            RemoveCalculationPipline(id);

            id = "4";
            RemoveCalculationPipline(id);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IList<CalculationPiplineInfo> GetAll()
        {
            return manager.GetAll().Select(t => new CalculationPiplineInfo
            {
                Id = t.Id.ToString(),
                Type = t.Type,
                AllowPush = t.AllowPush,
                Name = t.Name,
                Status = t.Status,
                StartTime = t.Setting.StartTime,
                EndTime = t.Setting.EndTime,
                Speed = t.Setting.Speed
            }).ToList();
        }

        /// <summary>
        /// 开始推送
        /// </summary>
        /// <param name="id"></param>
        private async Task StartPush(string id)
        {
            await AddCalculationPipline(id);

            var serilizerHandlerPairs = new Dictionary<string, Func<byte[], MetaDataSerializerInfo>> { { id, data => new MetaDataSerializerInfo { Time = null } } };
            var pushHandlerPairs = new Dictionary<string, Action<byte[]>> { {id,
                    async data =>
                    {
                        await metaPushChannel.Writer.WriteAsync((id, data));
                    }} };
            service.Start(new List<MetaDataConsumerInfo> { new MetaDataConsumerInfo { PiplineId = id } }, serilizerHandlerPairs, pushHandlerPairs);
        }

        /// <summary>
        /// 添加指定计算管道
        /// </summary>
        /// <param name="id"></param>
        private async Task AddCalculationPipline(string id)
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
            var info = infos.Where(x => x.Id == id).First();
            await manager.Add(info);
        }

        /// <summary>
        /// 移除指定计算管道
        /// </summary>
        /// <param name="id"></param>
        private void RemoveCalculationPipline(string id)
        {
            var info = new CalculationPiplineInfo { Id = id };
            manager.Remove(info);
        }
    }
}
