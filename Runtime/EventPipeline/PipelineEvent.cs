using System;
using UnityEngine;

namespace HybridToolkit.Events
{
    /// <summary>
    /// 所有管线事件的基类。
    /// <para>必须是引用类型 (Class)，以支持在管线中传递修改和继承。</para>
    /// </summary>
    public abstract class PipelineEvent
    {
        /// <summary>
        /// 获取事件是否已被拦截。
        /// </summary>
        public bool IsConsumed { get; private set; } = false;

        /// <summary>
        /// 获取事件是否处于只读锁定状态。
        /// </summary>
        public bool IsLocked { get; private set; } = false;

        /// <summary>
        /// [可重写] 定义此事件的自动锁定阈值。
        /// <para>当管线执行到优先级 <= 此值的监听者时，事件将自动变为只读。</para>
        /// <para>默认值：EventPriority.Monitor (-1000)</para>
        /// </summary>
        public virtual int LockThreshold => (int)EventPriority.Monitor;

        /// <summary>
        /// 拦截事件。后续的监听者将不会收到此事件。
        /// <para>如果事件已锁定，拦截操作将失败并报错。</para>
        /// </summary>
        public void Consume()
        {
            if (IsLocked)
            {
                Debug.LogError($"[EventPipeline] 拦截失败：事件 {this.GetType().Name} 已进入只读阶段。");
                return;
            }
            IsConsumed = true;
        }

        /// <summary>
        /// 检查锁定状态。建议在事件属性的 Setter 中调用此方法。
        /// </summary>
        /// <exception cref="InvalidOperationException">如果事件已锁定，抛出异常。</exception>
        protected void CheckLock()
        {
            if (IsLocked)
            {
                throw new InvalidOperationException($"[EventPipeline] 修改失败：事件 {this.GetType().Name} 已进入只读阶段，禁止修改数据。");
            }
        }

        /// <summary>
        /// 内部方法：锁定事件。
        /// </summary>
        internal void Lock() => IsLocked = true;

        /// <summary>
        /// 重置事件状态（用于对象池复用）。
        /// </summary>
        public virtual void Reset()
        {
            IsConsumed = false;
            IsLocked = false;
        }
    }
}