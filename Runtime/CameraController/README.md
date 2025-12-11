# CameraController

一个基于策略模式的灵活3D相机控制系统，提供手动控制和自动对齐功能，适用于各种Unity项目。

## 核心特性

- **策略模式设计**：通过ICameraMotionStrategy接口支持多种相机运动策略，易于扩展。
- **手动控制**：ManualMotionStrategy提供基于输入的相机平移、旋转和缩放。
- **自动对齐**：AutoAlignStrategy使用动画曲线实现平滑的自动归位功能。
- **可配置设置**：CameraSettings ScriptableObject允许调整灵敏度、限制和默认姿态。
- **平滑过渡**：内置插值和动量模型，提供自然的相机运动。
- **零垃圾回收**：优化性能，减少内存分配。

## 环境要求

- Unity 2022.3 或更高版本
- .NET 6 或更高版本

## 安装步骤

### 从Unity Package Manager安装（推荐）
1. 打开Unity项目
2. 进入 `Window` > `Package Manager`
3. 点击左上角的 `+` 按钮，选择 `Add package from git URL...`
4. 输入以下URL：
   ```
   https://github.com/YangLingCloud/HybridToolkit.git
   ```
5. 点击 `Add` 按钮开始安装

### 手动安装
1. 将CameraController文件夹复制到您的Unity项目中的Assets目录
2. 确保项目中已安装必要的依赖包
3. 在代码中引用相应的命名空间开始使用

## 快速开始

### 1. 基本设置
首先，创建一个CameraSettings资产来配置相机参数：

```csharp
using HybridToolkit.CameraController;
using UnityEngine;

// 在Unity编辑器中，右键创建：Create > HybridToolkit > Camera Settings
// 或通过代码创建：
var settings = ScriptableObject.CreateInstance<CameraSettings>();
settings.defaultPose = new CameraPose(Vector3.zero, Quaternion.identity, 5f);
settings.sensitivity = new Vector2(2f, 2f);
settings.limits = new CameraLimits();
```

### 2. 使用CameraController
将CameraController组件添加到相机对象，并分配设置：

```csharp
using HybridToolkit.CameraController;
using UnityEngine;

public class CameraDemo : MonoBehaviour {
    [SerializeField] private CameraSettings settings;
    private CameraController _cameraController;
    
    private void Start() {
        _cameraController = GetComponent<CameraController>();
        _cameraController.Initialize(settings);
        
        // 切换到手动控制模式
        _cameraController.SwitchToManual();
        
        // 或切换到自动对齐模式
        _cameraController.SwitchToAutoAlign();
    }
    
    private void Update() {
        // CameraController会自动更新当前策略
    }
}
```

### 3. 手动控制示例
ManualMotionStrategy支持鼠标和键盘输入：

```csharp
// CameraController使用ManualMotionStrategy时，可以通过输入控制相机：
// - 鼠标右键拖拽：旋转相机
// - WASD键：平移相机
// - 鼠标滚轮：缩放相机
// 输入映射可在CameraSettings中配置。
```

### 4. 自动对齐示例
AutoAlignStrategy可平滑地将相机移动到目标姿态：

```csharp
// 设置自动对齐的目标姿态
var targetPose = new CameraPose(new Vector3(0, 5, -10), Quaternion.identity, 5f);
_cameraController.AutoAlignTo(targetPose, 2.0f); // 2秒内完成对齐

// 检查是否对齐完成
if (_cameraController.IsAutoAlignFinished) {
    Debug.Log("相机已对齐到目标位置");
}
```

## 文件结构

```
CameraController/
├── CameraController.cs          # 相机控制器主类，管理策略切换和更新
├── CameraSettings.cs            # ScriptableObject配置资产，包含相机参数
├── ICameraMotionStrategy.cs     # 相机运动策略接口
├── ManualMotionStrategy.cs      # 手动控制策略实现
├── AutoAlignStrategy.cs         # 自动对齐策略实现
└── README.md                    # 文档
```

## 核心概念

### 1. 相机控制器 (CameraController)
相机系统的核心管理器，负责初始化设置、策略切换和每帧更新：

```csharp
// 初始化相机控制器
_cameraController.Initialize(settings);

// 切换到手动控制模式
_cameraController.SwitchToManual();

// 切换到自动对齐模式
_cameraController.SwitchToAutoAlign();

// 启动自动对齐到目标姿态
_cameraController.AutoAlignTo(targetPose, duration);
```

### 2. 相机设置 (CameraSettings)
可配置的ScriptableObject资产，包含所有相机参数：
- **默认姿态** (defaultPose): 相机的初始位置、旋转和缩放
- **灵敏度** (sensitivity): 手动控制时的平移和旋转灵敏度
- **限制** (limits): 相机移动的范围限制
- **自动归位** (autoReturn): 自动对齐的相关参数

### 3. 相机运动策略接口 (ICameraMotionStrategy)
所有相机运动策略必须实现的接口：
```csharp
public interface ICameraMotionStrategy {
    CameraPose CalculateNextPose(CameraPose currentPose, CameraSettings settings, float deltaTime);
    bool IsFinished { get; }
}
```

### 4. 手动控制策略 (ManualMotionStrategy)
基于用户输入（鼠标、键盘）的相机控制：
- **平移**: WASD键或鼠标中键拖拽
- **旋转**: 鼠标右键拖拽
- **缩放**: 鼠标滚轮

### 5. 自动对齐策略 (AutoAlignStrategy)
使用动画曲线平滑地将相机移动到目标姿态，支持自定义持续时间和缓动曲线。

## 使用方法

### 1. 创建和配置CameraSettings
在Unity编辑器中创建CameraSettings资产：
1. 右键点击Project窗口
2. 选择 `Create > HybridToolkit > Camera Settings`
3. 调整参数，如默认姿态、灵敏度等

### 2. 设置相机控制器
将CameraController组件添加到相机GameObject，并在脚本中初始化：

```csharp
using HybridToolkit.CameraController;
using UnityEngine;

public class CameraSetup : MonoBehaviour {
    [SerializeField] private CameraSettings settings;
    
    private void Start() {
        var cameraController = GetComponent<CameraController>();
        cameraController.Initialize(settings);
        
        // 默认使用手动控制
        cameraController.SwitchToManual();
    }
}
```

### 3. 控制模式切换
根据游戏状态切换相机模式：

```csharp
// 当玩家需要控制相机时
_cameraController.SwitchToManual();

// 当需要相机自动移动到某个位置时
_cameraController.SwitchToAutoAlign();
_cameraController.AutoAlignTo(targetPose, 1.5f);

// 检查自动对齐是否完成
if (_cameraController.IsAutoAlignFinished) {
    // 对齐完成后的逻辑
}
```

### 4. 扩展自定义策略
实现ICameraMotionStrategy接口创建新的相机运动策略：

```csharp
using HybridToolkit.CameraController;
using UnityEngine;

public class CustomMotionStrategy : ICameraMotionStrategy {
    public bool IsFinished => false; // 根据实际情况返回
    
    public CameraPose CalculateNextPose(CameraPose currentPose, CameraSettings settings, float deltaTime) {
        // 实现自定义运动逻辑
        var newPosition = currentPose.position + Vector3.forward * deltaTime;
        return new CameraPose(newPosition, currentPose.rotation, currentPose.zoom);
    }
}
```

## 最佳实践

1. **性能优化**: CameraController使用值类型和对象池减少垃圾回收，适合高频更新。
2. **输入处理**: 将相机输入与游戏输入分离，避免冲突。
3. **平滑过渡**: 使用AutoAlignStrategy进行场景切换或过场动画中的相机移动。
4. **参数调优**: 在CameraSettings中调整灵敏度和限制，以适应不同游戏类型。
5. **移动端适配**: 为触摸屏设备调整输入映射和灵敏度。
6. **调试辅助**: 使用CameraSettings的调试选项可视化相机边界和运动轨迹。

## 注意事项

- 确保相机GameObject有适当的碰撞体（如果需要碰撞检测）。
- 在VR或AR项目中，可能需要调整输入处理方式。
- 相机控制器依赖于Unity的Input System，确保正确配置输入映射。
- 自动对齐过程中，避免频繁切换策略，以免造成运动不连贯。