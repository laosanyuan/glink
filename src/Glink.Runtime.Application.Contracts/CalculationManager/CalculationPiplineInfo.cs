using Glink.Runtime.Application.Contracts.Pipline;
using System.ComponentModel.DataAnnotations;

namespace Glink.Runtime.Application.Contracts.CalculationManager
{
    /// <summary>
    /// 算子管道信息
    /// </summary>
    public class CalculationPiplineInfo : CalculationPiplineSetting
    {
        /// <summary>
        /// 标识
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// 算子库地址
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 类名称, 用于创建算子引擎实例
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 算子类型
        /// </summary>
        public long Type { get; set; }

        /// <summary>
        /// 算子名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 版本号
        /// 默认：1.0.0
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 是否允许推送给业务方
        /// </summary>
        public bool AllowPush { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public CalculationPiplineStatus Status { get; set; }
    }
}
