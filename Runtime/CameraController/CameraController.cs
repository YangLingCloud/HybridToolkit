using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using HybridToolkit.Events; 
using UnityEngine.Serialization;

namespace HybridToolkit.CameraController
{
    [Serializable]
    public struct CameraPose
    {
        public float Pitch;    // X
        public float Yaw;      // Y
        public float Distance; // Z

        public CameraPose(float pitch, float yaw, float distance)
        {
            Pitch = pitch;
            Yaw = yaw;
            Distance = distance;
        }

        public CameraPose(Vector3 v) : this(v.x, v.y, v.z) { }

        public Vector3 ToVector3() => new Vector3(Pitch, Yaw, Distance);

        /// <summary>
        /// 自定义插值：支持旋转和距离使用不同的进度 t
        /// </summary>
        public static CameraPose Lerp(CameraPose from, CameraPose to, float tRot, float tZoom)
        {
            return new CameraPose(
                Mathf.LerpUnclamped(from.Pitch, to.Pitch, tRot),
                Mathf.LerpAngle(from.Yaw, to.Yaw, tRot), // 使用 LerpAngle 处理 360 度问题
                Mathf.LerpUnclamped(from.Distance, to.Distance, tZoom)
            );
        }
        
        // 如果你需要标准 Lerp，也可以保留这个重载
        public static CameraPose Lerp(CameraPose from, CameraPose to, float t)
        {
            return Lerp(from, to, t, t);
        }
    }
    
    public class CameraController : MonoBehaviour
    {
        public Transform LookAtTarget;
        public CameraSettings Settings;
        
        [SerializeField,InspectorReadOnly]
        private CameraPose currentPose;
        
        // ★ 核心：持有当前的算法 ★
        private ICameraMotionStrategy _activeStrategy;

        // 状态监控变量
        [SerializeField,InspectorReadOnly]
        private float _lastInputTime;
        private Vector2 _currentFrameRotateInput; // 缓存当前帧的输入
        private float _currentFrameZoomInput;

        private ManualMotionStrategy _manualStrategy;
        private AutoAlignMotionStrategy _autoAlignStrategy;
        
        private void Awake()
        {
            currentPose = new CameraPose(Settings.DefaultPose);
            _lastInputTime = Time.time;
            
            _manualStrategy = new ManualMotionStrategy(Settings);
            _autoAlignStrategy = new AutoAlignMotionStrategy(Settings);
            
            UpdateTransform();
        }

        private void OnEnable()
        {
            EventPipeline<CameraRotateEvent>.Subscribe(OnCameraRotate);
            EventPipeline<CameraZoomEvent>.Subscribe(OnCameraZoom);
        }

        private void OnDisable()
        {
            EventPipeline<CameraRotateEvent>.Unsubscribe(OnCameraRotate);
            EventPipeline<CameraZoomEvent>.Unsubscribe(OnCameraZoom);
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // ========================================================
            // ★ 上层逻辑：决定使用哪个策略 (The Decision Maker) ★
            // ========================================================

            // 1. 检查是否发生了输入
            bool hasInput = _currentFrameRotateInput.sqrMagnitude > 0.001f || Mathf.Abs(_currentFrameZoomInput) > 0.001f;

            if (hasInput)
            {
                _lastInputTime = Time.time;
                
                // 如果当前不是手动模式，立刻切换回手动模式 (打断动画)
                if (_activeStrategy != _manualStrategy)
                {
                    SwitchToManual();
                }
            }
            // 2. 检查是否需要触发自动归位
            else if (Settings.EnableAutoReset)
            {
                bool isTimeout = Time.time - _lastInputTime > Settings.AutoMoveTimeInterval;
                bool isAtTarget = Vector3.Distance(currentPose.ToVector3(), Settings.DefaultPose) < 0.01f;
                bool isAlreadyAuto = _activeStrategy is AutoAlignMotionStrategy;

                // 只有在超时、不在目标点、且还没开始自动归位时，才切换
                if (isTimeout && !isAtTarget && _activeStrategy != _autoAlignStrategy)
                {
                    SwitchToAuto(currentPose, new CameraPose(Settings.DefaultPose));
                }
            }
            
            // 自动归位结束后，切回手动待机（省一点计算，逻辑也更干净）
            if (_activeStrategy == _autoAlignStrategy && _autoAlignStrategy.IsFinished)
            {
                SwitchToManual();
            }

            // ========================================================
            // ★ 下层执行：调用当前策略计算 (The Executor) ★
            // ========================================================
            
            currentPose = _activeStrategy.CalculateNextPose(
                currentPose, 
                _currentFrameRotateInput, 
                _currentFrameZoomInput, 
                dt
            );

            // 清理当前帧输入缓存
            _currentFrameRotateInput = Vector2.zero;
            _currentFrameZoomInput = 0f;

            // 限制并应用
            ClampAndApply();
        }

        private void SwitchToManual()
        {
            _manualStrategy.Reset(); // 重置速度
            _activeStrategy = _manualStrategy;
        }

        private void SwitchToAuto(CameraPose start, CameraPose target)
        {
            _autoAlignStrategy.Reset(start, target); // 重置计时器和目标
            _activeStrategy = _autoAlignStrategy;
        }
        // ================== Event Handlers ==================

        private void OnCameraRotate(CameraRotateEvent evt)
        {
            // 在真正的策略模式中，Controller 负责收集输入，而不是直接分发给 Strategy
            float yaw = evt.delta.x * Settings.RotateSensitivity;
            float pitch = -evt.delta.y * Settings.RotateSensitivity;
            _currentFrameRotateInput += new Vector2(yaw, pitch);
        }

        private void OnCameraZoom(CameraZoomEvent evt)
        {
            float zoom = -evt.delta * Settings.ZoomSensitivity;
            _currentFrameZoomInput += zoom;
        }
        
        public void MoveToTarget(Vector3 target, bool immediate)
        {
             if (immediate)
             {
                 currentPose = new CameraPose(target);
                 SwitchToManual();
                 _lastInputTime = Time.time; // 重置计时
             }
             else
             {
                 SwitchToAuto(currentPose, new CameraPose(target));
             }
        }

        private void ClampAndApply()
        {
            float cPitch = Mathf.Clamp(currentPose.Pitch, Settings.PitchLimit.x, Settings.PitchLimit.y);
            float cDist = Mathf.Clamp(currentPose.Distance, Settings.DistanceLimit.x, Settings.DistanceLimit.y);
            currentPose = new CameraPose(cPitch, currentPose.Yaw, cDist);
            
            if (!LookAtTarget) return;
            Quaternion rotation = Quaternion.Euler(currentPose.Pitch, currentPose.Yaw, 0);
            Vector3 position = LookAtTarget.position + rotation * (Vector3.back * currentPose.Distance);
            transform.rotation = rotation;
            transform.position = position;
        }
        
        private void UpdateTransform()
        {
            if (!LookAtTarget) return;

            Quaternion rotation = Quaternion.Euler(currentPose.Pitch, currentPose.Yaw, 0);
            // 注意：这里用 Vector3.back * distance 来计算位置
            Vector3 position = LookAtTarget.position + rotation * (Vector3.back * currentPose.Distance);

            transform.rotation = rotation;
            transform.position = position;
        }
    }
}