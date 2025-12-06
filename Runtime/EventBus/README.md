# EventBus

一个轻量级的事件总线系统，用于实现Unity应用中的解耦通信。

## 目录

- [核心特性](#核心特性)
- [快速开始](#快速开始)
- [文件结构](#文件结构)
- [核心概念](#核心概念)
- [使用方法](#使用方法)
- [最佳实践](#最佳实践)

## 核心特性

- **轻量级**：高性能、低内存占用的事件分发系统
- **类型安全**：强类型事件定义和使用
- **解耦通信**：松耦合的组件间通信方式
- **异步支持**：支持异步事件处理
- **事件队列**：内置事件队列管理
- **性能优化**：使用对象池减少GC分配
- **调试支持**：提供事件调试工具
- **扩展性**：易于扩展和自定义

## 快速开始

### 环境要求
- Unity 2022.3或更高版本
- .NET 6或更高版本

### 安装步骤
1. 通过Unity Package Manager安装HybridToolkit包
2. 在代码中引用`HybridToolkit.EventBus`命名空间
3. 开始使用事件总线系统

### 代码示例

#### 1. 事件定义
```csharp
using HybridToolkit.EventBus;
using UnityEngine;

// 定义事件类型
public class PlayerHealthChangedEvent {
    public int newHealth;
    public int oldHealth;
    
    public PlayerHealthChangedEvent(int newHealth, int oldHealth) {
        this.newHealth = newHealth;
        this.oldHealth = oldHealth;
    }
}

public class GameStateChangedEvent {
    public GameState newState;
    public GameState previousState;
    
    public GameStateChangedEvent(GameState newState, GameState previousState) {
        this.newState = newState;
        this.previousState = previousState;
    }
}

public enum GameState {
    Menu,
    Playing,
    Paused,
    GameOver
}
```

#### 2. 事件订阅
```csharp
using HybridToolkit.EventBus;
using UnityEngine;

public class HealthUI : MonoBehaviour {
    private void OnEnable() {
        // 订阅生命值变化事件
        EventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
    }
    
    private void OnDisable() {
        // 取消订阅
        EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChanged);
    }
    
    private void OnHealthChanged(PlayerHealthChangedEvent healthEvent) {
        Debug.Log($"Health changed from {healthEvent.oldHealth} to {healthEvent.newHealth}");
        // 更新UI逻辑
    }
}

// 订阅多个事件类型
public class GameManager : MonoBehaviour {
    private void OnEnable() {
        EventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
        EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }
    
    private void OnDisable() {
        EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChanged);
        EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
    }
    
    private void OnHealthChanged(PlayerHealthChangedEvent healthEvent) {
        if (healthEvent.newHealth <= 0) {
            EventBus.Publish(new GameStateChangedEvent(GameState.GameOver, GameState.Playing));
        }
    }
    
    private void OnGameStateChanged(GameStateChangedEvent stateEvent) {
        Debug.Log($"Game state changed from {stateEvent.previousState} to {stateEvent.newState}");
    }
}
```

#### 3. 事件发布
```csharp
using HybridToolkit.EventBus;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {
    private int currentHealth = 100;
    
    public void TakeDamage(int damage) {
        int oldHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // 发布生命值变化事件
        EventBus.Publish(new PlayerHealthChangedEvent(currentHealth, oldHealth));
    }
    
    public void Heal(int healAmount) {
        int oldHealth = currentHealth;
        currentHealth = Mathf.Min(100, currentHealth + healAmount);
        
        EventBus.Publish(new PlayerHealthChangedEvent(currentHealth, oldHealth));
    }
}

public class GameStateManager : MonoBehaviour {
    private GameState currentState = GameState.Menu;
    
    public void ChangeGameState(GameState newState) {
        if (currentState != newState) {
            GameState previousState = currentState;
            currentState = newState;
            
            EventBus.Publish(new GameStateChangedEvent(newState, previousState));
        }
    }
    
    public void StartGame() {
        ChangeGameState(GameState.Playing);
    }
    
    public void PauseGame() {
        ChangeGameState(GameState.Paused);
    }
    
    public void ResumeGame() {
        ChangeGameState(GameState.Playing);
    }
    
    public void EndGame() {
        ChangeGameState(GameState.GameOver);
    }
}
```

#### 4. 异步事件处理
```csharp
using HybridToolkit.EventBus;
using UnityEngine;
using System.Threading.Tasks;

public class AsyncEventHandler : MonoBehaviour {
    private async void OnEnable() {
        EventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChangedAsync);
    }
    
    private async void OnDisable() {
        EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChangedAsync);
    }
    
    private async void OnHealthChangedAsync(PlayerHealthChangedEvent healthEvent) {
        // 异步处理事件
        await ProcessHealthChangeAsync(healthEvent);
    }
    
    private async Task ProcessHealthChangeAsync(PlayerHealthChangedEvent healthEvent) {
        // 模拟异步操作
        await Task.Delay(100);
        
        Debug.Log($"Processed health change: {healthEvent.oldHealth} -> {healthEvent.newHealth}");
    }
}

// 使用异步Lambda表达式
public class AsyncLambdaHandler : MonoBehaviour {
    private void OnEnable() {
        EventBus.Subscribe<PlayerHealthChangedEvent>(async (healthEvent) => {
            await Task.Delay(50);
            Debug.Log($"Async lambda: Health {healthEvent.oldHealth} -> {healthEvent.newHealth}");
        });
    }
    
    private void OnDisable() {
        EventBus.Unsubscribe<PlayerHealthChangedEvent>(async (healthEvent) => {
            await Task.Delay(50);
            Debug.Log($"Async lambda: Health {healthEvent.oldHealth} -> {healthEvent.newHealth}");
        });
    }
}
```

## 文件结构

```
EventBus/
├── EventBus.cs                    # 事件总线核心
├── EventQueue.cs                  # 事件队列管理
├── EventPool.cs                   # 事件对象池
├── EventListener.cs               # 事件监听器
├── EventSubscription.cs           # 事件订阅管理
└── README.md                      # 文档
```

## 核心概念

### 1. 事件总线

事件总线是发布/订阅模式的一种实现：

```csharp
public static class EventBus {
    // 发布事件
    public static void Publish<T>(T eventData) where T : class;
    
    // 订阅事件
    public static void Subscribe<T>(Action<T> callback) where T : class;
    
    // 取消订阅
    public static void Unsubscribe<T>(Action<T> callback) where T : class;
    
    // 异步订阅
    public static void SubscribeAsync<T>(Func<T, Task> callback) where T : class;
    
    // 清除所有订阅
    public static void ClearAll();
}
```

### 2. 事件定义

事件是数据传输的载体：

```csharp
public class MyEvent {
    public string message;
    public int value;
    
    public MyEvent(string message, int value) {
        this.message = message;
        this.value = value;
    }
}
```

### 3. 事件监听器

事件监听器处理接收到的事件：

```csharp
public class MyEventListener : MonoBehaviour {
    private void OnEnable() {
        EventBus.Subscribe<MyEvent>(OnMyEventReceived);
    }
    
    private void OnDisable() {
        EventBus.Unsubscribe<MyEvent>(OnMyEventReceived);
    }
    
    private void OnMyEventReceived(MyEvent myEvent) {
        Debug.Log($"Received event: {myEvent.message}, value: {myEvent.value}");
    }
}
```

### 4. 事件队列

事件队列管理事件的发布顺序：

```csharp
// 立即发布
EventBus.Publish(new MyEvent("immediate", 1));

// 延迟发布
EventBus.PublishDelayed(new MyEvent("delayed", 2), 1.0f);

// 批量发布
EventBus.PublishBatch(new MyEvent[] {
    new MyEvent("batch1", 1),
    new MyEvent("batch2", 2),
    new MyEvent("batch3", 3)
});
```

## 使用方法

详细的代码示例请参考[快速开始](#快速开始)章节中的示例。

## 最佳实践

1. **事件命名**：使用描述性的事件名称和清晰的命名规范
2. **内存管理**：及时取消订阅，避免内存泄漏
3. **性能考虑**：避免在事件处理中进行大量计算
4. **错误处理**：在事件处理器中包含适当的错误处理
5. **线程安全**：确保事件处理器的线程安全性
6. **调试支持**：使用调试工具监控事件流

### 示例场景

#### 游戏状态管理

```csharp
// 游戏状态事件
public class GameStateEvent {
    public GameState previousState;
    public GameState currentState;
    public float transitionTime;
    
    public GameStateEvent(GameState previous, GameState current, float time) {
        previousState = previous;
        currentState = current;
        transitionTime = time;
    }
}

// UI状态管理
public class UIController : MonoBehaviour {
    private void OnEnable() {
        EventBus.Subscribe<GameStateEvent>(OnGameStateChanged);
    }
    
    private void OnDisable() {
        EventBus.Unsubscribe<GameStateEvent>(OnGameStateChanged);
    }
    
    private void OnGameStateChanged(GameStateEvent stateEvent) {
        switch (stateEvent.currentState) {
            case GameState.Menu:
                ShowMainMenu();
                break;
            case GameState.Playing:
                ShowGameUI();
                break;
            case GameState.Paused:
                ShowPauseMenu();
                break;
            case GameState.GameOver:
                ShowGameOverUI();
                break;
        }
    }
    
    private void ShowMainMenu() { /* 显示主菜单 */ }
    private void ShowGameUI() { /* 显示游戏UI */ }
    private void ShowPauseMenu() { /* 显示暂停菜单 */ }
    private void ShowGameOverUI() { /* 显示游戏结束UI */ }
}

// 音频管理
public class AudioManager : MonoBehaviour {
    private void OnEnable() {
        EventBus.Subscribe<GameStateEvent>(OnGameStateChanged);
    }
    
    private void OnDisable() {
        EventBus.Unsubscribe<GameStateEvent>(OnGameStateChanged);
    }
    
    private void OnGameStateChanged(GameStateEvent stateEvent) {
        switch (stateEvent.currentState) {
            case GameState.Menu:
                PlayBackgroundMusic("menu_theme");
                break;
            case GameState.Playing:
                PlayBackgroundMusic("game_theme");
                break;
            case GameState.Paused:
                PlaySoundEffect("pause_sound");
                break;
            case GameState.GameOver:
                PlayBackgroundMusic("game_over_theme");
                break;
        }
    }
    
    private void PlayBackgroundMusic(string musicName) { /* 播放背景音乐 */ }
    private void PlaySoundEffect(string soundName) { /* 播放音效 */ }
}
```

### 注意事项

1. **订阅管理**：确保在适当时机订阅和取消订阅
2. **事件风暴**：避免创建过多的事件类型
3. **循环依赖**：避免事件处理器之间的循环依赖
4. **性能监控**：监控事件系统对性能的影响
5. **调试工具**：使用事件调试工具追踪问题
6. **版本兼容性**：确保事件定义的向后兼容性
