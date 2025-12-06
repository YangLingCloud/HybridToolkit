# HybridToolkit Editor Tools

Unity编辑器扩展工具集合，提供Inspector界面增强和编辑器功能扩展。

## 目录

- [核心特性](#核心特性)
- [快速开始](#快速开始)
- [文件结构](#文件结构)
- [核心组件](#核心组件)
- [使用方法](#使用方法)
- [最佳实践](#最佳实践)

## 核心特性

- **Inspector增强**：扩展Unity Inspector面板功能
- **按钮工具**：在Inspector中创建可点击的方法按钮
- **只读绘制器**：提供Inspector属性只读显示功能
- **参数支持**：支持多种参数类型的按钮配置
- **模式控制**：支持播放模式和编辑模式的独立控制
- **批量操作**：支持对多个对象同时执行方法
- **反射技术**：基于反射的动态方法调用
- **易于扩展**：模块化设计，易于添加新的编辑器工具

## 快速开始

### 安装步骤
1. 通过Unity Package Manager安装HybridToolkit包
2. 在代码中引用相应的命名空间
3. 在MonoBehaviour类中使用编辑器特性

### 代码示例

#### 1. InspectorButton 工具使用

```csharp
using UnityEngine;
using HybridToolkit;

public class InspectorButtonExample : MonoBehaviour
{
    // 无参数按钮
    [InspectorButton("执行方法", showInPlayMode = true, showInEditMode = true)]
    private void TestNoParameters()
    {
        Debug.Log("调用了无参数方法");
    }
    
    // 基本参数按钮
    [InspectorButton("测试基本参数", showInPlayMode = true, showInEditMode = false)]
    private void TestBasicParameters(int count, float value, bool flag)
    {
        Debug.Log($"count: {count}, value: {value}, flag: {flag}");
    }
    
    // Vector参数按钮
    [InspectorButton("测试Vector3", showInPlayMode = true, showInEditMode = true)]
    private void TestVectorParameters(Vector3 position, Color color)
    {
        Debug.Log($"position: {position}, color: {color}");
    }
    
    // GameObject参数按钮
    [InspectorButton("创建对象", showInPlayMode = true, showInEditMode = true)]
    private void CreateGameObject(GameObject prefab, int count, string name)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.name = $"{name}_{i}";
        }
    }
}
```

#### 2. InspectorReadOnlyDrawer 使用

```csharp
using UnityEngine;
using HybridToolkit.Editor;

public class ReadOnlyExample : MonoBehaviour
{
    [ReadOnlyInspector]
    public string readonlyString = "这是一个只读字符串";
    
    [ReadOnlyInspector]
    public int readonlyInt = 42;
    
    [ReadOnlyInspector]
    public Vector3 readonlyVector = new Vector3(1, 2, 3);
    
    [ReadOnlyInspector]
    public GameObject readonlyGameObject;
    
    // 混合使用：既有可编辑也有只读属性
    public string editableString = "这是可编辑的字符串";
    
    [ReadOnlyInspector]
    public float readonlyFloat = 3.14f;
}
```

#### 3. 完整的编辑器工具示例

```csharp
using UnityEngine;
using UnityEditor;
using HybridToolkit;
using HybridToolkit.Editor;

public class EditorToolsDemo : MonoBehaviour
{
    [Header("测试数据")]
    [InspectorButton("重置数据", showInPlayMode = true, showInEditMode = true)]
    [SerializeField] private int testCount = 10;
    
    [SerializeField] private float testValue = 1.0f;
    
    [Header("对象操作")]
    [InspectorButton("清理子对象", showInPlayMode = true, showInEditMode = false)]
    [InspectorButton("激活对象", showInPlayMode = true, showInEditMode = true)]
    [SerializeField] private GameObject targetObject;
    
    [Header("批量操作")]
    [InspectorButton("批量设置标签", showInPlayMode = true, showInEditMode = false)]
    [SerializeField] private string newTag = "NewTag";
    
    [ReadOnlyInspector]
    public string runtimeInfo = "运行时信息";
    
    private void TestNoParameters()
    {
        Debug.Log("执行了测试方法");
    }
    
    private void ResetData()
    {
        testCount = 10;
        testValue = 1.0f;
        Debug.Log("数据已重置");
    }
    
    private void CleanChildren(GameObject parent, bool includeInactive)
    {
        Transform parentTransform = parent.transform;
        for (int i = parentTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = parentTransform.GetChild(i);
            if (includeInactive || child.gameObject.activeInHierarchy)
            {
                DestroyImmediate(child.gameObject);
            }
        }
        Debug.Log("子对象清理完成");
    }
    
    private void SetActive(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
            Debug.Log($"对象 {target.name} 状态设置为: {active}");
        }
    }
    
    private void BatchSetTag(GameObject[] targets, string tag)
    {
        foreach (var target in targets)
        {
            if (target != null)
            {
                target.tag = tag;
            }
        }
        Debug.Log($"批量设置标签完成，共处理 {targets.Length} 个对象");
    }
}
```

#### 4. 自定义属性绘制器

```csharp
using UnityEngine;
using UnityEditor;
using HybridToolkit.Editor;

// 自定义InspectorReadOnlyDrawer扩展
[CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
public class ReadOnlyInspectorDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}

// 自定义InspectorButton绘制器
[CustomPropertyDrawer(typeof(InspectorButtonAttribute))]
public class InspectorButtonDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var buttonAttr = (InspectorButtonAttribute)attribute;
        
        if (GUI.Button(position, buttonAttr.buttonName))
        {
            // 执行按钮方法
            ExecuteMethod(property.serializedObject.targetObject, buttonAttr);
        }
    }
    
    private void ExecuteMethod(UnityEngine.Object target, InspectorButtonAttribute attribute)
    {
        var method = target.GetType().GetMethod(attribute.methodName, 
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        
        if (method != null)
        {
            method.Invoke(target, null);
        }
        else
        {
            Debug.LogError($"方法 {attribute.methodName} 在 {target.GetType().Name} 中未找到");
        }
    }
}
```

## 文件结构

```
Editor/
├── InspectorButtonEditor.cs       # InspectorButton编辑器实现
├── InspectorReadOnlyDrawer.cs     # 只读属性绘制器
├── InspectorButtonAttribute.cs    # 按钮特性定义
├── ReadOnlyInspectorAttribute.cs  # 只读特性定义
├── EditorGUIExtensions.cs         # 编辑器GUI扩展工具
└── README.md                      # 文档
```

## 核心组件

### 1. InspectorButton 工具

在Inspector中显示可点击的方法按钮：

```csharp
[System.AttributeUsage(System.AttributeTargets.Method)]
public class InspectorButtonAttribute : System.Attribute
{
    public string buttonName;
    public bool showInPlayMode = true;
    public bool showInEditMode = false;
    
    public InspectorButtonAttribute(string buttonName)
    {
        this.buttonName = buttonName;
    }
}
```

**主要特性：**
- 支持无参数和有参数方法
- 可配置显示模式（播放模式/编辑模式）
- 支持多种参数类型
- 支持批量操作

### 2. InspectorButtonEditor

自定义编辑器实现InspectorButton功能：

```csharp
[CustomEditor(typeof(MonoBehaviour), true)]
public class InspectorButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DrawInspectorButtons();
    }
    
    private void DrawInspectorButtons()
    {
        var methods = target.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(m => m.GetCustomAttribute<InspectorButtonAttribute>() != null);
            
        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<InspectorButtonAttribute>();
            
            if (ShouldShowButton(attribute))
            {
                if (GUILayout.Button(attribute.buttonName))
                {
                    ExecuteMethod(method);
                }
            }
        }
    }
}
```

### 3. ReadOnlyInspectorDrawer

提供Inspector属性只读显示功能：

```csharp
[CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
public class ReadOnlyInspectorDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
```

### 4. EditorGUIExtensions

编辑器GUI扩展工具类：

```csharp
public static class EditorGUIExtensions
{
    public static void DrawReadOnlyLabel(string label, string value)
    {
        EditorGUILayout.LabelField(label, value, EditorStyles.textField);
    }
    
    public static void DrawInspectorButton(string buttonName, Action action)
    {
        if (GUILayout.Button(buttonName))
        {
            action?.Invoke();
        }
    }
    
    public static void DrawSeparator()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
    }
}
```

## 使用方法

详细的代码示例请参考[快速开始](#快速开始)章节中的示例。

## 最佳实践

1. **命名规范**：为按钮使用清晰的命名，说明按钮功能
2. **模式控制**：合理设置showInPlayMode和showInEditMode属性
3. **参数类型**：优先使用支持的标准参数类型
4. **批量操作**：确保按钮方法能正确处理多个对象
5. **错误处理**：在按钮方法中加入适当的错误处理
6. **文档注释**：为按钮方法添加XML文档注释
7. **性能考虑**：避免在按钮方法中执行耗时操作

### 注意事项

1. **方法要求**：InspectorButton只能用于实例方法（非静态方法）
2. **权限要求**：方法可以是private或public
3. **批量执行**：在Inspector中点击按钮会执行选中所有对象的对应方法
4. **参数展开**：有参数的方法会显示展开/折叠按钮
5. **编辑模式限制**：某些功能在编辑模式下可能不可用
6. **反射性能**：大量使用反射可能影响性能
7. **版本兼容性**：确保编辑器工具与Unity版本兼容
