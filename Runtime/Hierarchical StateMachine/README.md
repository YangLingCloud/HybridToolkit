# Hierarchical State Machine

一个灵活的层次状态机实现，用于管理复杂的状态逻辑和状态转换。

## 功能特点

- **层次结构**：支持嵌套状态，允许创建复杂的状态树
- **状态生命周期**：完整的状态生命周期管理（进入、更新、退出）
- **状态转换**：支持状态间的转换和条件判断
- **多状态并发**：支持同时激活多个状态
- **易于扩展**：可以轻松添加自定义状态类型

## 文件结构

```
Hierarchical State Machine/
├── StateMachine.cs  # 状态机核心类
├── State.cs         # 状态基类
└── README.md        # 文档
```

## 核心概念

### 1. 状态机 (StateMachine)

状态机是整个系统的核心，负责管理状态树和状态转换：

```csharp
public class StateMachine {
    // 状态机的根状态
    public State RootState { get; private set; }
    
    // 当前激活的状态列表
    public List<State> ActiveStates { get; private set; }
    
    // 状态机的所有者（通常是一个MonoBehaviour）
    public MonoBehaviour Owner { get; private set; }
    
    // ... 其他属性和方法
}
```

### 2. 状态 (State)

状态是系统的基本单元，定义了对象在特定条件下的行为：

```csharp
public class State {
    // 状态的名称
    public string StateName { get; protected set; }
    
    // 状态是否活跃
    public bool IsActive { get; private set; }
    
    // 状态的子状态列表
    public List<State> Children { get; private set; }
    
    // 当前活跃的子状态
    public State ActiveChild { get; private set; }
    
    // ... 其他属性和方法
}
```

### 3. 状态生命周期

每个状态都有完整的生命周期：

1. **进入状态**：`EnterState()` 方法在状态激活时调用
2. **更新状态**：`UpdateState()` 方法在状态激活时每帧调用
3. **退出状态**：`ExitState()` 方法在状态停止激活时调用

### 4. 状态转换

状态之间可以进行转换：

```csharp
// 从当前状态转换到新状态
stateMachine.SwitchState(newState);

// 设置根状态
stateMachine.SetRootState(rootState);
```

## 使用方法

### 1. 创建状态机

```csharp
using HybridToolkit.HierarchicalStateMachine;
using UnityEngine;

public class EnemyAI : MonoBehaviour {
    private StateMachine _stateMachine;
    
    private void Awake() {
        // 创建状态机实例
        _stateMachine = new StateMachine(this);
        
        // 设置根状态
        var rootState = new EnemyRootState();
        _stateMachine.SetRootState(rootState);
    }
    
    private void Update() {
        // 更新状态机
        _stateMachine.Update();
    }
}
```

### 2. 定义状态

```csharp
using HybridToolkit.HierarchicalStateMachine;
using UnityEngine;

// 敌人根状态
public class EnemyRootState : State {
    public EnemyRootState() {
        StateName = "EnemyRootState";
        
        // 添加子状态
        Children = new List<State> {
            new EnemyIdleState(),
            new EnemyChaseState(),
            new EnemyAttackState()
        };
        
        // 设置初始活跃子状态
        ActiveChild = Children[0]; // Idle state
    }
    
    public override void EnterState(StateMachine stateMachine) {
        base.EnterState(stateMachine);
        Debug.Log("Entering Enemy Root State");
    }
    
    public override void UpdateState(StateMachine stateMachine) {
        base.UpdateState(stateMachine);
        // 根状态的更新逻辑
    }
    
    public override void ExitState(StateMachine stateMachine) {
        base.ExitState(stateMachine);
        Debug.Log("Exiting Enemy Root State");
    }
}

// 敌人空闲状态
public class EnemyIdleState : State {
    public EnemyIdleState() {
        StateName = "EnemyIdleState";
    }
    
    public override void EnterState(StateMachine stateMachine) {
        base.EnterState(stateMachine);
        Debug.Log("Entering Idle State");
    }
    
    public override void UpdateState(StateMachine stateMachine) {
        base.UpdateState(stateMachine);
        // 空闲状态的更新逻辑
        
        // 例如：如果检测到玩家，切换到追逐状态
        if (PlayerDetected()) {
            var rootState = stateMachine.RootState;
            rootState.SwitchState(rootState.Children[1]); // Chase state
        }
    }
    
    public override void ExitState(StateMachine stateMachine) {
        base.ExitState(stateMachine);
        Debug.Log("Exiting Idle State");
    }
    
    private bool PlayerDetected() {
        // 检测玩家的逻辑
        return false;
    }
}
```

### 3. 使用状态机

```csharp
// 在MonoBehaviour中使用状态机
public class EnemyAI : MonoBehaviour {
    private StateMachine _stateMachine;
    
    private void Awake() {
        _stateMachine = new StateMachine(this);
        
        // 创建并设置根状态
        var rootState = new EnemyRootState();
        _stateMachine.SetRootState(rootState);
    }
    
    private void Update() {
        // 更新状态机
        _stateMachine.Update();
    }
    
    private void OnDestroy() {
        // 清理状态机
        _stateMachine.Clear();
    }
}
```

## API参考

### StateMachine 类

#### 属性

- `RootState`：状态机的根状态
- `ActiveStates`：当前活跃的状态列表
- `Owner`：状态机的所有者

#### 方法

- `StateMachine(MonoBehaviour owner)`：构造函数
- `SetRootState(State root)`：设置根状态
- `Update()`：更新状态机
- `Clear()`：清理状态机

### State 类

#### 属性

- `StateName`：状态的名称
- `IsActive`：状态是否活跃
- `Children`：状态的子状态列表
- `ActiveChild`：当前活跃的子状态

#### 方法

- `EnterState(StateMachine stateMachine)`：进入状态
- `UpdateState(StateMachine stateMachine)`：更新状态
- `ExitState(StateMachine stateMachine)`：退出状态
- `SwitchState(State newState)`：切换到新状态
- `AddChildState(State child)`：添加子状态

## 注意事项

1. 确保在MonoBehaviour的Update方法中调用状态机的Update方法
2. 在销毁对象时调用状态机的Clear方法以避免内存泄漏
3. 可以为不同的对象创建不同的状态机实例
4. 状态名称应该具有描述性，便于调试和维护
5. 可以通过重写State类的方法来实现自定义的状态行为
