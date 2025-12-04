using System;

namespace HybridToolkit.Events
{
    /// <summary>
    /// 事件绑定接口，用于定义事件处理方法的访问器
    /// </summary>
    /// <typeparam name="T">事件类型，必须实现IEvent接口</typeparam>
    public interface IEventBinding<T> {
    /// <summary>
    /// 带事件参数的事件处理方法
    /// </summary>
    public Action<T> OnEvent { get; set; }
    
    /// <summary>
    /// 不带事件参数的事件处理方法
    /// </summary>
    public Action OnEventNoArgs { get; set; }
}

/// <summary>
/// 事件绑定实现类，用于管理事件处理方法的注册和移除
/// </summary>
/// <typeparam name="T">事件类型，必须实现IEvent接口</typeparam>
public class EventBinding<T> : IEventBinding<T> where T : IEvent {
    /// <summary>
    /// 带事件参数的事件处理方法
    /// </summary>
    Action<T> onEvent = _ => { };
    
    /// <summary>
    /// 不带事件参数的事件处理方法
    /// </summary>
    Action onEventNoArgs = () => { };

    /// <summary>
    /// 带事件参数的事件处理方法属性
    /// </summary>
    Action<T> IEventBinding<T>.OnEvent {
        get => onEvent;
        set => onEvent = value;
    }

    /// <summary>
    /// 不带事件参数的事件处理方法属性
    /// </summary>
    Action IEventBinding<T>.OnEventNoArgs {
        get => onEventNoArgs;
        set => onEventNoArgs = value;
    }

    /// <summary>
    /// 构造函数，初始化带事件参数的事件处理方法
    /// </summary>
    /// <param name="onEvent">带事件参数的事件处理方法</param>
    public EventBinding(Action<T> onEvent) => this.onEvent = onEvent;
    
    /// <summary>
    /// 构造函数，初始化不带事件参数的事件处理方法
    /// </summary>
    /// <param name="onEventNoArgs">不带事件参数的事件处理方法</param>
    public EventBinding(Action onEventNoArgs) => this.onEventNoArgs = onEventNoArgs;
    
    /// <summary>
    /// 添加不带事件参数的事件处理方法
    /// </summary>
    /// <param name="onEvent">不带事件参数的事件处理方法</param>
    public void Add(Action onEvent) => onEventNoArgs += onEvent;
    
    /// <summary>
    /// 移除不带事件参数的事件处理方法
    /// </summary>
    /// <param name="onEvent">不带事件参数的事件处理方法</param>
    public void Remove(Action onEvent) => onEventNoArgs -= onEvent;
    
    /// <summary>
    /// 添加带事件参数的事件处理方法
    /// </summary>
    /// <param name="onEvent">带事件参数的事件处理方法</param>
    public void Add(Action<T> onEvent) => this.onEvent += onEvent;
    
    /// <summary>
    /// 移除带事件参数的事件处理方法
    /// </summary>
    /// <param name="onEvent">带事件参数的事件处理方法</param>
    public void Remove(Action<T> onEvent) => this.onEvent -= onEvent;
}
}