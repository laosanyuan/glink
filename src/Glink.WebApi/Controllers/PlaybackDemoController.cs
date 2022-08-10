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
    /// 回放中心 演示相关
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class PlaybackDemoController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ICalculationPiplineManager manager;
        private readonly IDataProducer<RawDataCenter> rawDataProducer;
        private readonly IPlaybackCenter service;
        private readonly ILogger<PlaybackDemoController> logger;
        private readonly Channel<(string, byte[])> metaDataPushChannel;

        public PlaybackDemoController(
            IConfiguration configuration,
            ICalculationPiplineManager manager,
            IDataProducer<RawDataCenter> rawDataProducer,
            IPlaybackCenter service,
            ILogger<PlaybackDemoController> logger,
            Channel<(string, byte[])> metaDataPushChannel)
        {
            this.configuration = configuration;
            this.manager = manager;
            this.rawDataProducer = rawDataProducer;
            this.service = service;
            this.logger = logger;
            this.metaDataPushChannel = metaDataPushChannel;
        }

        /// <summary>
        /// 一键演示
        /// </summary>
        [HttpGet]
        public async Task Auto()
        {
            await Init();
            Thread.Sleep(5000);

            await StartMA5();
            await StartSencondKline();

            PushData();
            Console.WriteLine("开始推送数据");
            Thread.Sleep(10000);

            await Update();
        }

        /// <summary>
        /// 启动演示程序
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task Init()
        {
            Console.WriteLine("启动演示程序");
            var id = "1";
            await AddCalculationPipline(id);
            Console.WriteLine("加载MA5 通道");

            await AddCalculationPipline("3");
            Console.WriteLine("加载MA5 通道");
        }

        /// <summary>
        /// 开始回放 --- MA5   默认：不限制时间范围、1倍速
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task StartMA5()
        {
            var id = "1";
            await StartPush(id, 5);
            Console.WriteLine("开始1倍速回放MA5！");
        }

        /// <summary>
        /// 开始回放 --- 秒K线   默认：不限制时间范围、1倍速
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task StartSencondKline()
        {
            var id = "3";
            await StartPush(id, 0);
            Console.WriteLine("开始1倍速回放秒K！");
        }

        /// <summary>
        /// 动态倍速回放
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="speed"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task Update(string id = "1", string startTime = "2022-08-06 09:30:00", string endTime = "2022-08-06 10:00:00", int speed = 10)
        {
            PushData();
            var pipline = manager.GetAll().Where(t => t.Id == id).First();

            pipline.Update(new CalculationPiplineSetting { StartTime = Convert.ToDateTime(startTime), EndTime = Convert.ToDateTime(endTime), Speed = speed });
            Console.WriteLine($"开始{speed}倍速回放, 数据管道类型：{id}！");
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
                    Status = t.Status,
                    StartTime = t.Setting.StartTime,
                    EndTime = t.Setting.EndTime,
                    Speed = t.Setting.Speed
                }).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(GetAll));
            }

            return new List<CalculationPiplineInfo>();
        }

        /// <summary>
        /// 暂停回放
        /// </summary>
        /// <returns></returns>
        /// <param name="piplineId"></param>
        [HttpGet]
        public void Stop(string piplineId = "1")
        {
            service.Stop(new List<MetaDataConsumerInfo>
            {
                new MetaDataConsumerInfo
                {
                    PiplineId = piplineId,
                }
            });
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
        /// 开始推送
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startIndex"></param>
        private async Task StartPush(string id, int startIndex)
        {
            var serilizerHandlerPairs = new Dictionary<string, Func<byte[], MetaDataSerializerInfo>> { { id,
                    data => new MetaDataSerializerInfo { Time =  ConvertTime(data, startIndex) } } };
            var pushHandlerPairs = new Dictionary<string, Action<byte[]>> { {id,
                    async data =>
                    {
                        await metaDataPushChannel.Writer.WriteAsync((id, data));
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

        /// <summary>
        /// 时间转换
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private DateTime ConvertTime(byte[] data, int startIndex)
        {
            var timeInt = BitConverter.ToInt32(data, startIndex);

            //var time = DateTime.ParseExact(timeStr, "HHmmssfff", System.Globalization.CultureInfo.CurrentCulture);
            // 09_30_00_000
            var result = new DateTime(
                2022,
                8,
                6,
                timeInt / 1_00_00_000,
                timeInt % 1_00_00_000 / 1_00_000,
                timeInt % 1_00_000 / 1000);

            return result;
        }
    }
}
