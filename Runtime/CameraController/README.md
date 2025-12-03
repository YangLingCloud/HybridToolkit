# Camera Controller

一个功能完善的Unity相机控制器，支持观察目标、角度控制、自动归位和平滑停止效果。

## 功能特点

1. **观察目标系统**：可以设置相机观察的目标对象
2. **角度控制**：通过Pitch（俯仰角）和Yaw（偏航角）控制相机朝向
3. **距离控制**：可以调整相机与观察目标的距离
4. **自动归位**：支持同步和异步归位功能，使用UniTask实现异步操作
5. **平滑停止**：鼠标/手指操作结束后，相机有自然的逐渐停止效果
6. **配置灵活**：通过ScriptableObject统一管理相机属性

## 文件结构

```
CameraController/
├── CameraSettings.cs         # 相机设置（ScriptableObject）
├── CameraController.cs       # 相机控制器主类
├── CameraControllerExample.cs # 使用示例
└── README.md                 # 文档
```

## 使用方法

### 1. 创建相机设置

1. 在Project窗口中右键点击
2. 选择 `Create > HybridToolkit > Camera > CameraSettings`
3. 在Inspector窗口中调整相机参数

### 2. 设置相机控制器

1. 创建一个空对象，命名为`CameraController`
2. 将`CameraController.cs`脚本挂载到该对象上
3. 配置以下参数：
   - `Settings`：选择之前创建的CameraSettings对象
   - `Target Camera`：选择要控制的相机
   - `Look At Target`：选择相机观察的目标对象
   - `Rotation Input`：设置旋转输入（如鼠标拖动）
   - `Zoom Input`：设置缩放输入（如鼠标滚轮）

### 3. 输入设置

需要使用Unity的Input System设置输入动作：

1. 创建一个Input Actions文件
2. 添加以下动作：
   - `Rotation`：类型为`Vector2`，绑定到鼠标拖动或触摸屏
   - `Zoom`：类型为`Value`（浮点数），绑定到鼠标滚轮或触摸捏合

### 4. 代码示例

```csharp
using HybridToolkit.CameraController;
using UnityEngine;

public class CameraExample : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    
    private void Start()
    {
        // 初始化相机
        cameraController.Initialize();
        
        // 设置观察目标
        cameraController.LookAtTarget = GameObject.Find("Player").transform;
    }
    
    public void OnResetCamera()
    {
        // 使用UniTask异步归位
        cameraController.ResetCamera(true);
    }
    
    public void OnSetCustomAngle()
    {
        // 设置自定义角度
        cameraController.SetCameraAngles(60f, 45f, 15f);
    }
}
```

## API参考

### CameraController

#### 属性

- `LookAtTarget`：获取或设置相机观察的目标对象
- `CurrentPitch`：当前俯仰角（度）
- `CurrentYaw`：当前偏航角（度）
- `CurrentDistance`：当前相机与目标的距离

#### 方法

- `Initialize()`：初始化相机设置
- `ResetCamera(bool useUniTask = true, float? targetPitchOverride = null, float? targetYawOverride = null, float? targetDistanceOverride = null)`：
  自动归位相机
  - `useUniTask`：是否使用UniTask进行异步归位
  - `targetPitchOverride`：自定义目标俯仰角（可选）
  - `targetYawOverride`：自定义目标偏航角（可选）
  - `targetDistanceOverride`：自定义目标距离（可选）
  
- `SetCameraAngles(float pitch, float yaw, float distance)`：直接设置相机角度和距离

### CameraSettings

#### 参数

- **默认参数**：
  - `Default Pitch`：默认俯仰角
  - `Default Yaw`：默认偏航角
  - `Default Distance`：默认距离
  
- **角度限制**：
  - `Min Pitch`：最小俯仰角
  - `Max Pitch`：最大俯仰角
  
- **距离限制**：
  - `Min Distance`：最小距离
  - `Max Distance`：最大距离
  
- **灵敏度**：
  - `Rotation Sensitivity`：旋转灵敏度
  - `Zoom Sensitivity`：缩放灵敏度
  
- **阻尼**：
  - `Rotation Damping`：旋转阻尼（用于平滑停止效果）
  - `Zoom Damping`：缩放阻尼（用于平滑停止效果）
  
- **归位设置**：
  - `Reset Speed`：归位速度

## 注意事项

1. 确保已安装Unity Input System和UniTask包
2. Pitch=90度时，相机将垂直向下指向观察目标
3. 使用异步归位时，系统会自动处理任务取消
4. 平滑停止效果通过阻尼参数控制，可以根据需要调整

## 版本要求

- Unity 2022.3或更高版本
- Unity Input System 1.0或更高版本
- UniTask 2.0或更高版本
