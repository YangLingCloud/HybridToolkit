# CustomAttribute

一个灵活的Unity自定义属性系统，用于增强Unity编辑器的功能性和开发者的工作流程。

## 目录

- [核心特性](#核心特性)
- [快速开始](#快速开始)
- [文件结构](#文件结构)
- [核心概念](#核心概念)
- [使用方法](#使用方法)
- [最佳实践](#最佳实践)

## 核心特性

- **自定义属性系统**：
  - ReadOnlyAttribute：只读属性显示
  - ConditionalDisplayAttribute：条件显示属性
  - ValidateInputAttribute：输入验证属性
  - ColorPickerAttribute：颜色选择器属性
  - ButtonAttribute：按钮属性
- **属性访问器**：
  - PropertyAccessor：属性访问器
  - PropertyValidator：属性验证器
  - PropertyDrawer：属性绘制器
- **验证器**：
  - RangeValidator：范围验证器
  - RegexValidator：正则表达式验证器
  - CustomValidator：自定义验证器
- **扩展性**：支持自定义属性和验证器

## 快速开始

### 环境要求
- Unity 2022.3或更高版本
- .NET 6或更高版本

### 安装步骤
1. 通过Unity Package Manager安装HybridToolkit包
2. 在代码中引用`HybridToolkit.CustomAttribute`命名空间
3. 开始使用各种自定义属性

### 代码示例

#### 1. 只读属性
```csharp
using HybridToolkit.CustomAttribute;
using UnityEngine;

public class PlayerStats : MonoBehaviour {
    [ReadOnly]
    public string playerName;
    
    [ReadOnly]
    public int playerLevel = 1;
    
    [ReadOnly]
    public float playerHealth = 100f;
    
    [ReadOnly]
    public Vector3 playerPosition;
    
    private void Update() {
        playerPosition = transform.position;
    }
}

// 使用方式：
// 在Unity编辑器中，PlayerStats组件的只读属性将无法在Inspector中编辑
```

#### 2. 条件显示属性
```csharp
using HybridToolkit.CustomAttribute;
using UnityEngine;

public class CharacterController : MonoBehaviour {
    public bool useCustomAnimation = false;
    
    [ConditionalDisplay("useCustomAnimation")]
    public string animationName;
    
    [ConditionalDisplay("useCustomAnimation")]
    public float animationSpeed = 1.0f;
    
    [ConditionalDisplay("useCustomAnimation", false)]
    public string defaultAnimation = "Idle";
    
    private void Update() {
        if (useCustomAnimation) {
            // 使用自定义动画
        } else {
            // 使用默认动画
        }
    }
}

// 使用方式：
// 当useCustomAnimation为true时，animationName和animationSpeed属性将显示
// 当useCustomAnimation为false时，defaultAnimation属性将显示
```

#### 3. 输入验证属性
```csharp
using HybridToolkit.CustomAttribute;
using UnityEngine;

public class InputValidator : MonoBehaviour {
    [ValidateInput("ValidateEmail", "请输入有效的邮箱地址")]
    public string email;
    
    [ValidateInput("ValidateAge", "年龄必须在0-120之间")]
    public int age;
    
    [ValidateInput("ValidateUrl", "请输入有效的URL")]
    public string website;
    
    private bool ValidateEmail(string value) {
        return !string.IsNullOrEmpty(value) && value.Contains("@") && value.Contains(".");
    }
    
    private bool ValidateAge(int value) {
        return value >= 0 && value <= 120;
    }
    
    private bool ValidateUrl(string value) {
        return !string.IsNullOrEmpty(value) && (value.StartsWith("http://") || value.StartsWith("https://"));
    }
}

// 使用方式：
// 在Inspector中编辑属性时，会自动进行验证并显示错误信息
```

#### 4. 颜色选择器属性
```csharp
using HybridToolkit.CustomAttribute;
using UnityEngine;

public class ColorExample : MonoBehaviour {
    [ColorPicker]
    public Color characterColor = Color.white;
    
    [ColorPicker("Character Color", true)]
    public Color backgroundColor = Color.black;
    
    [ColorPicker("UI Color", false, true)]
    public Color textColor = Color.white;
    
    private void Start() {
        // 使用颜色值
        Debug.Log($"Character color: {characterColor}");
        Debug.Log($"Background color: {backgroundColor}");
        Debug.Log($"Text color: {textColor}");
    }
}

// 使用方式：
// 在Inspector中点击颜色属性会打开颜色选择器
```

## 文件结构

```
CustomAttribute/
├── Attributes/
│   ├── ReadOnlyAttribute.cs         # 只读属性
│   ├── ConditionalDisplayAttribute.cs # 条件显示属性
│   ├── ValidateInputAttribute.cs    # 输入验证属性
│   ├── ColorPickerAttribute.cs      # 颜色选择器属性
│   ├── ButtonAttribute.cs           # 按钮属性
│   └── PropertyDrawerAttribute.cs   # 属性绘制器属性
├── Accessors/
│   ├── PropertyAccessor.cs          # 属性访问器
│   ├── PropertyValidator.cs         # 属性验证器
│   └── PropertyDrawer.cs            # 属性绘制器
├── Validators/
│   ├── RangeValidator.cs            # 范围验证器
│   ├── RegexValidator.cs            # 正则表达式验证器
│   └── CustomValidator.cs           # 自定义验证器
└── README.md                        # 文档
```

## 核心概念

### 1. 自定义属性

通过继承PropertyAttribute类创建自定义属性：

```csharp
[AttributeUsage(AttributeTargets.Field)]
public class MyCustomAttribute : PropertyAttribute {
    public string label;
    public Color color = Color.white;
    
    public MyCustomAttribute(string label) {
        this.label = label;
    }
}
```

### 2. 属性绘制器

通过继承PropertyDrawer类创建自定义属性绘制器：

```csharp
[CustomPropertyDrawer(typeof(MyCustomAttribute))]
public class MyCustomPropertyDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return base.GetPropertyHeight(property, label);
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // 自定义绘制逻辑
    }
}
```

### 3. 属性验证器

通过实现IPropertyValidator接口创建属性验证器：

```csharp
public class MyPropertyValidator : IPropertyValidator {
    public bool IsValid(object value) {
        // 验证逻辑
        return true;
    }
    
    public string GetErrorMessage() {
        return "验证失败";
    }
}
```

## 使用方法

详细的代码示例请参考[快速开始](#快速开始)章节中的示例。

## 最佳实践

1. **属性命名**：使用清晰的属性名称和标签
2. **验证逻辑**：实现简洁高效的验证逻辑
3. **错误处理**：提供明确的错误信息
4. **性能考虑**：避免在属性绘制器中进行复杂的计算
5. **兼容性**：确保自定义属性在不同Unity版本中正常工作
6. **文档化**：为自定义属性提供清晰的文档说明

### 示例场景

#### 游戏对象配置器

```csharp
public class GameObjectConfigurator : MonoBehaviour {
    [ReadOnly]
    public string objectID;
    
    [ConditionalDisplay("isPlayer")]
    public string playerName;
    
    [ValidateInput("ValidateHealth", "生命值必须在1-100之间")]
    public int health = 100;
    
    [ValidateInput("ValidateSpeed", "速度必须大于0")]
    public float speed = 5.0f;
    
    [ColorPicker("Object Color")]
    public Color objectColor = Color.white;
    
    public bool isPlayer = false;
    
    private bool ValidateHealth(int value) {
        return value > 0 && value <= 100;
    }
    
    private bool ValidateSpeed(float value) {
        return value > 0;
    }
    
    private void Start() {
        objectID = System.Guid.NewGuid().ToString();
        
        // 应用颜色
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) {
            renderer.material.color = objectColor;
        }
    }
}
```

### 注意事项

1. **AttributeUsage**：正确设置属性的使用范围
2. **Inspector更新**：属性值变化时刷新Inspector显示
3. **序列化**：确保属性可以正确序列化和反序列化
4. **多平台兼容**：考虑不同平台的兼容性
5. **性能影响**：自定义属性可能影响Inspector性能
6. **错误处理**：处理属性验证失败的情况
