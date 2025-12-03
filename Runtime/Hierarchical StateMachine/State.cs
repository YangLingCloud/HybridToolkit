using System.Collections.Generic;

namespace HSM {
    /// <summary>
    /// 状态基类
    /// 定义了状态机中的状态行为和生命周期
    /// </summary>
    public abstract class State {
        /// <summary>
        /// 所属的状态机
        /// </summary>
        public readonly StateMachine Machine;
        
        /// <summary>
        /// 父状态
        /// </summary>
        public readonly State Parent;
        
        /// <summary>
        /// 当前活跃的子状态
        /// </summary>
        public State ActiveChild;
        
        /// <summary>
        /// 状态的活动列表
        /// </summary>
        readonly List<IActivity> activities = new List<IActivity>();
        
        /// <summary>
        /// 只读的活动列表
        /// </summary>
        public IReadOnlyList<IActivity> Activities => activities;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="machine">所属的状态机</param>
        /// <param name="parent">父状态，默认为null</param>
        public State(StateMachine machine, State parent = null) {
            Machine = machine;
            Parent = parent;
        }
        
        /// <summary>
        /// 添加活动到状态
        /// </summary>
        /// <param name="a">要添加的活动</param>
        public void Add(IActivity a){ if (a != null) activities.Add(a); }
        
        /// <summary>
        /// 获取初始子状态
        /// 当状态启动时进入的初始子状态
        /// 返回null表示这是一个叶子状态
        /// </summary>
        /// <returns>初始子状态或null</returns>
        protected virtual State GetInitialState() => null;
        
        /// <summary>
        /// 获取当前帧的转换目标状态
        /// 返回null表示保持当前状态
        /// </summary>
        /// <returns>目标状态或null</returns>
        protected virtual State GetTransition() => null;
        
        // 生命周期钩子
        /// <summary>
        /// 状态进入时调用的钩子方法
        /// 可以被子类重写以添加自定义逻辑
        /// </summary>
        protected virtual void OnEnter() { }
        
        /// <summary>
        /// 状态退出时调用的钩子方法
        /// 可以被子类重写以添加自定义逻辑
        /// </summary>
        protected virtual void OnExit() { }
        
        /// <summary>
        /// 状态每帧更新时调用的钩子方法
        /// 可以被子类重写以添加自定义逻辑
        /// </summary>
        /// <param name="deltaTime">帧间隔时间</param>
        protected virtual void OnUpdate(float deltaTime) { }

        /// <summary>
        /// 进入状态
        /// 内部方法，由状态机调用
        /// </summary>
        internal void Enter() {
            if (Parent != null) Parent.ActiveChild = this;
            OnEnter();
            State init = GetInitialState();
            if (init != null) init.Enter();
        }
        
        /// <summary>
        /// 退出状态
        /// 内部方法，由状态机调用
        /// </summary>
        internal void Exit() {
            if (ActiveChild != null) ActiveChild.Exit();
            ActiveChild = null;
            OnExit();
        }
        
        /// <summary>
        /// 更新状态
        /// 内部方法，由状态机调用
        /// </summary>
        /// <param name="deltaTime">帧间隔时间</param>
        internal void Update(float deltaTime) {
            State t = GetTransition();
            if (t != null) {
                Machine.Sequencer.RequestTransition(this, t);
                return;
            }
            
            if (ActiveChild != null) ActiveChild.Update(deltaTime);
            OnUpdate(deltaTime);
        }
        
        /// <summary>
        /// 获取当前活跃路径的叶子状态
        /// 返回最深层的活跃后代状态
        /// </summary>
        /// <returns>叶子状态</returns>
        public State Leaf() {
            State s = this;
            while (s.ActiveChild != null) s = s.ActiveChild;
            return s;
        }
        
        /// <summary>
        /// 获取从当前状态到根状态的路径
        /// 按顺序返回当前状态、父状态、祖父状态...直到根状态
        /// </summary>
        /// <returns>状态路径的枚举器</returns>
        public IEnumerable<State> PathToRoot() {
            for (State s = this; s != null; s = s.Parent) yield return s;
        }
    }
}