# Reflection（反射调用系统）

基于反射的方法扫描与调用系统，用于在编辑器窗口中可视化执行标记了 `[ReflectiveInvoke]` 特性的方法。

## 核心特性

- **方法扫描**：扫描程序集中所有标记了 `[ReflectiveInvoke]` 的方法
- **参数类型反射**：获取方法参数类型及参数类型内部的公共字段信息
- **简单/复杂类型区分**：自动判断参数是否为简单类型（直接渲染输入控件）或复杂类型（展开字段级别编辑）
- **异常安全**：处理 `ReflectionTypeLoadException`，跳过无法加载的类型

## 文件结构

```
Reflection/
├── MethodReflector.cs    # 反射扫描器 + 数据模型
└── README.md             # 文档
```

## 数据模型

```
ReflectedTypeInfo              # 包含标记方法的类
├── Type                        # 类类型
└── Methods                     # 标记方法列表
    └── ReflectedMethodInfo     # 单个方法信息
        ├── MethodInfo          # 方法反射信息
        ├── Label               # 显示标签
        ├── IsStatic            # 是否静态
        └── Parameters          # 参数列表
            └── ReflectedParameterInfo   # 单个参数信息
                ├── ParameterType        # 参数类型
                ├── Name                 # 参数名
                ├── IsSimpleType         # 是否简单类型
                └── Fields               # 公共字段列表（仅复杂类型）
                    └── ReflectedFieldInfo
                        ├── FieldInfo    # 字段反射信息
                        ├── FieldType    # 字段类型
                        └── Name         # 字段名
```

## 快速开始

### 1. 标记方法

```csharp
using HybridToolkit;

public class GameService
{
    [ReflectiveInvoke("重置分数")]
    public static void ResetScore(int defaultScore)
    {
        // 实现
    }
}
```

### 2. 扫描方法

```csharp
using HybridToolkit.Reflection;

// 扫描指定程序集
var results = MethodReflector.Scan(typeof(GameService).Assembly);

// 扫描所有已加载程序集
var allResults = MethodReflector.ScanAll();

// 扫描单个类型
var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
var typeInfo = MethodReflector.ScanType(typeof(GameService), flags);
```

### 3. 使用复杂参数类型

复杂类型（类或结构体）的公共实例字段会被自动反射：

```csharp
public class SpawnConfig
{
    public Vector3 Position;
    public float Radius;
    public int Count;
}

public class SpawnService
{
    [ReflectiveInvoke("生成敌人")]
    public static void SpawnEnemies(SpawnConfig config)
    {
        // config.Position, config.Radius, config.Count 均可在编辑器中填写
    }
}
```

## 支持的类型

### 简单类型（IsSimpleType = true）

| 分类 | 类型 |
|------|------|
| 基元类型 | `int`, `float`, `double`, `long`, `short`, `byte`, `bool`, `string`, `char`, `uint`, `ulong`, `ushort`, `sbyte`, `decimal` |
| Unity 值类型 | `Vector2/3/4`, `Vector2Int/3Int`, `Color`, `Rect/RectInt`, `Bounds/BoundsInt`, `Quaternion` |
| 枚举 | 任意 `enum` 类型 |
| Unity 对象 | 任意 `UnityEngine.Object` 子类 |

### 复杂类型（IsSimpleType = false）

任何不属于上述简单类型的类或结构体。系统会反射其 `BindingFlags.Instance | BindingFlags.Public` 字段。

## API 参考

### MethodReflector

| 方法 | 描述 |
|------|------|
| `IsSimpleType(Type type)` | 判断类型是否为简单类型 |
| `Scan(Assembly assembly)` | 扫描指定程序集 |
| `ScanAll()` | 扫描所有已加载程序集 |
| `ScanType(Type type, BindingFlags flags)` | 扫描单个类型，无标记方法时返回 null |

## 与编辑器窗口的关系

`MethodReflector` 是纯运行时代码，不依赖 `UnityEditor`。编辑器窗口 `ReflectiveInvokeWindow`（位于 `Editor/`）调用 `MethodReflector.ScanAll()` 获取反射数据，然后构建 UIToolkit UI。

## 注意事项

1. **程序集定义**：`MethodReflector.cs` 属于 `com.LingYun.HybridToolkit` 程序集
2. **特性程序集**：`ReflectiveInvokeAttribute` 属于 `CustomAttribute` 程序集，使用时需确保 asmdef 引用正确
3. **命名空间**：数据模型和扫描器均在 `HybridToolkit.Reflection` 命名空间下
4. **性能**：`ScanAll()` 会遍历所有已加载程序集，建议仅在编辑器窗口打开时调用，而非每帧调用
