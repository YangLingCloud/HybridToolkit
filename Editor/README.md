# InspectorButton 编辑器工具

## 功能介绍

InspectorButton是一个Unity编辑器工具，允许在MonoBehaviour中任何方法上添加自定义特性，使其在Inspector面板中显示为可点击的按钮，并支持输入参数。

## 使用方法

### 1. 添加特性

在任何MonoBehaviour类的方法上添加`[InspectorButton]`特性：

```csharp
using UnityEngine;
using HybridToolkit;

public class MyScript : MonoBehaviour
{
    [InspectorButton("执行方法")]
    private void MyMethod(int count, float value, string message)
    {
        Debug.Log($"count: {count}, value: {value}, message: {message}");
    }
}
```

### 2. 特性参数

`InspectorButton`特性支持以下参数：

| 参数名 | 类型 | 默认值 | 描述 |
|--------|------|--------|------|
| buttonName | string | 方法名 | 按钮显示的名称 |
| showInPlayMode | bool | true | 是否在播放模式下显示按钮 |
| showInEditMode | bool | false | 是否在编辑模式下显示按钮 |

### 3. 支持的参数类型

目前支持以下参数类型：
- int
- float
- bool
- string
- Vector2
- Vector3
- Vector4
- Color
- GameObject
- Component及其实子类

## 示例

```csharp
using UnityEngine;
using HybridToolkit;

public class Example : MonoBehaviour
{
    // 无参数方法
    [InspectorButton("测试无参数方法", showInPlayMode = true, showInEditMode = true)]
    private void TestNoParameters()
    {
        Debug.Log("调用了无参数方法");
    }
    
    // 基本参数方法
    [InspectorButton("测试基本参数")]
    private void TestBasicParameters(int count, float value, bool flag)
    {
        Debug.Log($"count: {count}, value: {value}, flag: {flag}");
    }
    
    // Vector参数方法
    [InspectorButton("测试Vector3")]
    private void TestVectorParameters(Vector3 position, Color color)
    {
        Debug.Log($"position: {position}, color: {color}");
    }
}
```

## 注意事项

1. 方法必须是实例方法（非静态方法）
2. 方法可以是私有的（private）或公共的（public）
3. 在Inspector中点击按钮会执行选中的所有对象的对应方法
4. 如果方法有参数，会显示展开/折叠按钮来显示参数输入框

## 技术实现

- `InspectorButtonAttribute`: 自定义特性，用于标记需要在Inspector中显示按钮的方法
- `InspectorButtonEditor`: 自定义编辑器，继承自`Editor`，用于在Inspector中绘制按钮和参数输入框
- 利用反射获取所有带有`InspectorButtonAttribute`特性的方法
- 支持多种参数类型的输入控件绘制
