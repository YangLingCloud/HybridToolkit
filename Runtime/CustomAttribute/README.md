# 自定义特性 (CustomAttribute)

HybridToolkit中的自定义特性集合，用于增强Unity Inspector的功能和用户体验。

## 特性列表

### InspectorReadOnlyAttribute

使字段在Unity Inspector面板中显示为只读，但在运行时仍可通过代码修改。

#### 使用方法

```csharp
using UnityEngine;
using HybridToolkit;

public class Example : MonoBehaviour
{
    [Header("可编辑字段")]
    public string editableString = "可编辑字符串";
    public int editableInt = 42;
    
    [Header("只读字段")]
    [InspectorReadOnly]
    public string readOnlyString = "只读字符串";
    [InspectorReadOnly]
    public int readOnlyInt = 100;
    
    [InspectorReadOnly]
    public Vector3 readOnlyVector = new Vector3(1, 2, 3);
    
    [InspectorReadOnly]
    public int[] readOnlyArray = new int[] { 1, 2, 3, 4, 5 };
    
    private void Update()
    {
        // 在运行时更新只读字段的值
        readOnlyInt = Time.frameCount;
    }
}
```

#### 特点

- 支持各种数据类型：基本类型（int、float、bool等）、引用类型、数组、结构体（如Vector2、Vector3、Color等）
- 运行时仍可通过代码修改字段值
- 在Inspector中字段显示为灰色，无法直接编辑
- 自动适应Unity默认的属性绘制方式

### InspectorButtonAttribute

在Inspector面板中为MonoBehaviour方法添加可点击按钮，并支持参数输入。

#### 详细说明

请参考编辑器工具文档：`Assets/HybridToolkit/Editor/README.md`

## 技术实现

### InspectorReadOnlyAttribute

- **定义文件**：`InspectorReadOnlyAttribute.cs`
- **绘制器**：`Assets/HybridToolkit/Editor/InspectorReadOnlyDrawer.cs`
- **实现原理**：
  1. 创建继承自`PropertyAttribute`的自定义特性
  2. 创建继承自`PropertyDrawer`的属性绘制器
  3. 在绘制器中控制`GUI.enabled`状态实现只读效果
  4. 使用`EditorGUI.PropertyField`自动处理各种类型的字段绘制

## 许可证

MIT License
