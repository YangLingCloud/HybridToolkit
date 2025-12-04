using System.Collections.Generic;
using UnityEngine;

namespace HybridToolkit.Events
{
    /// <summary>
    /// 事件总线类，用于管理特定事件类型的订阅和发布
    /// </summary>
    /// <typeparam name="T">事件类型，必须实现IEvent接口</typeparam>
    public static class EventBus<T> where T : IEvent {
    /// <summary>
    /// 存储事件绑定的哈希集合，确保每个绑定只被添加一次
    /// </summary>
    static readonly HashSet<IEventBinding<T>> bindings = new HashSet<IEventBinding<T>>();
    
    /// <summary>
    /// 注册事件绑定
    /// </summary>
    /// <param name="binding">要注册的事件绑定</param>
    public static void Register(EventBinding<T> binding) => bindings.Add(binding);
    
    /// <summary>
    /// 注销事件绑定
    /// </summary>
    /// <param name="binding">要注销的事件绑定</param>
    public static void Deregister(EventBinding<T> binding) => bindings.Remove(binding);

    /// <summary>
    /// 触发事件，通知所有订阅者
    /// </summary>
    /// <param name="@event">要触发的事件对象</param>
    public static void Raise(T @event) {
        // 创建绑定集合的快照，以避免在遍历过程中修改集合导致的并发问题
        var snapshot = new HashSet<IEventBinding<T>>(bindings);

        foreach (var binding in snapshot) {
            // 再次检查绑定是否仍然存在，因为在遍历过程中可能已被移除
            if (bindings.Contains(binding)) {
                // 调用带事件参数的处理方法
                binding.OnEvent.Invoke(@event);
                // 调用不带事件参数的处理方法
                binding.OnEventNoArgs.Invoke();
            }
        }
    }

    /// <summary>
    /// 清除所有事件绑定
    /// </summary>
    static void Clear() {
        Debug.Log($"Clearing {typeof(T).Name} bindings");
        bindings.Clear();
    }
}
}
