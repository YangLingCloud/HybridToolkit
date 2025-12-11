using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HybridToolkit.HierachicalStateMachine {
    /// <summary>
    /// 顺序阶段类
    /// 实现了ISequence接口，用于顺序执行多个阶段步骤
    /// </summary>
    public class SequentialPhase : ISequence {
        /// <summary>
        /// 阶段步骤列表
        /// </summary>
        readonly List<PhaseStep> steps;
        
        /// <summary>
        /// 取消令牌
        /// </summary>
        readonly CancellationToken ct;
        
        /// <summary>
        /// 当前执行的步骤索引
        /// </summary>
        int index = -1;
        
        /// <summary>
        /// 当前执行的任务
        /// </summary>
        Task current;
        
        /// <summary>
        /// 阶段是否完成
        /// </summary>
        public bool IsDone { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="steps">阶段步骤列表</param>
        /// <param name="ct">取消令牌</param>
        public SequentialPhase(List<PhaseStep> steps, CancellationToken ct) {
            this.steps = steps;
            this.ct = ct;
        }
        
        /// <summary>
        /// 开始执行顺序阶段
        /// </summary>
        public void Start() => Next();

        /// <summary>
        /// 更新阶段状态
        /// </summary>
        /// <returns>阶段是否已完成</returns>
        public bool Update() {
            if (IsDone) return true;
            if (current == null || current.IsCompleted) Next();
            return IsDone;
        }

        /// <summary>
        /// 执行下一个步骤
        /// </summary>
        void Next() {
            index++;
            if (index >= steps.Count) { IsDone = true; return; }
            current = steps[index](ct);
        }
    }
}