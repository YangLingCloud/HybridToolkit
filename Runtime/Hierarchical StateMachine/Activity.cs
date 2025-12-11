using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HybridToolkit.HierachicalStateMachine {
    /// <summary>
    /// 活动模式枚举
    /// 定义了活动可能的状态
    /// </summary>
    public enum ActivityMode { 
        /// <summary>不活跃状态</summary>
        Inactive, 
        /// <summary>正在激活状态</summary>
        Activating, 
        /// <summary>活跃状态</summary>
        Active, 
        /// <summary>正在停用状态</summary>
        Deactivating 
    }

    /// <summary>
    /// 活动接口
    /// 定义了活动必须实现的方法
    /// </summary>
    public interface IActivity {
        /// <summary>
        /// 获取活动当前的模式
        /// </summary>
        ActivityMode Mode { get; }
        
        /// <summary>
        /// 异步激活活动
        /// </summary>
        /// <param name="ct">取消令牌</param>
        /// <returns>异步任务</returns>
        Task ActivateAsync(CancellationToken ct);
        
        /// <summary>
        /// 异步停用活动
        /// </summary>
        /// <param name="ct">取消令牌</param>
        /// <returns>异步任务</returns>
        Task DeactivateAsync(CancellationToken ct);
    }

    /// <summary>
    /// 活动基类
    /// 实现了IActivity接口的基本功能
    /// </summary>
    public abstract class Activity : IActivity {
        /// <summary>
        /// 活动当前的模式
        /// </summary>
        public ActivityMode Mode { get; protected set; } = ActivityMode.Inactive;

        /// <summary>
        /// 异步激活活动
        /// </summary>
        /// <param name="ct">取消令牌</param>
        /// <returns>异步任务</returns>
        public virtual async Task ActivateAsync(CancellationToken ct) {
            if (Mode != ActivityMode.Inactive) return;
            
            Mode = ActivityMode.Activating;
            await Task.CompletedTask;
            Mode = ActivityMode.Active;
            Debug.Log($"Activated {GetType().Name} (mode={Mode})");
        }

        /// <summary>
        /// 异步停用活动
        /// </summary>
        /// <param name="ct">取消令牌</param>
        /// <returns>异步任务</returns>
        public virtual async Task DeactivateAsync(CancellationToken ct) {
            if (Mode != ActivityMode.Active) return;
            
            Mode = ActivityMode.Deactivating;
            await Task.CompletedTask;
            Mode = ActivityMode.Inactive;
            Debug.Log($"Deactivated {GetType().Name} (mode={Mode})");
        }
    }
}