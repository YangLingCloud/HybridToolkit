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
        private CameraZoomEvent _zoomEvent = new CameraZoomEvent();
        private CameraRotateEvent _rotateEvent = new CameraRotateEvent();
        private CameraClickEvent _clickEvent = new CameraClickEvent();

        // 上一帧双指之间的距离
        private float _lastPinchDistance = 0f;

        // 是否正在进行双指操作
        private bool _isPinching = false;

        // 是否正在进行旋转操作
        private bool _isRotating = false;

        // 是否正在进行缩放操作
        private bool _isScaling = false;

        // 上一帧的触摸位置
        private Vector2 _lastTouchPosition = Vector2.zero;

        // 缩放敏感度
        [SerializeField] private float _zoomSensitivity = 1.0f;

        // 旋转敏感度
        [SerializeField] private float _rotationSensitivity = 1.0f;

        float lastInputTime = 0f;

        /// <summary>
        /// 初始化方法
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();
        }

        private void Update()
        {
            // 检测相机控制输入
            CheckCameraInput();
        }


        /// <summary>
        /// 检查相机控制输入
        /// </summary>
        private void CheckCameraInput()
        {
            CheckCameraZoomInput();
            CheckCameraRotationInput();
            CheckCameraClickInput();
        }

        /// <summary>
        /// 检查相机缩放输入
        /// </summary>
        private void CheckCameraZoomInput()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            // PC端鼠标滚轮缩放

            var mouse = Mouse.current;
            if (mouse != null)
            {
                float scrollDelta = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scrollDelta) > 0.01f)
                {
                    // 触发相机缩放事件
                    float zoomDelta = scrollDelta * _zoomSensitivity;
                    _zoomEvent.delta = zoomDelta;
                    EventBus<CameraZoomEvent>.Raise(_zoomEvent);

                    // 标记正在进行缩放操作
                    _isScaling = true;
                }
            }
#elif UNITY_ANDROID || UNITY_IOS
            // 移动端双指缩放手势
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
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
                            // 触发相机缩放事件
                            float zoomDelta = (scaleFactor - 1f) * _zoomSensitivity;
                            _zoomEvent.delta = zoomDelta;
                            EventBus<CameraZoomEvent>.Raise(_zoomEvent);
                            // 更新上一帧的距离
                            _lastPinchDistance = currentDistance;

                            // 标记正在进行缩放操作
                            _isScaling = true;
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
#endif
        }

        /// <summary>
        /// 检查相机旋转输入
        /// </summary>
        private void CheckCameraRotationInput()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            // PC端鼠标右键旋转

            var mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.isPressed)
            {
                // 获取鼠标移动差值
                Vector2 mouseDelta = mouse.delta.ReadValue();
                if (mouseDelta.magnitude > 0.01f)
                {
                    // 触发相机旋转事件
                    Vector2 rotationDelta = mouseDelta * _rotationSensitivity;
                    _rotateEvent.delta = rotationDelta;
                    EventBus<CameraRotateEvent>.Raise(_rotateEvent);
                    // 标记正在进行旋转操作
                    _isRotating = true;
                }
            }
            else
            {
                _isRotating = false;
            }

#elif UNITY_ANDROID || UNITY_IOS
            // 移动端单指触摸旋转

            var touchscreen = Touchscreen.current;
            {
                int touchCount = touchscreen.touches.Count;
                // 单指触摸且不是双指缩放时检测旋转
                if (touchCount == 1 && !_isPinching)
                {
                    var touch = touchscreen.touches[0];
                    if (touch.isInProgress)
                    {
                        Vector2 touchPosition = touch.position.ReadValue();

                        // 如果是刚开始触摸，记录初始位置
                        if (!_isRotating)
                        {
                            _isRotating = true;
                            _lastTouchPosition = touchPosition;
                            return;
                        }

                        // 计算触摸移动差值
                        Vector2 touchDelta = touchPosition - _lastTouchPosition;
                        if (touchDelta.magnitude > 0.01f)
                        {
                            // 触发相机旋转事件
                            Vector2 rotationDelta = touchDelta * _rotationSensitivity;
                            _rotateEvent.delta = rotationDelta;
                            EventBus<CameraRotateEvent>.Raise(_rotateEvent);

                            // 更新上一帧的触摸位置
                            _lastTouchPosition = touchPosition;
                        }
                    }
                    else
                    {
                        _isRotating = false;
                    }
                }
                else if (touchCount == 0)
                {
                    _isRotating = false;
                }
            }
#endif
        }

        /// <summary>
        /// 检查相机点击输入
        /// </summary>
        private void CheckCameraClickInput()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            // PC端鼠标左键点击

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasReleasedThisFrame)
            {
                // 如果没有进行旋转或缩放操作，才触发点击事件
                if (!_isRotating && !_isScaling)
                {
                    Vector2 mousePosition = mouse.position.ReadValue();
                    _clickEvent.position = mousePosition;
                    EventBus<CameraClickEvent>.Raise(_clickEvent);
                }

                // 重置旋转和缩放状态
                _isRotating = false;
                _isScaling = false;
            }

#elif UNITY_ANDROID || UNITY_IOS
            // 移动端单指触摸点击
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                int touchCount = touchscreen.touches.Count;
                if (touchCount == 1)
                {
                    var touch = touchscreen.touches[0];
                    if (touch.phase.ReadValue() == TouchPhase.Ended)
                    {
                        // 如果没有进行旋转或缩放操作，才触发点击事件
                        if (!_isRotating && !_isScaling)
                        {
                            Vector2 touchPosition = touch.position.ReadValue();
                            _clickEvent.position = touchPosition;
                            EventBus<CameraClickEvent>.Raise(_clickEvent);
                        }

                        // 重置旋转和缩放状态
                        _isRotating = false;
                        _isScaling = false;
                    }
                }
            }
#endif
        }

        /// <summary>
        /// 检查是否正在进行双指缩放
        /// </summary>
        public bool IsPinching()
        {
            return _isPinching;
        }

        /// <summary>
        /// 检查是否正在进行旋转操作
        /// </summary>
        public bool IsRotating()
        {
            return _isRotating;
        }

        /// <summary>
        /// 检查是否正在进行缩放操作
        /// </summary>
        public bool IsScaling()
        {
            return _isScaling;
        }

        /// <summary>
        /// 设置缩放敏感度
        /// </summary>
        /// <param name="sensitivity">缩放敏感度值</param>
        public void SetZoomSensitivity(float sensitivity)
        {
            _zoomSensitivity = sensitivity;
        }

        /// <summary>
        /// 设置旋转敏感度
        /// </summary>
        /// <param name="sensitivity">旋转敏感度值</param>
        public void SetRotationSensitivity(float sensitivity)
        {
            _rotationSensitivity = sensitivity;
        }
    }
    
    /// <summary>
    /// 相机缩放事件结构体，包含缩放差值
    /// </summary>
    public struct CameraZoomEvent : IEvent
    {
        /// <summary>
        /// 缩放差值（正数放大，负数缩小）
        /// </summary>
        public float delta;
    }

    /// <summary>
    /// 相机旋转事件结构体，包含旋转差值
    /// </summary>
    public struct CameraRotateEvent : IEvent
    {
        /// <summary>
        /// 旋转差值（X轴旋转相机上下，Y轴旋转相机左右）
        /// </summary>
        public Vector2 delta;
    }

    /// <summary>
    /// 相机点击事件结构体，包含点击位置
    /// </summary>
    public struct CameraClickEvent : IEvent
    {
        /// <summary>
        /// 点击位置（屏幕坐标）
        /// </summary>
        public Vector2 position;
    }
}