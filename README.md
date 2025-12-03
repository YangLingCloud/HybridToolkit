# HybridToolkit

一个整合多种设计模式和工具类的Unity开发套件，是RookieFramework的核心组件，提供了状态管理、相机控制、事件通信和单例模式等多种实用功能。

## 功能特性

### 1. 游戏逻辑状态管理系统
提供完整的分层状态机实现，用于管理复杂的游戏逻辑流程：
- 支持状态嵌套和层级管理
- 提供状态转换序列控制
- 支持并行和顺序执行的阶段管理
- 灵活的状态机构建器API

### 2. 相机控制组件
实现了灵活的3D相机控制系统：
- 支持自由旋转、缩放和平移
- 提供平滑过渡和自动归位功能
- 可配置的相机参数和限制
- 支持输入系统集成

### 3. 事件总线系统
轻量级的组件间通信机制：
- 类型安全的事件定义
- 支持事件订阅和触发
- 线程安全的事件处理
- 灵活的事件绑定管理

### 4. 设计模式实现
- **单例模式**
  - `SingletonMono<T>` - 基础泛型单例MonoBehaviour基类
  - `PersistentSingletonMono<T>` - 场景切换时不会被销毁的持久化单例
  - `RegulatorSingletonMono<T>` - 通过初始化时间管理多个实例的调节型单例


## 快速开始

详细的使用示例请参考各个模块目录下的README文件：
- **状态管理系统**：`Runtime/Hierarchical StateMachine/README.md`
- **相机控制组件**：`Runtime/CameraController/README.md`
- **事件总线系统**：`Runtime/EventBus/README.md`
- **单例模式实现**：`Runtime/Singleton/README.md`

## 安装说明

### 环境要求
- Unity 2022.3或更高版本
- .NET 6或更高版本

### 依赖
- [UniTask](https://github.com/Cysharp/UniTask) - 高性能异步编程库 (v2.5.10)
- [Unity InputSystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/) - 新的输入系统 (v1.7.0)

## 许可证

MIT License

## 作者

- YangLingYun - [GitHub](https://github.com/YangLingCloud)
