using Unity.Collections;
using Unity.Properties;
using UnityEngine;
// 无需引用Editor命名空间，因为Attribute类现在在HybridToolkit命名空间中

namespace HybridToolkit
{
    /// <summary>
    /// Inspector按钮示例脚本，用于演示InspectorButtonAttribute的使用
    /// </summary>
    public class InspectorButtonExample : MonoBehaviour
    {
        // 直接标记私有序列化字段，简单直观
        [SerializeField]
        [InspectorReadOnly] // 使用自定义的特性
        private float _readOnlyTest = 101111110f;
        
        
        /// <summary>
        /// 无参数方法示例
        /// </summary>
        [InspectorButton("无参数测试")]
        private void TestNoParameters()
        {
            _readOnlyTest  *= 2.15f;
            Debug.Log("调用了无参数方法");
        }
        
        /// <summary>
        /// 基本参数方法示例
        /// </summary>
        [InspectorButton("基本参数测试")]
        private void TestBasicParameters(int count, float value, bool flag)
        {
            Debug.Log($"调用了基本参数方法: count={count}, value={value}, flag={flag}");
        }
        
        /// <summary>
        /// 字符串参数方法示例
        /// </summary>
        [InspectorButton("字符串测试")]
        private void TestStringParameter(string message)
        {
            Debug.Log($"调用了字符串参数方法: message='{message}'");
        }
        
        /// <summary>
        /// Vector参数方法示例
        /// </summary>
        [InspectorButton("Vector3测试")]
        private void TestVectorParameters(Vector3 position, Color color)
        {
            Debug.Log($"调用了Vector参数方法: position={position}, color={color}");
        }
        
        /// <summary>
        /// 对象参数方法示例
        /// </summary>
        [InspectorButton("对象参数测试")]
        private void TestObjectParameters(GameObject gameObject, Transform transform)
        {
            Debug.Log($"调用了对象参数方法: gameObject={gameObject?.name}, transform={transform?.name}");
        }
        
        /// <summary>
        /// 仅编辑模式方法示例
        /// </summary>
        [InspectorButton("无参数测试",   false)]
        private void TestEditModeOnly()
        {
            Debug.Log("这是一个仅在编辑模式下显示的方法");
        }
        
        /// <summary>
        /// 仅播放模式方法示例
        /// </summary>
        [InspectorButton("无参数测试",   true,   false)]
        private void TestPlayModeOnly()
        {
            Debug.Log("这是一个仅在播放模式下显示的方法");
        }
    }
}