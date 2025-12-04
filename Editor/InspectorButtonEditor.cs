using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using HybridToolkit;

namespace HybridToolkit.Editor
{
    /// <summary>
    /// Inspector按钮编辑器，用于在Inspector中显示带有InspectorButtonAttribute的方法按钮
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class InspectorButtonEditor : UnityEditor.Editor
    {
        /// <summary>
        /// 存储参数值的字典
        /// </summary>
        private Dictionary<string, object> parameterValues = new Dictionary<string, object>();
        
        /// <summary>
        /// 存储方法展开状态的字典
        /// </summary>
        private Dictionary<string, bool> methodExpandedStates = new Dictionary<string, bool>();
        
        /// <summary>
        /// 重写OnInspectorGUI方法，绘制自定义UI
        /// </summary>
        public override void OnInspectorGUI()
        {
            // 绘制默认Inspector
            DrawDefaultInspector();
            
            // 获取目标对象
            var target = serializedObject.targetObject;
            
            // 获取所有带有InspectorButtonAttribute的方法
            var methods = GetMethodsWithAttribute<InspectorButtonAttribute>();
            
            if (methods.Count > 0)
            {
                // 添加分隔线
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Inspector按钮", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                
                // 绘制每个方法按钮
                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttribute<InspectorButtonAttribute>();
                    
                    // 检查是否应该显示此按钮
                    if (!ShouldShowButton(attribute))
                        continue;
                    
                    // 按钮名称
                    string buttonName = string.IsNullOrEmpty(attribute.ButtonName) ? method.Name : attribute.ButtonName;
                    
                    // 方法参数
                    var parameters = method.GetParameters();
                    
                    // 生成唯一键
                    string methodKey = method.DeclaringType.Name + "." + method.Name;
                    
                    // 初始化展开状态
                    if (!methodExpandedStates.ContainsKey(methodKey))
                    {
                        methodExpandedStates[methodKey] = parameters.Length > 0;
                    }
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    if (parameters.Length > 0)
                    {
                        // 有参数的方法，显示展开/折叠按钮
                        methodExpandedStates[methodKey] = EditorGUILayout.Foldout(methodExpandedStates[methodKey], buttonName);
                        
                        if (methodExpandedStates[methodKey])
                        {
                            // 显示参数输入框
                            EditorGUI.indentLevel++;
                            
                            foreach (var parameter in parameters)
                            {
                                string paramKey = methodKey + "." + parameter.Name;
                                
                                // 确保参数值字典中有该参数
                                if (!parameterValues.ContainsKey(paramKey))
                                {
                                    parameterValues[paramKey] = GetDefaultValue(parameter.ParameterType);
                                }
                                
                                // 绘制参数输入控件
                                parameterValues[paramKey] = DrawParameterField(parameter.Name, parameter.ParameterType, parameterValues[paramKey]);
                            }
                            
                            EditorGUI.indentLevel--;
                            
                            // 执行按钮
                            if (GUILayout.Button("执行"))
                            {
                                ExecuteMethod(method);
                            }
                        }
                    }
                    else
                    {
                        // 无参数的方法，直接显示按钮
                        if (GUILayout.Button(buttonName))
                        {
                            ExecuteMethod(method);
                        }
                    }
                    
                    EditorGUILayout.EndVertical();
                }
            }
        }
        
        /// <summary>
        /// 获取所有带有指定特性的方法
        /// </summary>
        private List<MethodInfo> GetMethodsWithAttribute<T>() where T : Attribute
        {
            var methods = new List<MethodInfo>();
            var type = target.GetType();
            
            // 获取所有公共和非公共方法
            var allMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            
            foreach (var method in allMethods)
            {
                if (method.GetCustomAttribute<T>() != null)
                {
                    methods.Add(method);
                }
            }
            
            return methods;
        }
        
        /// <summary>
        /// 检查是否应该显示按钮
        /// </summary>
        private bool ShouldShowButton(InspectorButtonAttribute attribute)
        {
            bool isPlaying = Application.isPlaying;
            
            if (isPlaying && attribute.ShowInPlayMode)
                return true;
            
            if (!isPlaying && attribute.ShowInEditMode)
                return true;
            
            return false;
        }
        
        /// <summary>
        /// 绘制参数输入字段
        /// </summary>
        private object DrawParameterField(string name, Type type, object value)
        {
            if (type == typeof(int))
            {
                return EditorGUILayout.IntField(name, (int)value);
            }
            else if (type == typeof(float))
            {
                return EditorGUILayout.FloatField(name, (float)value);
            }
            else if (type == typeof(bool))
            {
                return EditorGUILayout.Toggle(name, (bool)value);
            }
            else if (type == typeof(string))
            {
                return EditorGUILayout.TextField(name, (string)value);
            }
            else if (type == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(name, (Vector2)value);
            }
            else if (type == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(name, (Vector3)value);
            }
            else if (type == typeof(Vector4))
            {
                return EditorGUILayout.Vector4Field(name, (Vector4)value);
            }
            else if (type == typeof(Color))
            {
                return EditorGUILayout.ColorField(name, (Color)value);
            }
            else if (type == typeof(Quaternion))
            {
                Vector4 vector = new Vector4(((Quaternion)value).x, ((Quaternion)value).y, ((Quaternion)value).z, ((Quaternion)value).w);
                vector = EditorGUILayout.Vector4Field(name, vector);
                return new Quaternion(vector.x, vector.y, vector.z, vector.w);
            }
            else if (type == typeof(Rect))
            {
                return EditorGUILayout.RectField(name, (Rect)value);
            }
            else if (type == typeof(Bounds))
            {
                return EditorGUILayout.BoundsField(name, (Bounds)value);
            }
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return EditorGUILayout.ObjectField(name, (UnityEngine.Object)value, type, true);
            }
            else
            {
                // 不支持的类型，显示为禁用的文本字段
                EditorGUILayout.LabelField(name, "不支持的类型: " + type.Name, EditorStyles.textField);
                return value;
            }
        }
        
        /// <summary>
        /// 获取类型的默认值
        /// </summary>
        private object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// 执行方法
        /// </summary>
        private void ExecuteMethod(MethodInfo method)
        {
            try
            {
                var parameters = method.GetParameters();
                var args = new object[parameters.Length];
                
                // 获取参数值
                string methodKey = method.DeclaringType.Name + "." + method.Name;
                for (int i = 0; i < parameters.Length; i++)
                {
                    string paramKey = methodKey + "." + parameters[i].Name;
                    if (parameterValues.ContainsKey(paramKey))
                    {
                        args[i] = parameterValues[paramKey];
                    }
                }
                
                // 执行方法
                method.Invoke(target, args);
            }
            catch (Exception e)
            {
                Debug.LogError("执行方法时出错: " + e.Message);
            }
        }
    }
}