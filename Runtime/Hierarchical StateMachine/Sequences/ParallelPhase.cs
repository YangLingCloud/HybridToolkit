using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HSM {
    /// <summary>
    /// 并行阶段类
    /// 实现了ISequence接口，用于并行执行多个阶段步骤
    /// </summary>
    public class ParallelPhase : ISequence {
        /// <summary>
        /// 阶段步骤列表
        /// </summary>
        readonly List<PhaseStep> steps;
        
        /// <summary>
        /// 取消令牌
        /// </summary>
        readonly CancellationToken ct;
        
        /// <summary>
        /// 任务列表
        /// </summary>
        List<Task> tasks;
        
        /// <summary>
        /// 阶段是否完成
        /// </summary>
        public bool IsDone { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="steps">阶段步骤列表</param>
        /// <param name="ct">取消令牌</param>
        public ParallelPhase(List<PhaseStep> steps, CancellationToken ct) {
            this.steps = steps;
            this.ct = ct;
        }

        /// <summary>
        /// 开始执行并行阶段
        /// </summary>
        public void Start() {
            if (steps == null || steps.Count == 0) { IsDone = true; return; }
            tasks = new List<Task>(steps.Count);
            for (int i = 0; i < steps.Count; i++) tasks.Add(steps[i](ct));
        }

        /// <summary>
        /// 更新阶段状态
        /// </summary>
        /// <returns>阶段是否已完成</returns>
        public bool Update() {
            if (IsDone) return true;
            IsDone = tasks == null || tasks.TrueForAll(t => t.IsCompleted);
            return IsDone;
        }
    }
}