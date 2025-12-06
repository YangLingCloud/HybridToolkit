# Input Center

一个集中化的输入管理系统，用于Unity项目中统一处理各种输入事件。

## 目录

- [核心特性](#核心特性)
- [快速开始](#快速开始)
- [文件结构](#文件结构)
- [核心概念](#核心概念)
- [使用方法](#使用方法)
- [最佳实践](#最佳实践)

## 核心特性

- **统一输入管理**：集中处理所有输入事件
- **多种输入支持**：支持键盘、鼠标、手柄、触摸屏
- **输入映射**：可配置的输入映射系统
- **事件驱动**：基于事件的输入处理机制
- **输入缓存**：输入状态缓存和历史记录
- **性能优化**：优化的输入检测和处理
- **多平台支持**：跨平台输入处理
- **可扩展性**：易于扩展新的输入类型和设备

## 快速开始

### 环境要求
- Unity 2022.3或更高版本
- .NET 6或更高版本

### 安装步骤
1. 通过Unity Package Manager安装HybridToolkit包
2. 在代码中引用`HybridToolkit.InputCenter`命名空间
3. 初始化InputCenter管理器

### 代码示例

#### 1. 基本输入管理器

```csharp
using HybridToolkit.InputCenter;
using UnityEngine;

public class BasicInputManager : MonoBehaviour {
    private InputCenter inputCenter;
    
    private void Awake() {
        // 创建输入中心管理器
        inputCenter = new InputCenter();
        
        // 订阅输入事件
        inputCenter.OnKeyDown += HandleKeyDown;
        inputCenter.OnKeyUp += HandleKeyUp;
        inputCenter.OnMouseDown += HandleMouseDown;
        inputCenter.OnTouchStart += HandleTouchStart;
    }
    
    private void Update() {
        // 更新输入中心
        inputCenter.Update();
    }
    
    private void HandleKeyDown(KeyCode key) {
        Debug.Log($"Key pressed: {key}");
    }
    
    private void HandleKeyUp(KeyCode key) {
        Debug.Log($"Key released: {key}");
    }
    
    private void HandleMouseDown(int button) {
        Debug.Log($"Mouse button pressed: {button}");
    }
    
    private void HandleTouchStart(Touch touch) {
        Debug.Log($"Touch started at: {touch.position}");
    }
    
    private void OnDestroy() {
        // 取消订阅
        inputCenter.OnKeyDown -= HandleKeyDown;
        inputCenter.OnKeyUp -= HandleKeyUp;
        inputCenter.OnMouseDown -= HandleMouseDown;
        inputCenter.OnTouchStart -= HandleTouchStart;
        
        // 清理资源
        inputCenter.Dispose();
    }
}
```

#### 2. 输入映射系统

```csharp
using HybridToolkit.InputCenter;
using UnityEngine;

// 输入映射定义
public class InputMapping {
    public string MoveForward = "W";
    public string MoveBackward = "S";
    public string MoveLeft = "A";
    public string MoveRight = "D";
    public string Jump = "Space";
    public string Attack = "LeftMouse";
    public string Interact = "E";
}

public class PlayerInputController : MonoBehaviour {
    [SerializeField] private InputMapping inputMapping;
    private InputCenter inputCenter;
    
    private void Awake() {
        inputCenter = new InputCenter();
        
        // 设置输入映射
        inputCenter.SetMapping(inputMapping);
        
        // 订阅输入事件
        inputCenter.OnActionTriggered += HandleActionTriggered;
    }
    
    private void Update() {
        inputCenter.Update();
        
        // 获取输入状态
        HandleMovement();
        HandleActions();
    }
    
    private void HandleMovement() {
        Vector2 moveInput = Vector2.zero;
        
        if (inputCenter.GetKey(inputMapping.MoveForward)) moveInput.y += 1;
        if (inputCenter.GetKey(inputMapping.MoveBackward)) moveInput.y -= 1;
        if (inputCenter.GetKey(inputMapping.MoveLeft)) moveInput.x -= 1;
        if (inputCenter.GetKey(inputMapping.MoveRight)) moveInput.x += 1;
        
        // 应用移动逻辑
        Debug.Log($"Movement input: {moveInput}");
    }
    
    private void HandleActions() {
        if (inputCenter.GetKeyDown(inputMapping.Jump)) {
            Debug.Log("Jump action triggered");
        }
        
        if (inputCenter.GetKeyDown(inputMapping.Attack)) {
            Debug.Log("Attack action triggered");
        }
        
        if (inputCenter.GetKeyDown(inputMapping.Interact)) {
            Debug.Log("Interact action triggered");
        }
    }
    
    private void HandleActionTriggered(string actionName) {
        Debug.Log($"Action triggered: {actionName}");
    }
    
    private void OnDestroy() {
        inputCenter.OnActionTriggered -= HandleActionTriggered;
        inputCenter.Dispose();
    }
}
```

#### 3. 手柄输入支持

```csharp
using HybridToolkit.InputCenter;
using UnityEngine;

public class GamepadInputController : MonoBehaviour {
    private InputCenter inputCenter;
    
    private void Awake() {
        inputCenter = new InputCenter();
        
        // 订阅手柄事件
        inputCenter.OnGamepadConnected += HandleGamepadConnected;
        inputCenter.OnGamepadDisconnected += HandleGamepadDisconnected;
        inputCenter.OnGamepadButtonDown += HandleGamepadButtonDown;
        inputCenter.OnGamepadAxisChanged += HandleGamepadAxisChanged;
    }
    
    private void Update() {
        inputCenter.Update();
        HandleGamepadInput();
    }
    
    private void HandleGamepadInput() {
        // 获取手柄轴输入
        float leftStickX = inputCenter.GetAxis("LeftStickX");
        float leftStickY = inputCenter.GetAxis("LeftStickY");
        float rightStickX = inputCenter.GetAxis("RightStickX");
        float rightStickY = inputCenter.GetAxis("RightStickY");
        
        Debug.Log($"Left stick: ({leftStickX}, {leftStickY})");
        Debug.Log($"Right stick: ({rightStickX}, {rightStickY})");
        
        // 获取手柄按钮输入
        if (inputCenter.GetGamepadButtonDown("A")) {
            Debug.Log("Gamepad A button pressed");
        }
        
        if (inputCenter.GetGamepadButtonDown("B")) {
            Debug.Log("Gamepad B button pressed");
        }
        
        // 触发器输入
        float leftTrigger = inputCenter.GetAxis("LeftTrigger");
        float rightTrigger = inputCenter.GetAxis("RightTrigger");
        
        if (leftTrigger > 0.5f) {
            Debug.Log("Left trigger pressed");
        }
        
        if (rightTrigger > 0.5f) {
            Debug.Log("Right trigger pressed");
        }
    }
    
    private void HandleGamepadConnected(int playerIndex) {
        Debug.Log($"Gamepad connected for player {playerIndex}");
    }
    
    private void HandleGamepadDisconnected(int playerIndex) {
        Debug.Log($"Gamepad disconnected for player {playerIndex}");
    }
    
    private void HandleGamepadButtonDown(int playerIndex, string buttonName) {
        Debug.Log($"Player {playerIndex} pressed {buttonName}");
    }
    
    private void HandleGamepadAxisChanged(int playerIndex, string axisName, float value) {
        Debug.Log($"Player {playerIndex} axis {axisName} changed to {value}");
    }
    
    private void OnDestroy() {
        inputCenter.OnGamepadConnected -= HandleGamepadConnected;
        inputCenter.OnGamepadDisconnected -= HandleGamepadDisconnected;
        inputCenter.OnGamepadButtonDown -= HandleGamepadButtonDown;
        inputCenter.OnGamepadAxisChanged -= HandleGamepadAxisChanged;
        inputCenter.Dispose();
    }
}
```

#### 4. 输入历史和记录

```csharp
using HybridToolkit.InputCenter;
using UnityEngine;
using System.Collections.Generic;

public class InputRecorder : MonoBehaviour {
    private InputCenter inputCenter;
    private List<InputRecord> inputHistory;
    private bool isRecording = false;
    
    private void Awake() {
        inputCenter = new InputCenter();
        inputHistory = new List<InputRecord>();
        
        // 订阅所有输入事件进行记录
        inputCenter.OnKeyDown += RecordKeyDown;
        inputCenter.OnKeyUp += RecordKeyUp;
        inputCenter.OnMouseDown += RecordMouseDown;
        inputCenter.OnTouchStart += RecordTouchStart;
    }
    
    private void Update() {
        inputCenter.Update();
        
        // 开始/停止记录
        if (inputCenter.GetKeyDown("F9")) {
            ToggleRecording();
        }
        
        // 回放输入
        if (inputCenter.GetKeyDown("F10")) {
            PlaybackInput();
        }
        
        // 清除记录
        if (inputCenter.GetKeyDown("F11")) {
            ClearInputHistory();
        }
    }
    
    private void ToggleRecording() {
        isRecording = !isRecording;
        Debug.Log($"Input recording: {(isRecording ? "Started" : "Stopped")}");
        
        if (!isRecording) {
            Debug.Log($"Recorded {inputHistory.Count} input events");
        }
    }
    
    private void RecordKeyDown(KeyCode key) {
        if (isRecording) {
            inputHistory.Add(new InputRecord {
                Time = Time.time,
                Type = InputEventType.KeyDown,
                Key = key
            });
        }
    }
    
    private void RecordKeyUp(KeyCode key) {
        if (isRecording) {
            inputHistory.Add(new InputRecord {
                Time = Time.time,
                Type = InputEventType.KeyUp,
                Key = key
            });
        }
    }
    
    private void RecordMouseDown(int button) {
        if (isRecording) {
            inputHistory.Add(new InputRecord {
                Time = Time.time,
                Type = InputEventType.MouseDown,
                MouseButton = button
            });
        }
    }
    
    private void RecordTouchStart(Touch touch) {
        if (isRecording) {
            inputHistory.Add(new InputRecord {
                Time = Time.time,
                Type = InputEventType.TouchStart,
                Touch = touch
            });
        }
    }
    
    private void PlaybackInput() {
        if (inputHistory.Count == 0) {
            Debug.Log("No input history to playback");
            return;
        }
        
        Debug.Log($"Playing back {inputHistory.Count} input events");
        
        foreach (var record in inputHistory) {
            switch (record.Type) {
                case InputEventType.KeyDown:
                    inputCenter.SimulateKeyDown(record.Key);
                    break;
                case InputEventType.KeyUp:
                    inputCenter.SimulateKeyUp(record.Key);
                    break;
                case InputEventType.MouseDown:
                    inputCenter.SimulateMouseDown(record.MouseButton);
                    break;
                case InputEventType.TouchStart:
                    inputCenter.SimulateTouchStart(record.Touch);
                    break;
            }
        }
    }
    
    private void ClearInputHistory() {
        inputHistory.Clear();
        Debug.Log("Input history cleared");
    }
    
    private void OnDestroy() {
        inputCenter.OnKeyDown -= RecordKeyDown;
        inputCenter.OnKeyUp -= RecordKeyUp;
        inputCenter.OnMouseDown -= RecordMouseDown;
        inputCenter.OnTouchStart -= RecordTouchStart;
        inputCenter.Dispose();
    }
}

// 输入记录数据结构
public class InputRecord {
    public float Time;
    public InputEventType Type;
    public KeyCode Key;
    public int MouseButton;
    public Touch Touch;
}

public enum InputEventType {
    KeyDown,
    KeyUp,
    MouseDown,
    MouseUp,
    TouchStart,
    TouchEnd,
    GamepadButtonDown,
    GamepadButtonUp,
    GamepadAxisChanged
}
```

## 文件结构

```
InputCenter/
├── InputCenter.cs          # 主输入管理器
├── InputMapping.cs         # 输入映射配置
├── InputRecorder.cs        # 输入记录器
├── InputHistory.cs         # 输入历史记录
├── GamepadInput.cs         # 手柄输入处理
├── TouchInput.cs           # 触摸输入处理
├── KeyboardInput.cs        # 键盘输入处理
├── MouseInput.cs           # 鼠标输入处理
└── README.md               # 文档
```

## 核心概念

### 1. 输入中心管理器

输入系统的核心管理类：

```csharp
public class InputCenter : IDisposable {
    // 事件委托
    public event Action<KeyCode> OnKeyDown;
    public event Action<KeyCode> OnKeyUp;
    public event Action<int> OnMouseDown;
    public event Action<int> OnMouseUp;
    public event Action<Touch> OnTouchStart;
    public event Action<Touch> OnTouchEnd;
    public event Action<string> OnActionTriggered;
    public event Action<int> OnGamepadConnected;
    public event Action<int> OnGamepadDisconnected;
    public event Action<int, string> OnGamepadButtonDown;
    public event Action<int, string, float> OnGamepadAxisChanged;
    
    // 公共方法
    public void Update();
    public void SetMapping(InputMapping mapping);
    public bool GetKey(string actionName);
    public bool GetKeyDown(string actionName);
    public bool GetKeyUp(string actionName);
    public float GetAxis(string axisName);
    public bool GetGamepadButton(string buttonName);
    public bool GetGamepadButtonDown(string buttonName);
    public void Dispose();
}
```

### 2. 输入映射

配置和管理输入映射的工具类：

```csharp
[System.Serializable]
public class InputMapping {
    public string MoveForward = "W";
    public string MoveBackward = "S";
    public string MoveLeft = "A";
    public string MoveRight = "D";
    public string Jump = "Space";
    public string Attack = "LeftMouse";
    public string Interact = "E";
    
    // 映射管理方法
    public void LoadFromFile(string filePath);
    public void SaveToFile(string filePath);
    public void ResetToDefaults();
}
```

### 3. 输入历史记录

记录和回放输入事件的系统：

```csharp
public class InputHistory {
    // 记录管理
    public void Record(InputRecord record);
    public void Clear();
    public List<InputRecord> GetHistory();
    
    // 回放功能
    public void Playback(List<InputRecord> history);
    public void PlaybackFromTime(float startTime);
    public void PlaybackToTime(float endTime);
    
    // 查询功能
    public List<InputRecord> GetRecordsByType(InputEventType type);
    public List<InputRecord> GetRecordsByTimeRange(float startTime, float endTime);
}
```

### 4. 手柄输入处理

处理游戏手柄输入的专用类：

```csharp
public static class GamepadInput {
    // 手柄检测
    public static bool IsGamepadConnected(int playerIndex);
    public static int GetConnectedGamepadCount();
    public static List<int> GetConnectedGamepadIndices();
    
    // 按钮输入
    public static bool GetButton(int playerIndex, string buttonName);
    public static bool GetButtonDown(int playerIndex, string buttonName);
    public static bool GetButtonUp(int playerIndex, string buttonName);
    
    // 轴输入
    public static float GetAxis(int playerIndex, string axisName);
    public static Vector2 GetAxis2D(int playerIndex, string xAxisName, string yAxisName);
    
    // 振动反馈
    public static void SetVibration(int playerIndex, float leftSpeed, float rightSpeed, float duration);
    public static void StopVibration(int playerIndex);
}
```

### 5. 触摸输入处理

处理触摸屏输入的专用类：

```csharp
public static class TouchInput {
    // 触摸检测
    public static int GetTouchCount();
    public static List<Touch> GetAllTouches();
    public static Touch GetTouch(int index);
    
    // 触摸状态
    public static bool IsTouching();
    public static bool IsTouchDown(int fingerId);
    public static bool IsTouchUp(int fingerId);
    public static bool IsTouchHeld(int fingerId);
    
    // 触摸位置
    public static Vector2 GetTouchPosition(int fingerId);
    public static Vector2 GetTouchDeltaPosition(int fingerId);
    public static float GetTouchPressure(int fingerId);
}
```

## 使用方法

详细的代码示例请参考[快速开始](#快速开始)章节中的示例。

## 最佳实践

1. **统一管理**：使用InputCenter统一处理所有输入，避免分散的输入逻辑
2. **事件驱动**：优先使用事件系统处理输入，提高代码可维护性
3. **输入映射**：使用输入映射系统，提高代码的可配置性
4. **性能优化**：合理使用输入缓存，避免重复的输入检测
5. **多平台适配**：考虑不同平台的输入差异，提供适配方案
6. **调试支持**：使用输入记录功能辅助调试和测试
7. **错误处理**：在输入处理中加入适当的错误处理机制
