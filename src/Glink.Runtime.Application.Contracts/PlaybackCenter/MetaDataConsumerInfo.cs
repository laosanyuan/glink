using System.ComponentModel.DataAnnotations;

namespace Glink.Runtime.Application.Contracts.PlaybackCenter
{
    /// <summary>
    /// 元数据消费 信息
    /// </summary>
    public class MetaDataConsumerInfo
    {
        /// <summary>
        /// 数据类型标识, 必填
        /// </summary>
        [Required]
        public string PiplineId { get; set; }

        /// <summary>
        /// 开始时间, 支持多时区
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间, 支持多时区
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 市场
        /// </summary>
        public string Market { get; set; }

        /// <summary>
        /// 推送速率
        /// </summary>
        public int? Speed { get; set; } = 1;

        /// <summary>
        /// 数据接收方类型
        /// </summary>
        public IList<MetaDataReceiverType> ReceiverType { get; set; }
    }

    /// <summary>
    /// 支持的业务类型
    /// </summary>
    public enum MetaDataReceiverType
    {
        /// <summary>
        /// 输出
        /// </summary>
        Console = 0
    }
}
