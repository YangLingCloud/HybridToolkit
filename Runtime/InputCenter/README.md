# InputCenter - 输入中心

## 概述

InputCenter是一个基于Unity新InputSystem的输入管理系统，提供了统一的输入事件处理和管理功能。

## 功能特性

- ✅ 双指缩放支持
- ✅ 基于事件的输入处理
- ✅ 持久化单例设计
- ✅ 易于扩展的架构

## 快速开始

### 1. 安装依赖

确保项目中已安装Unity InputSystem包（v1.7.0或更高版本）。

### 2. 使用双指缩放功能

#### 方法1：使用事件监听（推荐）

```csharp
using HybridToolkit;
using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 0.01f;
    
    private void OnEnable()
    {
        // 注册双指缩放事件
        InputCenter.Instance.OnPinch.AddListener(OnPinchZoom);
    }
    
    private void OnDisable()
    {
        // 取消注册双指缩放事件
        InputCenter.Instance.OnPinch.RemoveListener(OnPinchZoom);
    }
    
    /// <summary>
    /// 双指缩放事件处理函数
    /// </summary>
    /// <param name="midpoint">缩放中点的屏幕坐标</param>
    /// <param name="deltaDistance">缩放的距离变化量（正值放大，负值缩小）</param>
    private void OnPinchZoom(Vector2 midpoint, float deltaDistance)
    {
        // 根据缩放变化量调整相机视野
        Camera.main.fieldOfView = Mathf.Clamp(
            Camera.main.fieldOfView - (deltaDistance * zoomSpeed),
            30f, 90f);
    }
}
```

#### 方法2：直接使用示例脚本

1. 将`PinchExample.cs`脚本附加到任何游戏对象上
2. 在Inspector面板中调整缩放参数
3. 在移动设备或Unity编辑器的触摸模拟模式下测试双指缩放

## 核心组件

### InputCenter

输入中心的核心类，管理所有输入事件和动作。

#### 主要方法和属性：

- `OnPinch`：双指缩放事件，参数为缩放中点和距离变化量
- `IsPinching()`：检查当前是否正在进行双指缩放
- `GetPinchValue()`：获取当前缩放动作的值

### PinchInteraction

自定义的输入交互类，实现双指缩放的检测和处理逻辑。

#### 主要参数：

- `minDeltaThreshold`：触发缩放事件所需的最小距离变化
- `triggerOnStart`：是否在手势开始时触发事件

### PinchExample

双指缩放的示例脚本，演示如何使用InputCenter的缩放功能。

#### 主要参数：

- `pinchSensitivity`：缩放灵敏度
- `minScale`：最小缩放比例
- `maxScale`：最大缩放比例

## 架构设计

InputCenter采用事件驱动的架构设计：

1. **输入层**：基于Unity InputSystem，处理原始触摸输入
2. **交互层**：使用自定义的PinchInteraction检测双指缩放手势
3. **事件层**：通过InputCenter分发缩放事件
4. **应用层**：游戏逻辑监听并响应缩放事件

## 性能优化

- 仅在有触摸输入时处理缩放逻辑
- 使用事件机制避免轮询
- 限制最小和最大缩放范围防止过度计算

## 平台支持

- ✅ iOS
- ✅ Android
- ✅ Unity编辑器（支持触摸模拟）

## 版本要求

- Unity 2020.3或更高版本
- Unity InputSystem 1.7.0或更高版本

## 示例场景

将`PinchExample`脚本附加到场景中的任何对象上，然后在移动设备或编辑器的触摸模拟模式下测试双指缩放功能。

## 扩展开发

您可以通过继承InputCenter或创建自定义的InputInteraction来扩展输入系统的功能。

### 创建自定义输入交互：

```csharp
using UnityEngine.InputSystem;

public class CustomInteraction : IInputInteraction<Vector2>
{
    public void Process(ref InputInteractionContext context)
    {
        // 实现自定义交互逻辑
    }
    
    public void Reset()
    {
        // 重置交互状态
    }
}
```

## 常见问题

### Q: 双指缩放功能在编辑器中不工作？
A: 确保在Project Settings > Input System Package中启用了触摸模拟，或使用移动设备进行测试。

### Q: 如何调整缩放灵敏度？
A: 在PinchExample脚本或您自己的事件处理函数中调整缩放因子。

### Q: 可以同时在多个对象上使用缩放功能吗？
A: 是的，每个对象都可以独立监听OnPinch事件并实现自己的缩放逻辑。

## 更新日志

### v0.0.2
- 新增双指缩放功能
- 创建InputCenter输入管理系统
- 添加PinchExample示例脚本

### v0.0.1
- 初始版本

## 许可证

MIT License
