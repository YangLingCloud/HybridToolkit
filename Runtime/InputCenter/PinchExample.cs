using UnityEngine;
using HybridToolkit.Events;

namespace HybridToolkit
{
    /// <summary>
    /// 双指缩放示例 - 演示如何使用InputCenter的双指缩放功能
    /// 将此脚本附加到任何游戏对象上，即可通过双指触摸屏幕来缩放该对象
    /// </summary>
    public class PinchExample : MonoBehaviour
    {
        [Header("缩放设置")]
        /// <summary>
        /// 缩放灵敏度 - 控制缩放的速度
        /// </summary>
        [Range(0.001f, 0.01f)]
        public float pinchSensitivity = 0.005f;
        
        /// <summary>
        /// 最小缩放比例
        /// </summary>
        [Range(0.1f, 1.0f)]
        public float minScale = 0.5f;
        
        /// <summary>
        /// 最大缩放比例
        /// </summary>
        [Range(1.0f, 10.0f)]
        public float maxScale = 5.0f;
        
        /// <summary>
        /// 当前缩放比例
        /// </summary>
        private float _currentScale = 1.0f;
        
        /// <summary>
        /// 事件绑定对象
        /// </summary>
        private EventBinding<HybridToolkit.Events.PinchEvent> _pinchEventBinding;
        
        private void OnEnable()
        {
            // 创建并注册双指缩放事件绑定
            _pinchEventBinding = new EventBinding<HybridToolkit.Events.PinchEvent>(OnPinch);
            EventBus<HybridToolkit.Events.PinchEvent>.Register(_pinchEventBinding);
        }
        
        private void OnDisable()
        {
            // 取消注册双指缩放事件绑定
            if (_pinchEventBinding != null)
            {
                EventBus<HybridToolkit.Events.PinchEvent>.Deregister(_pinchEventBinding);
                _pinchEventBinding = null;
            }
        }
        
        /// <summary>
        /// 双指缩放事件回调
        /// </summary>
        /// <param name="pinchEvent">双指缩放事件数据</param>
        private void OnPinch(HybridToolkit.Events.PinchEvent pinchEvent)
        {
            // 根据缩放因子计算缩放比例
            float scaleChange = Mathf.Pow(pinchEvent.scaleFactor, pinchSensitivity);
            
            // 更新当前缩放比例
            _currentScale *= scaleChange;
            
            // 限制缩放比例在最小值和最大值之间
            _currentScale = Mathf.Clamp(_currentScale, minScale, maxScale);
            
            // 应用缩放
            transform.localScale = Vector3.one * _currentScale;
            
            // 在控制台输出缩放信息
            Debug.Log($"双指缩放: 中点 = {pinchEvent.midpoint}, 缩放因子 = {pinchEvent.scaleFactor}, 缩放比例 = {_currentScale}");
        }
    }
}
