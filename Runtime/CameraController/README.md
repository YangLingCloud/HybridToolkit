# CameraController

一个灵活的相机控制系统，支持多种相机模式和相机效果，适用于Unity游戏开发。

## 目录

- [核心特性](#核心特性)
- [快速开始](#快速开始)
- [文件结构](#文件结构)
- [核心概念](#核心概念)
- [使用方法](#使用方法)
- [最佳实践](#最佳实践)

## 核心特性

- **多种相机模式**：
  - 简单跟随相机
  - 动态视角相机
  - 轨道相机
  - 第一人称相机
- **平滑移动**：使用平滑算法实现流畅的相机移动
- **边界控制**：限制相机移动范围，防止穿过地形边界
- **相机碰撞**：防止相机穿过障碍物
- **相机效果**：
  - 相机抖动
  - 相机摇摆
  - 相机震动
  - 相机过渡效果
- **可扩展性**：支持自定义相机模式和效果

## 快速开始

### 环境要求
- Unity 2022.3或更高版本
- .NET 6或更高版本

### 安装步骤
1. 通过Unity Package Manager安装HybridToolkit包
2. 在代码中引用`HybridToolkit.CameraController`命名空间
3. 继承CameraControllerBase类开始使用

### 代码示例

#### 1. 简单跟随相机
```csharp
using HybridToolkit.CameraController;
using UnityEngine;

public class FollowCamera : CameraControllerBase {
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -5);
    [SerializeField] private float smoothSpeed = 0.1f;
    
    protected override void Update() {
        base.Update();
        
        if (target != null) {
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.LookAt(target);
        }
    }
}

// 使用方式：
// 在场景中创建一个空对象，添加FollowCamera脚本，设置target为玩家对象
```

#### 2. 动态视角相机
```csharp
using HybridToolkit.CameraController;
using UnityEngine;

public class DynamicCamera : CameraControllerBase {
    [SerializeField] private Transform target;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 10f;
    [SerializeField] private float rotationSpeed = 2f;
    
    private float currentZoom = 5f;
    private float currentRotation = 0f;
    
    protected override void Update() {
        base.Update();
        
        // 鼠标滚轮控制缩放
        currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        
        // 鼠标右键控制旋转
        if (Input.GetMouseButton(1)) {
            currentRotation += Input.GetAxis("Mouse X") * rotationSpeed;
        }
        
        if (target != null) {
            Quaternion rotation = Quaternion.Euler(45, currentRotation, 0);
            Vector3 position = target.position - (rotation * Vector3.forward * currentZoom);
            
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 5f);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 5f);
        }
    }
}

// 使用方式：
// 在场景中创建一个空对象，添加DynamicCamera脚本，设置target为玩家对象
```

#### 3. 相机碰撞系统
```csharp
using HybridToolkit.CameraController;
using UnityEngine;

public class CollisionCamera : CameraControllerBase {
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -5);
    [SerializeField] private float collisionRadius = 0.5f;
    [SerializeField] private LayerMask collisionMask = -1;
    
    private Vector3 desiredPosition;
    private Vector3 finalPosition;
    
    protected override void Update() {
        base.Update();
        
        if (target != null) {
            desiredPosition = target.position + offset;
            
            // 射线检测碰撞
            RaycastHit hit;
            Vector3 direction = desiredPosition - target.position;
            float distance = direction.magnitude;
            
            if (Physics.SphereCast(target.position, collisionRadius, direction.normalized, out hit, distance, collisionMask)) {
                finalPosition = hit.point - direction.normalized * 0.5f;
            } else {
                finalPosition = desiredPosition;
            }
            
            transform.position = Vector3.Lerp(transform.position, finalPosition, Time.deltaTime * 10f);
            transform.LookAt(target);
        }
    }
}

// 使用方式：
// 在场景中创建一个空对象，添加CollisionCamera脚本，设置target为玩家对象，设置collisionMask为地形层
```

#### 4. 相机摇摆效果
```csharp
using HybridToolkit.CameraController;
using UnityEngine;

public class ShakeCamera : CameraControllerBase {
    [SerializeField] private Transform target;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDuration = 0.5f;
    
    private Vector3 originalPosition;
    private float shakeTimer;
    
    protected override void Start() {
        base.Start();
        originalPosition = transform.localPosition;
    }
    
    protected override void Update() {
        base.Update();
        
        if (shakeTimer > 0) {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
            transform.localPosition = originalPosition + shakeOffset;
            shakeTimer -= Time.deltaTime;
        } else {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * 10f);
        }
        
        if (target != null) {
            transform.LookAt(target);
        }
    }
    
    public void TriggerShake() {
        shakeTimer = shakeDuration;
    }
    
    public void TriggerShake(float intensity, float duration) {
        shakeIntensity = intensity;
        shakeDuration = duration;
        shakeTimer = duration;
    }
}

// 使用方式：
// 在场景中创建一个空对象，添加ShakeCamera脚本，设置target为玩家对象
// 在需要相机摇摆的地方调用：
// FindObjectOfType<ShakeCamera>().TriggerShake();
```

## 文件结构

```
CameraController/
├── CameraControllerBase.cs        # 相机控制器基类
├── FollowCamera.cs                # 简单跟随相机
├── DynamicCamera.cs               # 动态视角相机
├── CollisionCamera.cs             # 相机碰撞系统
├── ShakeCamera.cs                 # 相机摇摆效果
└── README.md                      # 文档
```

## 核心概念

### 1. 相机控制器基类 (CameraControllerBase)

所有相机控制器的基类，提供基础功能：

```csharp
public class MyCamera : CameraControllerBase {
    // 继承CameraControllerBase开始实现自定义相机逻辑
}
```

### 2. 跟随相机 (FollowCamera)

简单跟随目标对象的相机：

```csharp
public class MyFollowCamera : CameraControllerBase {
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -5);
    
    protected override void Update() {
        base.Update();
        
        if (target != null) {
            transform.position = target.position + offset;
            transform.LookAt(target);
        }
    }
}
```

### 3. 动态相机 (DynamicCamera)

支持缩放、旋转等动态操作的相机：

```csharp
public class MyDynamicCamera : CameraControllerBase {
    // 实现动态相机逻辑，支持缩放、旋转等功能
}
```

### 4. 相机碰撞 (CollisionCamera)

防止相机穿过障碍物的相机系统：

```csharp
public class MyCollisionCamera : CameraControllerBase {
    // 实现碰撞检测逻辑，防止相机穿障
}
```

## 使用方法

详细的代码示例请参考[快速开始](#快速开始)章节中的示例。

## 最佳实践

1. **相机性能**：相机更新频率应该根据游戏类型调整
2. **碰撞设置**：合理设置碰撞层，避免不必要的碰撞检测
3. **平滑参数**：调整平滑参数以获得最佳视觉效果
4. **相机状态**：为不同的游戏状态切换不同的相机模式
5. **用户输入**：分离相机控制和游戏控制，避免输入冲突

### 示例场景

#### 第三人称游戏相机

```csharp
public class ThirdPersonCamera : CameraControllerBase {
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float zoomSpeed = 2f;
    
    private float currentRotation = 0f;
    private float currentDistance = 5f;
    private float currentHeight = 2f;
    
    protected override void Update() {
        base.Update();
        
        // 鼠标输入控制相机旋转和缩放
        if (Input.GetMouseButton(1)) {
            currentRotation += Input.GetAxis("Mouse X") * rotationSpeed;
        }
        
        currentDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, 3f, 10f);
        
        if (target != null) {
            // 计算相机位置
            Quaternion rotation = Quaternion.Euler(0, currentRotation, 0);
            Vector3 position = target.position - (rotation * Vector3.forward * currentDistance);
            position.y += currentHeight;
            
            // 应用位置和旋转
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 10f);
            transform.LookAt(target.position + Vector3.up * 1f);
        }
    }
}
```

### 注意事项

1. **相机层级**：确保相机在正确的渲染层级
2. **输入冲突**：避免相机输入与其他系统冲突
3. **性能优化**：使用对象池减少相机创建和销毁
4. **移动端适配**：为移动端设备优化相机控制
5. **视觉反馈**：提供清晰的视觉反馈给用户
6. **边界限制**：设置相机移动边界，防止穿模
7. **状态管理**：管理不同的相机状态和模式
