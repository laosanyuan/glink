using Glink.Runtime.Application.Contracts.Pipline;

namespace Glink.Runtime.Application.Contracts.CalculationManager
{
    /// <summary>
    /// 算子管道管理
    /// </summary>
    public interface ICalculationPiplineManager
    {
        /// <summary>
        /// 查询所有
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ICalculationPipline> GetAll();

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public Task Add(CalculationPiplineInfo info);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="info"></param>
        public void Remove(CalculationPiplineInfo info);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="info"></param>
        public Task Update(CalculationPiplineInfo info);

        /// <summary>
        /// 同步状态
        /// </summary>
        public void Sync(CalculationPiplineInfo info);
    }
}
