using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using HybridToolkit;
using HybridToolkit.Events; // 假设你的 EventBus 命名空间
using UnityEngine.Serialization;

namespace HybridToolkit.CameraController
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target Settings")]
        public Transform LookAtTarget;
        public CameraSettings Settings;

        // 核心状态: X=Pitch, Y=Yaw, Z=Distance
        [FormerlySerializedAs("_currentState")]
        [SerializeField] 
        [InspectorReadOnly] // 假设你有这个Attribute，没有的话用普通的 [SerializeField]
        private Vector3 _currentPose;

        [SerializeField] 
        [InspectorReadOnly]
        private Vector3 _velocity; 

        private EventBinding<CameraRotateEvent> rotationEventBinding;
        private EventBinding<CameraZoomEvent> zoomEventBinding;

        private CancellationTokenSource _cts;
        
        [SerializeField] 
        [InspectorReadOnly]
        private bool _isAutoMoving = false; 

        [SerializeField] 
        [InspectorReadOnly]
        float _autoMoveTime = 0f;
        
        private void Awake()
        {
            if (Settings == null)
            {
                Debug.LogError("CameraSettings is missing!");
                return;
            }

            _currentPose = Settings.DefaultPose;
            UpdateCameraTransform();

            rotationEventBinding = new EventBinding<CameraRotateEvent>(OnCameraRotate);
            zoomEventBinding = new EventBinding<CameraZoomEvent>(OnCameraZoom);
        }

        private void OnEnable()
        {
            EventBus<CameraRotateEvent>.Register(rotationEventBinding);
            EventBus<CameraZoomEvent>.Register(zoomEventBinding);
        }

        private void OnDisable()
        {
            EventBus<CameraRotateEvent>.Deregister(rotationEventBinding);
            EventBus<CameraZoomEvent>.Deregister(zoomEventBinding);
            CancelAutoMove();
        }

        private void Update()
        {
            // 自动归位时，跳过物理计算
            if (_isAutoMoving) return;

            // 1. 应用物理动量逻辑 (保持原样)
            if (_velocity.sqrMagnitude > 0.0001f)
            {
                _currentPose += _velocity * Time.deltaTime;
                _velocity = Vector3.Lerp(_velocity, Vector3.zero, Settings.Damping * Time.deltaTime);
                
                // 当速度极小时直接归零，节省计算
                if (_velocity.sqrMagnitude < 0.0001f) _velocity = Vector3.zero;

                ClampState();
                UpdateCameraTransform();
            }
            
            if(Time.time >= _autoMoveTime && _currentPose!=Settings.DefaultPose)
            {
                SetRotationToTarget(Settings.DefaultPose);
            }
        }

        // ================== Event Handlers ==================

        private void OnCameraRotate(CameraRotateEvent evt)
        {
            CancelAutoMove(); // 打断动画
            float targetYawVel = evt.delta.x * Settings.RotateSensitivity;
            float targetPitchVel = -evt.delta.y * Settings.RotateSensitivity;
            _velocity.y += targetYawVel; 
            _velocity.x += targetPitchVel;
            
            _autoMoveTime =Time.time+ Settings.AutoMoveTimeInterval;
        }

        private void OnCameraZoom(CameraZoomEvent evt)
        {
            CancelAutoMove(); // 打断动画
            float targetZoomVel = -evt.delta * Settings.ZoomSensitivity;
            _velocity.z += targetZoomVel;
            
            _autoMoveTime =Time.time+ Settings.AutoMoveTimeInterval;
        }
        
        // ================== Public API ==================

        public void SetRotationToTarget(Vector3 targetRotation, bool IsHard = false)
        {
            CancelAutoMove();

            if (IsHard)
            {
                _currentPose = targetRotation;
                _velocity = Vector3.zero;
                ClampState();
                UpdateCameraTransform();
            }
            else
            {
                // 启动基于曲线的异步移动
                MoveToTargetAsync(targetRotation).Forget();
            }
        }

        // ================== Logic Core (Modified) ==================

        private async UniTaskVoid MoveToTargetAsync(Vector3 targetPose)
        {
            _isAutoMoving = true;
            _velocity = Vector3.zero; 

            // 记录起始点
            Vector3 startPose = _currentPose;
            float duration = Settings.AutoMoveDuration;
            float timer = 0f;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                // 如果持续时间太短，视为硬切
                if (duration <= 0.01f)
                {
                    _currentPose = targetPose;
                    UpdateCameraTransform();
                    return;
                }

                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    
                    // 计算归一化时间进度 [0, 1]
                    float progress = Mathf.Clamp01(timer / duration);

                    // 在曲线上采样
                    // 允许曲线值超过1或小于0 (用于制作弹跳/Overshoot效果)
                    float rotT = Settings.RotationCurve.Evaluate(progress);
                    float zoomT = Settings.ZoomCurve.Evaluate(progress);

                    // 插值计算
                    // 注意：对于Yaw(Y轴)，如果希望 "走近路" (比如从350度转到10度只转20度而不是340度)，
                    // 可以使用 Mathf.LerpAngle。但如果希望完全受控的“回放”，Mathf.Lerp 更可预测。
                    // 这里保持 Vector3 结构的一致性使用 Lerp，你可以根据需求改为 LerpAngle。
                    
                    _currentPose.x = Mathf.LerpUnclamped(startPose.x, targetPose.x, rotT); // Pitch
                    _currentPose.y = Mathf.LerpAngle(startPose.y, targetPose.y, rotT); // Yaw
                    _currentPose.z = Mathf.LerpUnclamped(startPose.z, targetPose.z, zoomT); // Distance

                    // 动画过程中依然应用限制，防止曲线 Overshoot 导致穿模或翻转
                    ClampState();
                    UpdateCameraTransform();

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                // 确保最终精确到达
                _currentPose = targetPose;
                ClampState();
                UpdateCameraTransform();
            }
            catch (System.OperationCanceledException)
            {
                // 被打断，保持当前位置交给物理系统接管
            }
            finally
            {
                _isAutoMoving = false;
                DisposeCts();
            }
        }

        private void CancelAutoMove()
        {
            if (_cts == null) return;
            _cts.Cancel();
            DisposeCts();
            _isAutoMoving = false;
        }

        private void DisposeCts()
        {
            if (_cts != null)
            {
                _cts.Dispose();
                _cts = null;
            }
        }

        private void ClampState()
        {
            _currentPose.x = Mathf.Clamp(_currentPose.x, Settings.PitchLimit.x, Settings.PitchLimit.y);
            _currentPose.z = Mathf.Clamp(_currentPose.z, Settings.DistanceLimit.x, Settings.DistanceLimit.y);
        }

        private void UpdateCameraTransform()
        {
            if (!LookAtTarget) return;

            Quaternion rotation = Quaternion.Euler(_currentPose.x, _currentPose.y, 0);
            Vector3 position = LookAtTarget.position + rotation * (Vector3.back * _currentPose.z);

            transform.rotation = rotation;
            transform.position = position;
        }
    }
}