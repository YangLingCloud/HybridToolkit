using System.Collections.Generic;

namespace HybridToolkit.HierachicalStateMachine {
    /// <summary>
    /// 层次状态机类
    /// 管理状态树和状态转换
    /// </summary>
    public class StateMachine {
        /// <summary>
        /// 状态机的根状态
        /// </summary>
        public readonly State Root;
        
        /// <summary>
        /// 状态转换序列器
        /// 用于管理状态转换的顺序和时间
        /// </summary>
        public readonly TransitionSequencer Sequencer;
        
        /// <summary>
        /// 状态机是否已启动
        /// </summary>
        bool started;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="root">状态机的根状态</param>
        public StateMachine(State root) {
            Root = root;
            Sequencer = new TransitionSequencer(this);
        }

        /// <summary>
        /// 启动状态机
        /// 只在第一次调用时生效
        /// </summary>
        public void Start() {
            if (started) return;
            
            started = true;
            Root.Enter();
        }

        /// <summary>
        /// 每帧更新状态机
        /// 如果状态机未启动，会自动启动
        /// </summary>
        /// <param name="deltaTime">帧间隔时间</param>
        public void Tick(float deltaTime) {
            if (!started) Start();
            Sequencer.Tick(deltaTime);
        }
        
        /// <summary>
        /// 内部更新方法
        /// 直接更新根状态
        /// </summary>
        /// <param name="deltaTime">帧间隔时间</param>
        internal void InternalTick(float deltaTime) => Root.Update(deltaTime);
        
        /// <summary>
        /// 执行状态转换
        /// 通过退出到共享祖先，然后进入到目标状态来完成转换
        /// </summary>
        /// <param name="from">当前状态</param>
        /// <param name="to">目标状态</param>
        public void ChangeState(State from, State to) {
            if (from == to || from == null || to == null) return;
            
            // 找到两个状态的最近公共祖先
            State lca = TransitionSequencer.Lca(from, to);
            
            // 退出当前分支，直到但不包括最近公共祖先
            for (State s = from; s != lca; s = s.Parent) s.Exit();
            
            // 进入目标分支，从最近公共祖先到目标状态
            var stack = new Stack<State>();
            for (State s = to; s != lca; s = s.Parent) stack.Push(s);
            while (stack.Count > 0) stack.Pop().Enter();
        }
    }
}