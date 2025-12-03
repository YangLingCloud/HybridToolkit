# Event Bus

一个轻量级的事件总线系统，用于Unity应用中的组件间通信。

## 功能特点

- **类型安全**：基于泛型实现的事件总线，确保类型安全
- **自动初始化**：在运行时自动初始化所有事件总线
- **编辑器支持**：在编辑器播放模式退出时自动清理事件总线
- **灵活绑定**：支持带参数和不带参数的事件处理方法
- **易于扩展**：可以轻松添加新的事件类型

## 文件结构

```
EventBus/
├── EventBinding.cs           # 事件绑定接口和实现
├── EventBus.cs               # 事件总线核心类
├── EventBusUtil.cs           # 事件总线工具类
├── Events.cs                 # 事件接口和示例事件
├── PredefinedAssemblyUtil.cs # 预定义程序集工具类
└── README.md                 # 文档
```

## 核心概念

### 1. 事件接口 (IEvent)

所有事件类型都必须实现`IEvent`接口：

```csharp
public interface IEvent { }
```

### 2. 事件类型

事件通常定义为结构体或类，包含事件所需的数据：

```csharp
public struct PlayerEvent : IEvent {
    public int health;
    public int mana;
}
```

### 3. 事件绑定

事件绑定用于将事件处理方法与事件总线关联：

```csharp
// 创建事件绑定
var binding = new EventBinding<PlayerEvent>();

// 添加事件处理方法
binding.OnEvent += OnPlayerEvent;
binding.OnEventNoArgs += OnPlayerEventNoArgs;

// 注册到事件总线
EventBus<PlayerEvent>.Register(binding);
```

### 4. 事件总线

事件总线用于发布和订阅事件：

```csharp
// 发布事件
var playerEvent = new PlayerEvent { health = 100, mana = 50 };
EventBus<PlayerEvent>.Raise(playerEvent);
```

## 使用方法

### 1. 定义事件类型

```csharp
using HybridToolkit.EventBus;

public struct EnemyDeathEvent : IEvent {
    public int enemyId;
    public int score;
}
```

### 2. 订阅事件

```csharp
using HybridToolkit.EventBus;
using UnityEngine;

public class ScoreManager : MonoBehaviour {
    private EventBinding<EnemyDeathEvent> enemyDeathBinding;
    
    private void Awake() {
        // 创建事件绑定
        enemyDeathBinding = new EventBinding<EnemyDeathEvent>();
        
        // 添加事件处理方法
        enemyDeathBinding.OnEvent += OnEnemyDeath;
        
        // 注册到事件总线
        EventBus<EnemyDeathEvent>.Register(enemyDeathBinding);
    }
    
    private void OnEnemyDeath(EnemyDeathEvent e) {
        Debug.Log($"Enemy {e.enemyId} died, gained {e.score} points!