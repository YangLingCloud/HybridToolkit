using System;
using UnityEngine;
using UnityEngine.InputSystem;
using HybridToolkit.Events;

namespace HybridToolkit
{
    /// <summary>
    /// 输入中心类，管理各种输入事件
    /// </summary>
    public class InputCenter : PersistentSingletonMono<InputCenter>
    {
        // 使用EventBus中的PinchEvent事件
        
        // 上一帧双指之间的距离
        private float _lastPinchDistance = 0f;
        
        // 是否正在进行双指操作
        private bool _isPinching = false;
        
        /// <summary>
        /// 初始化方法
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();
        }
        
        private void Update()
        {
            // 检测双指触摸缩放
            CheckPinchGesture();
        }
        
        /// <summary>
        /// 检查双指缩放手势
        /// </summary>
        private void CheckPinchGesture()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return;
            
            // 获取当前触摸点数量
            int touchCount = touchscreen.touches.Count;
            
            // 只有双指触摸时才检测缩放
            if (touchCount == 2)
            {
                var touch1 = touchscreen.touches[0];
                var touch2 = touchscreen.touches[1];
                
                // 检查两个触摸点是否都处于活动状态
                if (touch1.isInProgress && touch2.isInProgress)
                {
                    // 获取两个触摸点的位置
                    Vector2 pos1 = touch1.position.ReadValue();
                    Vector2 pos2 = touch2.position.ReadValue();
                    
                    // 计算双指之间的距离
                    float currentDistance = Vector2.Distance(pos1, pos2);
                    
                    // 如果是刚开始双指触摸，记录初始距离
                    if (!_isPinching)
                    {
                        _isPinching = true;
                        _lastPinchDistance = currentDistance;
                        return;
                    }
                    
                    // 计算缩放因子
                    float scaleFactor = currentDistance / _lastPinchDistance;
                    
                    // 只有当缩放因子变化明显时才触发事件
                    if (Mathf.Abs(scaleFactor - 1f) > 0.01f)
                    {
                        // 计算双指中点
                        Vector2 midpoint = (pos1 + pos2) * 0.5f;
                        
                        // 通过EventBus触发缩放事件
                         EventBus<HybridToolkit.Events.PinchEvent>.Raise(new HybridToolkit.Events.PinchEvent { midpoint = midpoint, scaleFactor = scaleFactor });
                        
                        // 更新上一帧的距离
                        _lastPinchDistance = currentDistance;
                    }
                }
                else
                {
                    _isPinching = false;
                }
            }
            else
            {
                _isPinching = false;
            }
        }
        
        /// <summary>
        /// 检查是否正在进行双指缩放
        /// </summary>
        public bool IsPinching()
        {
            return _isPinching;
        }
    }
}
