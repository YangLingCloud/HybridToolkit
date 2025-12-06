# Hierarchical StateMachine

基于分层状态机的行为模式实现，用于Unity应用中复杂状态管理和状态转换。

## 目录

- [核心特性](#核心特性)
- [快速开始](#快速开始)
- [文件结构](#文件结构)
- [核心概念](#核心概念)
- [使用方法](#使用方法)
- [最佳实践](#最佳实践)

## 核心特性

- **分层状态机**：支持状态嵌套和继承关系
- **状态转换**：基于事件的状态转换机制
- **并发状态**：支持多个并行状态同时运行
- **调试工具**：内置状态机可视化调试工具
- **模块化设计**：状态机可组合和重用
- **性能优化**：优化的状态转换算法
- **事件驱动**：基于事件的状态转换
- **动画集成**：与Unity动画系统无缝集成

## 快速开始

### 环境要求
- Unity 2022.3或更高版本
- .NET 6或更高版本

### 安装步骤
1. 通过Unity Package Manager安装HybridToolkit包
2. 在代码中引用`HybridToolkit.HierarchicalStateMachine`命名空间
3. 继承相应的状态机基类开始使用

### 代码示例

#### 1. 简单状态机

```csharp
using HybridToolkit.HierarchicalStateMachine;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private StateMachine stateMachine;
    
    private void Awake() {
        stateMachine = new StateMachine();
        
        // 创建状态
        var idleState = new PlayerIdleState("Idle");
        var runState = new PlayerRunState("Run");
        var jumpState = new PlayerJumpState("Jump");
        var attackState = new PlayerAttackState("Attack");
        
        // 添加状态到状态机
        stateMachine.AddState(idleState);
        stateMachine.AddState(runState);
        stateMachine.AddState(jumpState);
        stateMachine.AddState(attackState);
        
        // 定义状态转换
        stateMachine.AddTransition(idleState, runState, "Move");
        stateMachine.AddTransition(runState, idleState, "Stop");
        stateMachine.AddTransition(idleState, jumpState, "Jump");
        stateMachine.AddTransition(runState, jumpState, "Jump");
        stateMachine.AddTransition(jumpState, idleState, "Landed");
        stateMachine.AddTransition(idleState, attackState, "Attack");
        stateMachine.AddTransition(runState, attackState, "Attack");
        
        // 设置初始状态
        stateMachine.SetInitialState(idleState);
    }
    
    private void Update() {
        // 更新状态机
        stateMachine.Update();
        
        // 处理输入事件
        if (Input.GetKeyDown(KeyCode.Space)) {
            stateMachine.SendEvent("Jump");
        } else if (Input.GetKeyDown(KeyCode.Mouse0)) {
            stateMachine.SendEvent("Attack");
        } else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
            stateMachine.SendEvent("Move");
        } else {
            stateMachine.SendEvent("Stop");
        }
    }
}
```

#### 2. 分层状态机

```csharp
using HybridToolkit.HierarchicalStateMachine;
using UnityEngine;

public class EnemyController : MonoBehaviour {
    private StateMachine stateMachine;
    
    private void Awake() {
        stateMachine = new StateMachine();
        
        // 创建父状态
        var aliveState = new EnemyAliveState("Alive");
        var deadState = new EnemyDeadState("Dead");
        
        // 创建子状态
        var patrolState = new EnemyPatrolState("Patrol", aliveState);
        var chaseState = new EnemyChaseState("Chase", aliveState);
        var attackState = new EnemyAttackState("Attack", aliveState);
        
        // 添加状态
        stateMachine.AddState(aliveState);
        stateMachine.AddState(deadState);
        stateMachine.AddState(patrolState);
        stateMachine.AddState(chaseState);
        stateMachine.AddState(attackState);
        
        // 设置初始状态
        stateMachine.SetInitialState(aliveState);
    }
    
    private void Update() {
        stateMachine.Update();
    }
    
    public void PlayerDetected() {
        stateMachine.SendEvent("PlayerDetected");
    }
    
    public void PlayerLost() {
        stateMachine.SendEvent("PlayerLost");
    }
}
```

#### 3. 并发状态机

```csharp
using HybridToolkit.HierarchicalStateMachine;
using UnityEngine;

public class NPCController : MonoBehaviour {
    private StateMachine mainStateMachine;
    private StateMachine animationStateMachine;
    private StateMachine interactionStateMachine;
    
    private void Awake() {
        // 主状态机
        mainStateMachine = new StateMachine();
        var idleMainState = new NPCIdleState("Idle");
        var workingMainState = new NPCWorkingState("Working");
        mainStateMachine.AddState(idleMainState);
        mainStateMachine.AddState(workingMainState);
        mainStateMachine.SetInitialState(idleMainState);
        
        // 动画状态机
        animationStateMachine = new StateMachine();
        var idleAnimState = new AnimationIdleState("Idle");
        var walkAnimState = new AnimationWalkState("Walk");
        var workAnimState = new AnimationWorkState("Work");
        animationStateMachine.AddState(idleAnimState);
        animationStateMachine.AddState(walkAnimState);
        animationStateMachine.AddState(workAnimState);
        animationStateMachine.SetInitialState(idleAnimState);
        
        // 交互状态机
        interactionStateMachine = new StateMachine();
        var normalInteractionState = new NormalInteractionState("Normal");
        var talkingInteractionState = new TalkingInteractionState("Talking");
        interactionStateMachine.AddState(normalInteractionState);
        interactionStateMachine.AddState(talkingInteractionState);
        interactionStateMachine.SetInitialState(normalInteractionState);
    }
    
    private void Update() {
        // 更新所有状态机
        mainStateMachine.Update();
        animationStateMachine.Update();
        interactionStateMachine.Update();
    }
}
```

## 文件结构

```
Hierarchical StateMachine/
├── StateMachine.cs                 # 基础状态机类
├── State.cs                        # 状态基类
├── StateTransition.cs              # 状态转换类
├── TransitionCondition.cs          # 转换条件类
├── StateMachineDebugger.cs         # 调试工具
└── README.md                       # 文档
```

## 核心概念

### 1. 状态机基础

状态机是状态和状态转换的集合：

```csharp
public class GameStateMachine : StateMachine {
    public GameStateMachine() {
        // 创建状态
        var menuState = new GameMenuState("Menu");
        var playingState = new GamePlayingState("Playing");
        var pausedState = new GamePausedState("Paused");
        var gameOverState = new GameOverState("GameOver");
        
        // 添加状态
        AddState(menuState);
        AddState(playingState);
        AddState(pausedState);
        AddState(gameOverState);
        
        // 设置初始状态
        SetInitialState(menuState);
    }
}
```

### 2. 状态转换

状态之间的转换由事件触发：

```csharp
// 添加状态转换
stateMachine.AddTransition(fromState, toState, "EventName");

// 发送事件
stateMachine.SendEvent("EventName");
```

### 3. 分层状态

状态可以有父状态和子状态：

```csharp
// 创建父状态
var aliveState = new EnemyAliveState("Alive");

// 创建子状态（继承父状态）
var patrolState = new EnemyPatrolState("Patrol", aliveState);
var chaseState = new EnemyChaseState("Chase", aliveState);
```

### 4. 条件转换

基于条件的转换：

```csharp
// 添加条件转换
stateMachine.AddTransition(fromState, toState, "EventName", condition);
```

## 使用方法

详细的代码示例请参考[快速开始](#快速开始)章节中的示例。

## 最佳实践

1. **状态命名**：使用清晰的状态名称，避免歧义
2. **状态职责**：每个状态应该有明确的职责和边界
3. **转换逻辑**：保持状态转换逻辑简单明了
4. **条件使用**：使用条件转换处理复杂的状态变化
5. **调试工具**：使用内置调试工具可视化状态机运行
6. **性能考虑**：避免在Update方法中执行复杂的计算
7. **状态层次**：合理设计状态层次，避免过度嵌套

### 注意事项

1. **初始状态**：必须设置状态机的初始状态
2. **状态转换**：避免在状态转换过程中产生死循环
3. **事件处理**：确保发送的事件与状态转换定义的事件名称匹配
4. **状态更新**：在Update方法中更新状态机
5. **内存管理**：在场景切换时正确清理状态机
6. **调试模式**：在开发阶段启用调试模式检查状态机状态
7. **并发状态**：并发状态机的更新顺序可能影响结果
8. **转换条件**：确保转换条件返回布尔值
