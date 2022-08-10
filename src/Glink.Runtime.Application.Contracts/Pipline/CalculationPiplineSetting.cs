namespace Glink.Runtime.Application.Contracts.Pipline
{
    /// <summary>
    /// 计算管道 设置
    /// </summary>
    public class CalculationPiplineSetting
    {
        /// <summary>
        /// 回放开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 回放结束时间
        /// </summary>
        public DateTime? EndTime { get; set;}

        /// <summary>
        /// 速率
        /// </summary>
        public int Speed { get; set; }
    }
}
