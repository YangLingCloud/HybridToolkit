# 更新日志

本项目所有重要变更都会记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)，并遵循 [Semantic Versioning](https://semver.org/spec/v2.0.0.html) 版本规范。

## 目录

- [最新发布](#最新发布)
- [版本历史](#版本历史)
- [贡献指南](#贡献指南)
- [版本命名规则](#版本命名规则)
- [变更类型说明](#变更类型说明)

## 最新发布

### [0.0.3] - 2025-12-07

#### 新增功能
- **相机系统升级**：实现更自然的动量模型，提供平滑惯性效果
- **输入系统整合**：统一PC端和移动端缩放处理逻辑
- **性能优化**：实现零GC设计，大幅减少垃圾回收开销
- **文档完善**：为所有模块README添加详细更新日志

#### 功能改进
- **相机控制系统**：优化动量算法，提供更自然的相机移动体验
- **输入中心整合**：统一缩放输入处理逻辑，简化API调用
- **事件总线优化**：改进事件对象管理机制，减少内存分配
- **代码结构清理**：移除冗余代码和未使用的文件
- **条件编译优化**：清理旧InputManager相关代码

#### 功能移除
- 移除 `CameraControllerExample.cs` 示例文件
- 移除 `PinchExample.cs` 示例文件  
- 移除 `InputSystem_Actions` 相关配置文件
- 移除 `LICENSE` 文件（移至主仓库）
- 移除 `CustomActions.meta` 元数据文件

#### 技术改进
```csharp
// 新的相机动量模型示例
public class AdvancedCameraController : MonoBehaviour {
    private Vector3 velocity;
    private float momentum = 0.9f;
    
    private void Update() {
        // 平滑惯性移动
        transform.position += velocity * Time.deltaTime;
        velocity *= momentum;
    }
}
```

## 版本历史

### [0.0.2] - 2023-11-15

#### 新增功能
- **完整中文注释**：为所有核心模块添加详细中文文档
  - 状态机系统：`StateMachine`, `State`, `Stage`, `Sequence`
  - 相机控制系统：`CameraController`, `CameraSettings`  
  - 事件总线系统：`EventBus`, `EventBusUtil`
  - 单例模式实现：`SingletonMono`, `PersistentSingletonMono`

#### 功能改进
- **依赖管理**：更新 `package.json`，添加 Unity InputSystem 依赖 (v1.7.0)
- **文档重构**：完善主 README.md 结构
  - 重新组织功能特性介绍
  - 添加各模块详细说明
  - 优化代码示例展示方式
- **版本明确**：指定 UniTask (v2.5.10) 和 Unity InputSystem (v1.7.0) 版本要求

#### 文档更新
```markdown
# 主README结构优化
## 核心特性
## 快速开始  
## 主要组件
### 状态机系统
### 相机控制系统
### 事件总线系统
### 单例模式实现
```

### [0.0.1] - 初始版本

#### 初始功能
- **基础状态机系统**：提供完整的状态机实现
- **相机控制组件**：支持基础相机移动和旋转
- **事件总线系统**：实现松耦合的事件通信机制
- **单例模式实现**：提供多种单例模式变体
- **项目结构**：建立基础项目架构和配置文件

#### 初始架构
```
Assets/HybridToolkit/
├── Runtime/                    # 运行时组件
│   ├── StateMachine/          # 状态机系统
│   ├── CameraController/      # 相机控制
│   ├── EventBus/             # 事件总线
│   └── Singleton/            # 单例模式
├── Editor/                    # 编辑器工具
├── package.json              # 包配置
└── README.md                 # 项目文档
```