# HybridToolkit

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Unity](https://img.shields.io/badge/Unity-2022.3+-blue.svg)](https://unity.com/)
[![.NET](https://img.shields.io/badge/.NET-6+-green.svg)](https://dotnet.microsoft.com/)
[![GitHub Stars](https://img.shields.io/github/stars/YangLingCloud/HybridToolkit?style=social)](https://github.com/YangLingCloud/HybridToolkit)

一个整合多种设计模式和工具类的Unity开发套件，是RookieFramework的核心组件，提供了状态管理、相机控制、事件通信和单例模式等多种实用功能。

## 目录

- [核心特性](#核心特性)
- [快速开始](#快速开始)
- [主要组件](#主要组件)
- [依赖库](#依赖库)
- [API参考](#api参考)
- [常见问题](#常见问题)

## 核心特性

### 游戏逻辑状态管理系统
提供完整的分层状态机实现，用于管理复杂的游戏逻辑流程：
- 支持状态嵌套和层级管理
- 提供状态转换序列控制
- 支持并行和顺序执行的阶段管理
- 灵活的状态机构建器API

### 相机控制组件
实现了基于策略模式的灵活3D相机控制系统：
- **策略模式设计**：通过ICameraMotionStrategy接口支持可扩展的运动策略
- **手动控制策略**：ManualMotionStrategy提供自由旋转、缩放和平移控制
- **自动对齐策略**：AutoAlignStrategy实现基于曲线的平滑自动归位功能
- **可配置相机参数**：CameraSettings ScriptableObject提供灵敏度、限制等配置
- **输入系统集成**：支持Unity Input System和传统输入
- **平滑过渡**：支持手动与自动模式之间的平滑切换
- **零垃圾回收设计**：减少运行时内存分配

### 事件管线系统 (Event Pipeline)
基于UniTask的高性能异步事件管线系统：
- 异步管线，深度集成UniTask，支持async/await
- 零GC设计，核心管线无堆内存分配
- 优先级排序，支持High、Normal、Low等精细控制
- 管线控制，支持事件拦截和中断
- 阶段锁定，基于优先级的自动只读锁定机制
- 混合注册，支持同步和异步方法的混合订阅
- 安全稳健，内置Unity对象生命周期检查

### 设计模式实现
- **单例模式**
  - `SingletonMono<T>` - 基础泛型单例MonoBehaviour基类
  - `PersistentSingletonMono<T>` - 场景切换时不会被销毁的持久化单例
  - `RegulatorSingletonMono<T>` - 通过初始化时间管理多个实例的调节型单例

### 自定义特性
- **InspectorReadOnlyAttribute** - 使字段在Inspector面板中显示为只读
  - 支持各种数据类型（包括基本类型、Vector、数组等）
  - 运行时仍可通过代码修改字段值
  - 通过InspectorReadOnlyDrawer实现绘制逻辑

### 编辑器工具
- **InspectorButton工具** - 在Inspector中为MonoBehaviour方法添加可点击按钮
  - 支持在编辑模式和播放模式下显示按钮
  - 支持自定义按钮名称
  - 支持多种参数类型输入
  - 提供展开/折叠的参数面板

### 输入中心
统一输入管理系统：
- 支持键盘、鼠标、手柄输入
- 触摸手势识别
- 输入映射和重映射
- 输入历史记录和回放

## 快速开始

### 环境要求
- Unity 2022.3或更高版本
- .NET 6或更高版本

### 安装步骤

#### 从Unity Package Manager安装（推荐）
1. 打开Unity项目
2. 进入 `Window` > `Package Manager`
3. 点击左上角的 `+` 按钮，选择 `Add package from git URL...`
4. 输入以下URL：
   ```
   https://github.com/YangLingCloud/HybridToolkit.git
   ```
5. 点击 `Add` 按钮开始安装

#### 手动安装
1. 将HybridToolkit文件夹复制到您的Unity项目中的Assets目录
2. 确保项目中已安装必要的依赖包
3. 在代码中引用相应的命名空间开始使用

### 第一个示例

#### 初始化状态机
```csharp
using HybridToolkit.HierarchicalStateMachine;

public class PlayerController : MonoBehaviour {
    private StateMachine _stateMachine;
    
    private void Awake() {
        _stateMachine = new StateMachine(this);
        var idleState = new PlayerIdleState();
        _stateMachine.SetRootState(idleState);
    }
    
    private void Update() {
        _stateMachine.Update();
    }
    
    private void OnDestroy() {
        _stateMachine?.Clear();
    }
}
```

#### 使用事件通信
```csharp
    /// <summary>
    /// 相机旋转事件结构体，包含旋转差值
    /// </summary>
    public class  CameraRotateEvent : PipelineEvent
    {
        /// <summary>
        /// 旋转差值（X轴旋转相机上下，Y轴旋转相机左右）
        /// </summary>
        public Vector2 delta;
    }

    private void OnEnable()
    {
        EventPipeline<CameraRotateEvent>.Subscribe(OnCameraRotate);
    }

    private void OnDisable()
    {
        EventPipeline<CameraRotateEvent>.Unsubscribe(OnCameraRotate);
    }

 	private void OnCameraRotate(CameraRotateEvent evt){}
```

#### 使用相机控制
```csharp
        private void Awake()
        {
            currentPose = new CameraPose(Settings.DefaultPose);
            _lastInputTime = Time.time;
            
            // 默认使用手动策略
            _activeStrategy = new ManualMotionStrategy(Settings);
            
            UpdateTransform();
        }
```

详细的使用示例请参考各个模块目录下的README文件：
- **状态管理系统**：`Runtime/Hierarchical StateMachine/README.md`
- **相机控制组件**：`Runtime/CameraController/README.md`
- **事件管线系统**：`Runtime/EventPipeline/README.md`
- **单例模式实现**：`Runtime/Singleton/README.md`
- **自定义特性**：`Runtime/CustomAttribute/README.md`
- **编辑器工具**：`Editor/README.md`
- **输入中心**：`Runtime/InputCenter/README.md`

## 主要组件

| 组件名称 | 描述 | 路径 | 特性 |
|---------|------|------|------|
| **分层状态机** | 复杂游戏逻辑状态管理 | `Runtime/Hierarchical StateMachine/` | 状态嵌套、序列控制、阶段管理 |
| **相机控制器** | 基于策略模式的灵活3D相机控制系统 | `Runtime/CameraController/` | 策略模式设计、手动控制、自动对齐、平滑过渡、输入集成 |
| **事件管线** | 高性能异步事件管线系统 | `Runtime/EventPipeline/` | 异步管线、零GC、优先级排序、阶段锁定 |
| **单例模式** | 多种单例模式实现 | `Runtime/Singleton/` | 持久化、调节型、泛型支持 |
| **自定义特性** | Inspector增强工具 | `Runtime/CustomAttribute/` | 只读显示、按钮工具 |
| **编辑器工具** | Inspector按钮等工具 | `Editor/` | 运行时支持、参数面板 |
| **输入中心** | 统一输入管理 | `Runtime/InputCenter/` | 多平台支持、手势识别 |

### 项目结构

```
Assets/HybridToolkit/
├── Runtime/                    # 运行时组件
│   ├── Hierarchical StateMachine/  # 状态机系统
│   ├── CameraController/           # 相机控制
│   ├── EventPipeline/             # 事件管线系统
│   ├── Singleton/                 # 单例模式
│   ├── CustomAttribute/           # 自定义特性
│   ├── InputCenter/               # 输入中心
│   └── com.LingYun.HybridToolkit.asmdef
├── Editor/                     # 编辑器工具
│   ├── InspectorButtonEditor.cs
│   ├── InspectorReadOnlyDrawer.cs
│   └── com.LingYun.HybridToolkit.Editor.asmdef
├── package.json               # 包配置
├── CHANGELOG.md              # 更新日志
└── README.md                 # 项目文档
```

## 依赖库

| 库名称 | 版本 | 描述 | 链接 |
|--------|------|------|------|
| UniTask | v2.5.10 | 高性能异步编程库 | [GitHub](https://github.com/Cysharp/UniTask) |
| Unity InputSystem | v1.7.0 | 新的输入系统 | [Unity Docs](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/) |

## API参考

### 主要命名空间

```csharp
// 状态机相关
using HybridToolkit.HierarchicalStateMachine;

// 事件管线相关
using HybridToolkit.Events;

// 相机控制相关
using HybridToolkit.CameraController;

// 单例模式相关
using HybridToolkit.Singleton;

// 自定义特性和工具类
using HybridToolkit.CustomAttribute;

// 输入中心相关
using HybridToolkit.InputCenter;
```

### 核心类

#### 状态机
- `StateMachine` - 状态机主类
- `State` - 状态基类
- `StateMachineBuilder` - 状态机构建器
- `Sequence` - 状态序列
- `TransitionSequencer` - 转换序列器

#### 事件管线
- `EventPipeline<T>` - 泛型事件管线主类
- `PipelineEvent` - 事件基类
- `EventPriority` - 事件优先级枚举 (High, Normal, Low)

#### 相机控制
- `ICameraMotionStrategy` - 相机运动策略接口
- `ManualMotionStrategy` - 手动控制策略
- `AutoAlignStrategy` - 自动对齐策略
- `CameraController` - 相机控制器主类
- `CameraSettings` - 相机设置ScriptableObject

#### 单例模式
- `SingletonMono<T>` - 基础单例泛型类
- `PersistentSingletonMono<T>` - 持久化单例
- `RegulatorSingletonMono<T>` - 调节型单例

#### 自定义特性
- `InspectorReadOnlyAttribute` - 只读显示特性
- `InspectorButtonAttribute` - Inspector按钮特性

#### 编辑器工具
- `InspectorButtonEditor` - Inspector按钮编辑器
- `InspectorReadOnlyDrawer` - 只读字段绘制器

## 提交规范

请使用[Conventional Commits](https://www.conventionalcommits.org/)格式：

```bash
# 功能新增
git commit -m "feat: add new camera smoothing algorithm"

# 问题修复  
git commit -m "fix: resolve memory leak in event bus"

# 文档更新
git commit -m "docs: update README with new examples"

# 样式调整
git commit -m "style: format code according to style guide"

# 重构代码
git commit -m "refactor: improve event handling performance"

# 测试相关
git commit -m "test: add unit tests for state machine"

# 构建相关
git commit -m "build: update package.json dependencies"
```

### 版本管理
遵循[Semantic Versioning](https://semver.org/)规范：
- **主版本号** (X.y.z)：不兼容的API修改
- **次版本号** (x.Y.z)：向下兼容的功能性新增
- **修订号** (x.y.Z)：向下兼容的问题修正

## 常见问题

### Q: 如何处理状态机的内存泄漏？
A: 确保在MonoBehaviour的OnDestroy方法中调用`stateMachine.Clear()`来清理状态机资源。

```csharp
private void OnDestroy() {
    _stateMachine?.Clear();
}
```

### Q: 相机控制器支持触摸屏操作吗？
A: 支持，相机控制器使用Unity Input System，可以配置触摸屏输入。确保项目中启用了Input System包。

### Q: 如何自定义单例的初始化顺序？
A: 使用`RegulatorSingletonMono<T>`并重写`InitializationTime`属性来控制初始化顺序。

```csharp
public class MyManager : RegulatorSingletonMono<MyManager> {
    public override int InitializationTime => 1; // 优先级越高越早初始化
    
    protected override void Init() {
        base.Init();
        // 初始化逻辑
    }
}
```

### Q: 如何调试状态机？
A: 使用`StateMachineBuilder`来构建状态机时，可以启用调试模式：

```csharp
var builder = new StateMachineBuilder();
builder.EnableDebugMode(true);
var machine = builder.Build();
```

## 作者

- **YangLingYun**  - [GitHub](https://github.com/YangLingCloud)

## 受启发于

### Hierarchical State Machine
直接引用自该仓库和视频
- https://www.youtube.com/watch?v=c-XoTg6Fba4&t=1s
- https://github.com/adammyhre

### 致谢
感谢所有为Unity开源社区做出贡献的开发者们！