using System.Threading;
using System.Threading.Tasks;

namespace HybridToolkit.HierachicalStateMachine {
    /// <summary>
    /// 序列接口
    /// 定义了序列的基本操作
    /// </summary>
    public interface ISequence {
        /// <summary>
        /// 获取序列是否完成
        /// </summary>
        bool IsDone { get; }
        
        /// <summary>
        /// 开始序列
        /// </summary>
        void Start();
        
        /// <summary>
        /// 更新序列状态
        /// </summary>
        /// <returns>序列是否已完成</returns>
        bool Update();
    }
    
    /// <summary>
    /// 阶段步骤委托
    /// 表示一个活动操作（激活或停用）
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>异步任务</returns>
    public delegate Task PhaseStep(CancellationToken ct);

    /// <summary>
    /// 空操作阶段
    /// 实现了ISequence接口，是一个立即完成的空操作序列
    /// </summary>
    public class NoopPhase : ISequence {
        /// <summary>
        /// 序列是否完成
        /// </summary>
        public bool IsDone { get; private set; }
        
        /// <summary>
        /// 开始序列
        /// 立即将IsDone设置为true，表示序列已完成
        /// </summary>
        public void Start() => IsDone = true; // 立即完成
        
        /// <summary>
        /// 更新序列状态
        /// </summary>
        /// <returns>序列是否已完成</returns>
        public bool Update() => IsDone;
    }
}