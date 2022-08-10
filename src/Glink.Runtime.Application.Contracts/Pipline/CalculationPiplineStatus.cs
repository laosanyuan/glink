namespace Glink.Runtime.Application.Contracts.Pipline
{
    /// <summary>
    /// 算子管道 状态
    /// </summary>
    public enum CalculationPiplineStatus
    {
        /// <summary>
        /// 运行中
        /// </summary>
        Runing,

        /// <summary>
        /// 已停止推送
        /// </summary>
        PushStoped,

        /// <summary>
        /// 推送中
        /// </summary>
        Pushing,
    }
}
