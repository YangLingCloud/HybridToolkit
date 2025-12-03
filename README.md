# HybridToolkit

一个整合多种设计模式和工具类的Unity开发套件，是RookieFramework的核心组件。

## 功能特性

### 设计模式实现
- **单例模式**
  - `SingletonMono<T>` - 基础泛型单例MonoBehaviour基类
  - `PersistentSingletonMono<T>` - 场景切换时不会被销毁的持久化单例
  - `RegulatorSingletonMono<T>` - 通过初始化时间管理多个实例的调节型单例

### 核心优势
- **0GC设计**：减少内存分配和垃圾回收开销
- **高性能**：优化的实现确保最小的性能影响
- **易用性**：简洁的API设计，易于集成和使用
- **扩展性**：支持自定义扩展和重写

## 快速开始

### 使用单例模式

#### 基础单例
```csharp
using HybridToolkit;
using UnityEngine;

public class GameManager : SingletonMono<GameManager>
{
    public void StartGame()
    {
        Debug.Log("游戏开始");
    }
}

// 使用方式
void SomeMethod()
{
    GameManager.Instance.StartGame();
}
```

#### 持久化单例（场景切换不销毁）
```csharp
using HybridToolkit;
using UnityEngine;

public class AudioManager : PersistentSingletonMono<AudioManager>
{
    [SerializeField] private AudioSource bgmSource;
    
    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.Play();
    }
}
```

#### 调节型单例（自动管理多个实例）
```csharp
using HybridToolkit;
using UnityEngine;

public class NetworkManager : RegulatorSingletonMono<NetworkManager>
{
    public void ConnectToServer()
    {
        // 连接服务器逻辑
    }
}
```

## 安装说明

### 环境要求
- Unity 2022.3或更高版本
- .NET 6或更高版本

### 依赖
- [UniTask](https://github.com/Cysharp/UniTask) - 高性能异步编程库

## 许可证

MIT License

## 作者

- YangLingYun - [GitHub](https://github.com/YangLingCloud)
