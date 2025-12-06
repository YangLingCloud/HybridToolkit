# Singleton

一个灵活的单例模式实现集合，用于Unity应用中管理全局唯一实例。

## 目录

- [核心特性](#核心特性)
- [快速开始](#快速开始)
- [文件结构](#文件结构)
- [核心概念](#核心概念)
- [使用方法](#使用方法)
- [最佳实践](#最佳实践)

## 核心特性

- **泛型实现**：基于泛型的单例模式，支持多种单例类型
- **多种单例类型**：
  - 普通单例 (SingletonMono)
  - 持久化单例 (PersistentSingletonMono)
  - 调节型单例 (RegulatorSingletonMono)
- **自动实例化**：在需要时自动创建单例实例
- **场景管理**：支持场景切换时保持或销毁单例
- **线程安全**：在多线程环境下安全地访问单例实例

## 快速开始

### 环境要求
- Unity 2022.3或更高版本
- .NET 6或更高版本

### 安装步骤
1. 通过Unity Package Manager安装HybridToolkit包
2. 在代码中引用`HybridToolkit.Singleton`命名空间
3. 继承相应的单例基类开始使用

### 代码示例

#### 1. 普通单例
```csharp
using HybridToolkit.Singleton;
using UnityEngine;

public class UIManager : SingletonMono<UIManager> {
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private GameObject pauseMenu;
    
    public void ShowPauseMenu() {
        pauseMenu.SetActive(true);
    }
    
    public void HidePauseMenu() {
        pauseMenu.SetActive(false);
    }
}

// 使用方式：
UIManager.Instance.ShowPauseMenu();
```

#### 2. 持久化单例
```csharp
using HybridToolkit.Singleton;
using UnityEngine;

public class GameManager : PersistentSingletonMono<GameManager> {
    [SerializeField] private int playerScore;
    [SerializeField] private int playerLives;
    
    public void AddScore(int points) {
        playerScore += points;
        Debug.Log($"Score: {playerScore}");
    }
}

// 使用方式：
GameManager.Instance.AddScore(100);
```

#### 3. 调节型单例
```csharp
using HybridToolkit.Singleton;
using UnityEngine;

public class NetworkManager : RegulatorSingletonMono<NetworkManager> {
    // 设置初始化时间，决定单例创建顺序
    protected override float InitializationTime => 10f;
    
    private bool isConnected;
    
    protected override void Initialize() {
        base.Initialize();
        ConnectToServer();
    }
    
    private void ConnectToServer() {
        isConnected = true;
        Debug.Log("Connected to server");
    }
    
    public void SendMessage(string message) {
        if (isConnected) {
            Debug.Log($"Sending message: {message}");
        } else {
            Debug.Log("Not connected to server");
        }
    }
}

// 使用方式：
NetworkManager.Instance.SendMessage("Hello Server");
```

## 文件结构

```
Singleton/
├── SingletonMono.cs            # 普通单例基类
├── PersistentSingletonMono.cs  # 持久化单例基类
├── RegulatorSingletonMono.cs   # 调节型单例基类
└── README.md                   # 文档
```

## 核心概念

### 1. 单例模式

单例模式确保一个类只有一个实例，并提供一个全局访问点：

```csharp
public class MyManager : SingletonMono<MyManager> {
    // 单例的实现细节
}
```

### 2. 普通单例 (SingletonMono)

基本的单例实现，在场景中只有一个实例：

```csharp
public class UIManager : SingletonMono<UIManager> {
    // UI管理逻辑
}
```

### 3. 持久化单例 (PersistentSingletonMono)

持久化单例在场景切换时不会被销毁：

```csharp
public class GameManager : PersistentSingletonMono<GameManager> {
    // 游戏管理逻辑，跨场景持久化
}
```

### 4. 调节型单例 (RegulatorSingletonMono)

调节型单例通过初始化时间来管理多个单例实例的创建顺序：

```csharp
public class NetworkManager : RegulatorSingletonMono<NetworkManager> {
    // 设置初始化时间，决定单例创建顺序
    protected override float InitializationTime => 10f;
    
    // 网络管理逻辑，具有特定的初始化顺序
}
```

## 使用方法

详细的代码示例请参考[快速开始](#快速开始)章节中的示例。

## 最佳实践

1. **用途明确**：只对真正需要全局访问的类使用单例模式
2. **避免滥用**：不要将所有类都设计为单例，这会导致代码耦合度高
3. **依赖注入**：考虑使用依赖注入代替单例模式，以提高代码的可测试性
4. **生命周期管理**：明确单例的生命周期，避免内存泄漏
5. **初始化逻辑**：将复杂的初始化逻辑放在Initialize方法中，而不是Awake方法

### 示例场景

#### 游戏管理器

使用持久化单例管理游戏状态：

```csharp
public class GameManager : PersistentSingletonMono<GameManager> {
    public enum GameState { Menu, Playing, Paused, GameOver }
    
    public GameState CurrentState { get; private set; }
    public int Score { get; private set; }
    public int Lives { get; private set; }
    
    protected override void Initialize() {
        base.Initialize();
        ResetGame();
    }
    
    public void StartGame() {
        CurrentState = GameState.Playing;
        Debug.Log("Game started");
    }
    
    public void PauseGame() {
        if (CurrentState == GameState.Playing) {
            CurrentState = GameState.Paused;
            Debug.Log("Game paused");
        }
    }
    
    public void ResumeGame() {
        if (CurrentState == GameState.Paused) {
            CurrentState = GameState.Playing;
            Debug.Log("Game resumed");
        }
    }
    
    public void GameOver() {
        CurrentState = GameState.GameOver;
        Debug.Log("Game over");
    }
    
    public void AddScore(int points) {
        Score += points;
        Debug.Log($"Score: {Score}");
    }
    
    public void LoseLife() {
        Lives--;
        if (Lives <= 0) {
            GameOver();
        }
        Debug.Log($"Lives: {Lives}");
    }
    
    public void ResetGame() {
        Score = 0;
        Lives = 3;
        CurrentState = GameState.Menu;
        Debug.Log("Game reset");
    }
}

// 使用方式：
// 开始游戏
GameManager.Instance.StartGame();

// 添加分数
GameManager.Instance.AddScore(100);

// 暂停游戏
GameManager.Instance.PauseGame();
```

### 注意事项

1. **命名规范**：单例类名通常以"Manager"、"Service"等结尾，便于识别
2. **继承关系**：单例类应该直接继承自相应的单例基类
3. **Awake方法**：如果需要重写Awake方法，确保调用base.Awake()
4. **场景管理**：
   - 普通单例在场景切换时会被销毁
   - 持久化单例在场景切换时保持存在
5. **初始化顺序**：
   - 调节型单例通过InitializationTime属性控制初始化顺序
   - 数值较小的初始化时间会先创建
6. **线程安全**：单例实例的访问是线程安全的，但初始化过程不是
7. **资源清理**：在OnDestroy方法中清理资源，避免内存泄漏
