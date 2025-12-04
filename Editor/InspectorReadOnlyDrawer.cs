using UnityEditor;
using UnityEngine;

namespace HybridToolkit.Editor
{
    // 关键：将绘制器关联到我们创建的属性
    [CustomPropertyDrawer(typeof(InspectorReadOnlyAttribute))]
    public class InspectorReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 核心：在绘制前禁用GUI
            bool previousGUIState = GUI.enabled;
            GUI.enabled = false; // 禁用交互，字段将变灰

            // 使用默认方式绘制属性字段
            EditorGUI.PropertyField(position, property, label, true);

            // 绘制完成后，恢复之前的GUI状态
            GUI.enabled = previousGUIState;
        }
    }
}