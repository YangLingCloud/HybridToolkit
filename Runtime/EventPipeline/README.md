# HybridToolkit.Events (Event Pipeline)

一个基于 **UniTask** 的高性能、异步事件管线系统。它专为解决复杂游戏逻辑中的顺序执行、数据流转、状态管理以及逻辑层与表现层的解耦而设计。

## 目录

- [核心特性](#核心特性)
- [环境要求](#环境要求)
- [安装](#安装)
- [快速开始](#快速开始)
- [高级特性：阶段性锁定](#高级特性阶段性锁定)
- [性能优化](#性能优化)
- [API 参考](#api-参考)
- [文件结构](#文件结构)

## 核心特性

- **异步管线**：深度集成 `UniTask`，支持 `async/await`，允许高优先级逻辑阻塞低优先级逻辑（例如：等待网络校验完成后，再执行后续计算）。
- **零 GC**：核心管线在事件触发、排序和异步等待过程中，无堆内存分配（基于 Struct Task）。
- **优先级排序**：支持 High, Normal, Low 等精细化控制执行顺序，确保业务逻辑的确定性。
- **管线控制**：支持事件拦截（Consume），可中断后续逻辑的执行。
- **阶段锁定**：支持基于优先级的自动只读锁定机制，防止表现层（如 UI）意外修改逻辑层数据。
- **混合注册**：同时支持同步方法（`void`）和异步方法（`async UniTask`）的混合订阅。
- **安全稳健**：内置 Unity 对象生命周期检查，自动清理已销毁的监听者。

## 环境要求

- Unity 2021.3 或更高版本
- **UniTask** (必须安装 [Cysharp.Threading.Tasks](https://github.com/Cysharp/UniTask))

## 安装

1. 确保项目中已安装 UniTask。
2. 将 `HybridToolkit/Events` 文件夹复制到你的 Unity 项目中。

## 快速开始

### 1. 定义事件
所有事件必须继承自 `PipelineEvent` 类。为了利用锁定保护机制，建议使用属性并在 Setter 中调用 `CheckLock()`。

```csharp
using HybridToolkit.Events;

public class DamageEvent : PipelineEvent 
{
    private int _amount;
    
    // 建议使用属性保护数据
    public int Amount 
    {
        get => _amount;
        set 
        { 
            CheckLock(); // 检查是否处于只读阶段
            _amount = value; 
        }
    }
    
    public string TargetName;
}
````
2. 订阅事件

支持在 OnEnable 中注册，在 OnDisable 中注销。支持混合使用同步和异步方法。
````csharp
using UnityEngine;
using Cysharp.Threading.Tasks;
using HybridToolkit.Events;

public class PlayerHealth : MonoBehaviour
{
void OnEnable()
{
// 注册核心逻辑 (Normal 优先级)
EventPipeline<DamageEvent>.Subscribe(OnTakeDamage, (int)EventPriority.Normal);

        // 注册 UI 显示 (Monitor 优先级)
        EventPipeline<DamageEvent>.Subscribe(OnShowUI, (int)EventPriority.Monitor);
    }

    void OnDisable() 
    {
        EventPipeline<DamageEvent>.Unsubscribe(OnTakeDamage);
        EventPipeline<DamageEvent>.Unsubscribe(OnShowUI);
    }

    // 核心逻辑：可以修改数据
    void OnTakeDamage(DamageEvent e) 
    {
        e.Amount -= 10; // 扣除护甲
        if (e.Amount < 0) e.Amount = 0;
    }

    // 表现层逻辑：在此阶段事件已变为只读
    void OnShowUI(DamageEvent e) 
    {
        Debug.Log($"受到伤害: {e.Amount}");
        
        // e.Amount = 0; // 取消注释此行将抛出 InvalidOperationException 异常
    }
}
````


3. 触发事件

由于管线支持异步，触发时必须使用 await 等待管线执行完毕。
````csharp


public class GameTester : MonoBehaviour
{
public async void TriggerDamage()
{
var evt = new DamageEvent { Amount = 100 };

        // 等待整个管线（包含可能的异步任务）执行完毕
        await EventPipeline<DamageEvent>.Raise(evt);
        
        Debug.Log("伤害处理流程结束");
    }
}
````


高级特性：阶段性锁定

系统允许通过重写 LockThreshold 属性，来定义事件在哪个优先级阶段变为只读。默认情况下，事件在进入 Monitor (-1000) 优先级时会自动锁定。
自定义锁定阈值

如果某个事件非常敏感（例如聊天消息或核心状态同步），希望经过 High 优先级处理后即禁止修改，可以如下配置：
````csharp


public class SecureEvent : PipelineEvent
{
// 重写阈值：当优先级 <= Normal (0) 时，事件即刻锁定
public override int LockThreshold => (int)EventPriority.Normal;

    private string _data;
    public string Data 
    {
        get => _data;
        set { CheckLock(); _data = value; }
    }
}
````



行为说明：

    High 优先级：允许读取和修改。

    Normal 优先级：只读（已触发锁定）。

    Low 优先级：只读。

    Monitor 优先级：只读。

性能优化

对于极高频触发的事件（如弹幕游戏中的子弹击中判定），建议结合 静态对象池 使用，以避免 new 操作产生的 GC。
````csharp
public class PooledEvent : PipelineEvent
{
    public int Value;
    
    // 简单的静态池实现
    private static readonly Stack<PooledEvent> _pool = new Stack<PooledEvent>();
    
    public static PooledEvent Get(int value)
    {
        var evt = _pool.Count > 0 ? _pool.Pop() : new PooledEvent();
        evt.Reset(); // 必须重置父类状态
        evt.Value = value;
        return evt;
    }
    
    public static void Release(PooledEvent evt)
    {
        _pool.Push(evt);
    }
}

// 使用方式
public async void Trigger()
{
    var evt = PooledEvent.Get(100);
    try 
    {
        await EventPipeline<PooledEvent>.Raise(evt);
    }
    finally
    {
        PooledEvent.Release(evt);
    }
}
````


## API 参考

### EventPipeline<T>

| 方法 | 描述 |
|------|------|
| `Subscribe(Func<T, UniTask> callback, int priority)` | 注册异步监听者。返回 IDisposable 用于注销（Lambda 必需）。 |
| `Subscribe(Action<T> callback, int priority)` | 注册同步监听者。系统会自动包装。 |
| `Unsubscribe(Func<T, UniTask> callback)` | 注销异步监听者。 |
| `Unsubscribe(Action<T> callback)` | 注销同步监听者。 |
| `Raise(T eventData)` | [Awaitable] 触发管线。必须使用 await 等待执行完毕。 |

### PipelineEvent

| 属性/方法 | 描述 |
|-----------|------|
| `bool IsConsumed` | 获取事件是否已被拦截。 |
| `bool IsLocked` | 获取事件是否处于只读状态。 |
| `virtual int LockThreshold` | [可重写] 定义自动锁定的优先级阈值。 |
| `void Consume()` | 标记事件为拦截状态，停止向后传播（受锁限制）。 |
| `void Reset()` | 重置状态（用于对象池复用）。 |
| `void CheckLock()` | 检查锁状态，若已锁定则抛出异常。 |
## 文件结构

```plaintext
HybridToolkit/
└── Events/
    ├── EventPipeline.cs           # 核心管线实现（包含 Listener 结构与 Raise 逻辑）
    ├── PipelineEvent.cs           # 事件基类（包含锁机制与拦截逻辑）
    └── EventPriority.cs           # 优先级枚举定义
```