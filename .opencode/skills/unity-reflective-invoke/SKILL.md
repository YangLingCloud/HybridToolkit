---
name: unity-reflective-invoke
description: Work with the ReflectiveInvoke system — add [ReflectiveInvoke] attributes, create parameter types, extend the EditorWindow, and write tests for reflection-based method invocation in this Unity project.
---

## What this skill covers

The ReflectiveInvoke system lets developers mark methods with `[ReflectiveInvoke("label")]` and invoke them from a UIToolkit EditorWindow with full parameter editing support.

## Architecture

### Files

| File | Assembly | Purpose |
|------|----------|---------|
| `Runtime/CustomAttribute/ReflectiveInvokeAttribute.cs` | `CustomAttribute` | The `[ReflectiveInvoke]` attribute (namespace `HybridToolkit`) |
| `Runtime/Reflection/MethodReflector.cs` | `com.LingYun.HybridToolkit` | Reflection scanner + data models (namespace `HybridToolkit.Reflection`) |
| `Editor/ReflectiveInvokeWindow.cs` | `com.LingYun.HybridToolkit.Editor` | UIToolkit EditorWindow (namespace `HybridToolkit.Editor`) |

### Assembly references

- `ReflectiveInvokeAttribute` is in the **`CustomAttribute`** asmdef (GUID `6650098a52c822249ac0aaa4e57e314e`). Any assembly that uses it must reference this GUID.
- `MethodReflector` and data models are in **`com.LingYun.HybridToolkit`** (GUID `fbe10999214d6b149a096d22eacfaf73`).
- Test asmdefs must reference **both** GUIDs above plus the Unity test framework GUIDs.

### Namespace resolution

`ReflectiveInvokeAttribute` lives in namespace `HybridToolkit`. From child namespaces like `HybridToolkit.Reflection`, it is accessible without an explicit `using` due to C# namespace resolution rules (the compiler checks parent namespaces).

## How to add a new [ReflectiveInvoke] method

```csharp
using HybridToolkit;

public class MyService
{
    [ReflectiveInvoke("Do Something")]
    public static void DoSomething(int count, string name)
    {
        // Implementation
    }
}
```

The method will automatically appear in the EditorWindow (`HybridToolkit > 反射调用窗口`).

## How to add a complex parameter type

Complex types (classes/structs with public fields) are automatically expanded in the UI:

```csharp
public class AttackConfig
{
    public int Damage;
    public float Range;
    public bool IsCritical;
}

public class CombatService
{
    [ReflectiveInvoke("Execute Attack")]
    public static void Attack(AttackConfig config) { }
}
```

The EditorWindow will show a foldout with `Damage`, `Range`, and `IsCritical` fields.

## Supported parameter types

### Simple types (rendered as single fields)
- Primitives: `int`, `float`, `double`, `long`, `short`, `byte`, `bool`, `string`, `char`
- Unity value types: `Vector2/3/4`, `Vector2Int/3Int`, `Color`, `Rect/RectInt`, `Bounds/BoundsInt`, `Quaternion`
- Enums: any `enum` type
- Unity objects: any `UnityEngine.Object` subclass (`GameObject`, `Transform`, `ScriptableObject`, etc.)

### Complex types (expanded into field editors)
- Any `class` or `struct` with public instance fields
- Fields of the complex type must themselves be supported types

## Key API

### MethodReflector

```csharp
// Scan a single assembly
List<ReflectedTypeInfo> results = MethodReflector.Scan(assembly);

// Scan all loaded assemblies
List<ReflectedTypeInfo> results = MethodReflector.ScanAll();

// Check if a type renders as a single field vs expanded
bool simple = MethodReflector.IsSimpleType(typeof(int)); // true
bool complex = MethodReflector.IsSimpleType(typeof(MyClass)); // false
```

### Data model hierarchy

```
ReflectedTypeInfo
  ├── Type
  └── Methods: List<ReflectedMethodInfo>
        ├── MethodInfo, Label, IsStatic
        └── Parameters: List<ReflectedParameterInfo>
              ├── ParameterType, Name, IsSimpleType
              └── Fields: List<ReflectedFieldInfo>  (only for complex types)
                    ├── FieldInfo, FieldType, Name
```

## EditorWindow behavior

- **Auto-scan on open**: `CreateGUI()` calls `ScanAndBuild()` automatically — no manual refresh button.
- **UIToolkit-only**: Uses `VisualElement`, `Foldout`, `Button`, typed fields (`IntegerField`, `FloatField`, etc.). No IMGUI.
- **Instance methods**: For `UnityEngine.Object` subclasses, an `ObjectField` is shown to select the target. For plain classes, an instance is created via `Activator.CreateInstance`.
- **Execute All**: Each class card has an "Execute All Methods" button.
- **Hover effects**: Buttons use `MouseEnterEvent`/`MouseLeaveEvent` for hover highlighting.

## Writing tests

Tests go in `Assets/Tests/Editor/ReflectiveInvokeTests.cs`. The test asmdef (`HybridToolkit.Tests.Editor`) must reference:

```json
"references": [
    "GUID:fbe10999214d6b149a096d22eacfaf73",
    "GUID:6650098a52c822249ac0aaa4e57e314e",
    "GUID:27619889b8ba8c24980f86f30d6cb2cf",
    "GUID:0acc523941302664db1f4e527237feb3"
]
```

Test pattern:

```csharp
[Test]
public void Scan_FindsMyType()
{
    var results = MethodReflector.Scan(typeof(MyType).Assembly);
    var found = results.Find(t => t.Type == typeof(MyType));
    Assert.IsNotNull(found);
    Assert.AreEqual(expectedCount, found.Methods.Count);
}
```

## Coding conventions

- XML doc comments in **Chinese**
- Use `#region` for logical grouping
- Use indexed `for` loops, not `foreach`
- Error logging: `Debug.LogError($"[ReflectiveInvoke] message: {e.Message}")`
- Namespace: attributes in `HybridToolkit`, reflection in `HybridToolkit.Reflection`, editor in `HybridToolkit.Editor`
