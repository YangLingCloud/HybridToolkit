using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace HSM {
    /// <summary>
    /// 转换序列器类
    /// 负责管理状态之间的转换过程
    /// </summary>
    public class TransitionSequencer {
        /// <summary>
        /// 关联的状态机
        /// </summary>
        public readonly StateMachine Machine;
        
        /// <summary>
        /// 当前阶段的序列器（停用或激活）
        /// </summary>
        ISequence sequencer;
        
        /// <summary>
        /// 下一阶段的操作
        /// </summary>
        Action nextPhase;
        
        /// <summary>
        /// 待处理的转换请求
        /// </summary>
        (State from, State to)? pending;
        
        /// <summary>
        /// 上一次的转换状态
        /// </summary>
        State lastFrom, lastTo;
        
        /// <summary>
        /// 取消令牌源
        /// </summary>
        CancellationTokenSource cts;
        
        /// <summary>
        /// 是否使用顺序执行模式（false表示并行执行）
        /// </summary>
        bool UseSequential = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="machine">关联的状态机</param>
        public TransitionSequencer(StateMachine machine) {
            Machine = machine;
        }

        /// <summary>
        /// 请求从一个状态转换到另一个状态
        /// </summary>
        /// <param name="from">起始状态</param>
        /// <param name="to">目标状态</param>
        public void RequestTransition(State from, State to) {
            if (to == null || from == to) return;
            if (sequencer != null){ pending = (from, to); return; }
            BeginTransition(from, to);
        }

        /// <summary>
        /// 收集阶段步骤
        /// </summary>
        /// <param name="chain">状态链</param>
        /// <param name="deactivate">是否为停用操作</param>
        /// <returns>阶段步骤列表</returns>
        static List<PhaseStep> GatherPhaseSteps(List<State> chain, bool deactivate) {
            var steps = new List<PhaseStep>();

            for (int i = 0; i < chain.Count; i++) {
                var st = chain[i];
                var acts = chain[i].Activities;
                for (int j = 0; j < acts.Count; j++){
                    var a = acts[j];
                    bool include = deactivate ? (a.Mode == ActivityMode.Active)
                        : (a.Mode == ActivityMode.Inactive);
                    if (!include) continue;

                    Debug.Log($"[Phase {(deactivate?"Exit":"Enter")}] state={st.GetType().Name}, activity={a.GetType().Name}, mode={a.Mode}");

                    steps.Add(ct => deactivate ? a.DeactivateAsync(ct) : a.ActivateAsync(ct));
                }
            }
            return steps;
        }
        
        /// <summary>
        /// 获取需要退出的状态列表
        /// 从起始状态到LCA（但不包括LCA），按从下到上的顺序
        /// </summary>
        /// <param name="from">起始状态</param>
        /// <param name="lca">最近公共祖先</param>
        /// <returns>需要退出的状态列表</returns>
        static List<State> StatesToExit(State from, State lca) {
            var list = new List<State>();
            for (var s = from; s != null && s != lca; s = s.Parent) list.Add(s);
            return list;
        }
        
        /// <summary>
        /// 获取需要进入的状态列表
        /// 从目标状态到LCA（但不包括LCA），按从上到下的顺序返回
        /// </summary>
        /// <param name="to">目标状态</param>
        /// <param name="lca">最近公共祖先</param>
        /// <returns>需要进入的状态列表</returns>
        static List<State> StatesToEnter(State to, State lca) {
            var stack = new Stack<State>();
            for (var s = to; s != lca; s = s.Parent) stack.Push(s);
            return new List<State>(stack);
        }

        /// <summary>
        /// 开始转换过程
        /// </summary>
        /// <param name="from">起始状态</param>
        /// <param name="to">目标状态</param>
        void BeginTransition(State from, State to) {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            
            var lca        = Lca(from, to);
            var exitChain  = StatesToExit(from, lca);
            var enterChain = StatesToEnter(to,  lca);
            
            // 1. 停用"旧分支"
            var exitSteps  = GatherPhaseSteps(exitChain, deactivate: true);
            sequencer = UseSequential
                ? new SequentialPhase(exitSteps, cts.Token)
                : new ParallelPhase(exitSteps, cts.Token);
            sequencer.Start();
            
            nextPhase = () => {
                // 2. 切换状态
                Machine.ChangeState(from, to);
                // 3. 激活"新分支"
                var enterSteps = GatherPhaseSteps(enterChain, deactivate: false);
                sequencer = UseSequential
                    ? new SequentialPhase(enterSteps, cts.Token)
                    : new ParallelPhase(enterSteps, cts.Token);
                sequencer.Start();
            };
        }

        /// <summary>
        /// 结束转换过程
        /// </summary>
        void EndTransition() {
            sequencer = null;

            if (pending.HasValue) {
                (State from, State to) p = pending.Value;
                pending = null;
                BeginTransition(p.from, p.to);
            }
        }

        /// <summary>
        /// 更新序列器状态
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public void Tick(float deltaTime) {
            if (sequencer != null) {
                if (sequencer.Update()) {
                    if (nextPhase != null) {
                        var n = nextPhase;
                        nextPhase = null;
                        n();
                    } else {
                        EndTransition();
                    }
                }
                return; // 在转换过程中，不运行正常更新
            }
            Machine.InternalTick(deltaTime);
        }

        /// <summary>
        /// 计算两个状态的最近公共祖先(LCA)
        /// </summary>
        /// <param name="a">第一个状态</param>
        /// <param name="b">第二个状态</param>
        /// <returns>最近公共祖先状态</returns>
        public static State Lca(State a, State b) {
            // 创建'a'的所有父状态的集合
            var ap = new HashSet<State>();
            for (var s = a; s != null; s = s.Parent) ap.Add(s);

            // 查找'b'的第一个也是'a'的父状态
            for (var s = b; s != null; s = s.Parent)
                if (ap.Contains(s))
                    return s;

            // 如果没有找到公共祖先，返回null
            return null;
        }
    }
}