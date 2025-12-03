using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace HybridToolkit.CameraController
{
    /// <summary>
    /// 相机控制器类，用于控制相机的旋转、缩放和平滑过渡
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("基础设置")]
        /// <summary>
        /// 相机设置配置文件，包含相机的各项参数设置
        /// </summary>
        [SerializeField] private CameraSettings settings;
        
        /// <summary>
        /// 要控制的相机组件
        /// </summary>
        [SerializeField] private Camera targetCamera;
        
        /// <summary>
        /// 相机观察的目标对象
        /// </summary>
        [SerializeField] private Transform lookAtTarget;
        
        [Header("输入设置")]
        /// <summary>
        /// 旋转输入动作引用
        /// </summary>
        [SerializeField] private InputActionReference rotationInput;
        
        /// <summary>
        /// 缩放输入动作引用
        /// </summary>
        [SerializeField] private InputActionReference zoomInput;
        
        // 相机当前状态
        /// <summary>
        /// 相机当前姿态（x=Pitch, y=Yaw, z=Distance）
        /// </summary>
        [SerializeField] private Vector3 currentPose;
        
        // 相机目标状态
        /// <summary>
        /// 相机目标姿态（x=Pitch, y=Yaw, z=Distance）
        /// </summary>
        private Vector3 targetPose;
        
        // 速度变量（用于平滑停止效果）
        /// <summary>
        /// 旋转速度向量（用于平滑停止效果）
        /// </summary>
        private Vector2 rotationVelocity = Vector2.zero;
        
        /// <summary>
        /// 缩放速度（用于平滑停止效果）
        /// </summary>
        private float zoomVelocity = 0f;
        
        // 输入相关
        /// <summary>
        /// 当前旋转输入值
        /// </summary>
        private Vector2 rotationInputValue;
        
        /// <summary>
        /// 当前缩放输入值
        /// </summary>
        private float zoomInputValue;
        
        /// <summary>
        /// 是否正在交互中
        /// </summary>
        private bool isInteracting;
        
        // 取消令牌源
        /// <summary>
        /// 归位操作的取消令牌源
        /// </summary>
        private CancellationTokenSource resetCts;
        
        /// <summary>
        /// 获取或设置观察目标
        /// </summary>
        public Transform LookAtTarget
        {
            get => lookAtTarget;
            set => lookAtTarget = value;
        }
        
        /// <summary>
        /// 获取或设置当前俯仰角（度）
        /// </summary>
        public float CurrentPitch
        {
            get => currentPose.x;
            set
            {
                currentPose.x = Mathf.Clamp(value, settings.MinPitch, settings.MaxPitch);
                targetPose.x = currentPose.x;
            }
        }
        
        /// <summary>
        /// 获取或设置当前偏航角（度）
        /// </summary>
        public float CurrentYaw
        {
            get => currentPose.y;
            set
            {
                currentPose.y = value;
                targetPose.y = currentPose.y;
            }
        }
        
        /// <summary>
        /// 获取或设置当前距离
        /// </summary>
        public float CurrentDistance
        {
            get => currentPose.z;
            set
            {
                currentPose.z = Mathf.Clamp(value, settings.MinDistance, settings.MaxDistance);
                targetPose.z = currentPose.z;
            }
        }
        
        /// <summary>
        /// 获取或设置当前相机姿态（x=Pitch, y=Yaw, z=Distance）
        /// </summary>
        public Vector3 CurrentPose
        {
            get => currentPose;
            set
            {
                currentPose.x = Mathf.Clamp(value.x, settings.MinPitch, settings.MaxPitch);
                currentPose.y = value.y;
                currentPose.z = Mathf.Clamp(value.z, settings.MinDistance, settings.MaxDistance);
                targetPose = currentPose;
            }
        }
        
        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }
            
            Initialize();
        }
        
        private void OnEnable()
        {
            if (rotationInput != null)
            {
                rotationInput.action.performed += OnRotationPerformed;
                rotationInput.action.canceled += OnRotationCanceled;
                rotationInput.action.Enable();
            }
            
            if (zoomInput != null)
            {
                zoomInput.action.performed += OnZoomPerformed;
                zoomInput.action.canceled += OnZoomCanceled;
                zoomInput.action.Enable();
            }
        }
        
        private void OnDisable()
        {
            if (rotationInput != null)
            {
                rotationInput.action.performed -= OnRotationPerformed;
                rotationInput.action.canceled -= OnRotationCanceled;
                rotationInput.action.Disable();
            }
            
            if (zoomInput != null)
            {
                zoomInput.action.performed -= OnZoomPerformed;
                zoomInput.action.canceled -= OnZoomCanceled;
                zoomInput.action.Disable();
            }
            
            // 取消归位任务
            if (resetCts != null)
            {
                resetCts.Cancel();
                resetCts.Dispose();
            }
        }
        
        private void Update()
        {
            HandleInput();
            UpdateCameraPosition();
        }
        
        /// <summary>
        /// 初始化相机设置
        /// </summary>
        public void Initialize()
        {
            if (settings == null)
            {
                Debug.LogError("CameraSettings is not assigned!");
                return;
            }
            
            currentPose = new Vector3(settings.DefaultPitch, settings.DefaultYaw, settings.DefaultDistance);
            targetPose = currentPose;
            
            UpdateCameraPosition();
        }
        
        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            if (isInteracting)
            {
                // 应用旋转输入
                targetPose.y += rotationInputValue.x * settings.RotationSensitivity;
                targetPose.x += -rotationInputValue.y * settings.RotationSensitivity;
                
                // 限制俯仰角
                targetPose.x = Mathf.Clamp(targetPose.x, settings.MinPitch, settings.MaxPitch);
                
                // 应用缩放输入
                targetPose.z += -zoomInputValue * settings.ZoomSensitivity;
                targetPose.z = Mathf.Clamp(targetPose.z, settings.MinDistance, settings.MaxDistance);
            }
            else
            {
                // 逐渐停止旋转
                rotationVelocity = Vector2.SmoothDamp(rotationVelocity, Vector2.zero, ref rotationVelocity, settings.RotationDamping);
                
                targetPose.y += rotationVelocity.x * settings.RotationSensitivity;
                targetPose.x += -rotationVelocity.y * settings.RotationSensitivity;
                targetPose.x = Mathf.Clamp(targetPose.x, settings.MinPitch, settings.MaxPitch);
                
                // 逐渐停止缩放
                zoomVelocity = Mathf.SmoothDamp(zoomVelocity, 0f, ref zoomVelocity, settings.ZoomDamping);
                
                targetPose.z += -zoomVelocity * settings.ZoomSensitivity;
                targetPose.z = Mathf.Clamp(targetPose.z, settings.MinDistance, settings.MaxDistance);
            }
        }
        
        /// <summary>
        /// 更新相机位置
        /// </summary>
        private void UpdateCameraPosition()
        {
            if (lookAtTarget == null)
                return;
            
            // 平滑过渡到目标角度和距离
            currentPose.x = Mathf.Lerp(currentPose.x, targetPose.x, Time.deltaTime * 10f);
            currentPose.y = Mathf.Lerp(currentPose.y, targetPose.y, Time.deltaTime * 10f);
            currentPose.z = Mathf.Lerp(currentPose.z, targetPose.z, Time.deltaTime * 10f);
            
            // 计算相机位置
            Vector3 direction = new Vector3(0f, 0f, -currentPose.z);
            Quaternion rotation = Quaternion.Euler(currentPose.x, currentPose.y, 0f);
            Vector3 position = lookAtTarget.position + rotation * direction;
            
            // 设置相机位置和旋转
            targetCamera.transform.position = position;
            targetCamera.transform.LookAt(lookAtTarget.position);
        }
        
        /// <summary>
        /// 旋转输入执行
        /// </summary>
        private void OnRotationPerformed(InputAction.CallbackContext context)
        {
            rotationInputValue = context.ReadValue<Vector2>();
            rotationVelocity = rotationInputValue;
            isInteracting = true;
        }
        
        /// <summary>
        /// 旋转输入取消
        /// </summary>
        private void OnRotationCanceled(InputAction.CallbackContext context)
        {
            isInteracting = false;
        }
        
        /// <summary>
        /// 缩放输入执行
        /// </summary>
        private void OnZoomPerformed(InputAction.CallbackContext context)
        {
            zoomInputValue = context.ReadValue<float>();
            zoomVelocity = zoomInputValue;
            isInteracting = true;
        }
        
        /// <summary>
        /// 缩放输入取消
        /// </summary>
        private void OnZoomCanceled(InputAction.CallbackContext context)
        {
            isInteracting = false;
        }
        
        /// <summary>
        /// 自动归位
        /// </summary>
        /// <param name="useUniTask">是否使用UniTask进行异步归位</param>
        /// <param name="targetPitchOverride">自定义目标俯仰角（可选）</param>
        /// <param name="targetYawOverride">自定义目标偏航角（可选）</param>
        /// <param name="targetDistanceOverride">自定义目标距离（可选）</param>
        public void ResetCamera(bool useUniTask = true,
                               float? targetPitchOverride = null,
                               float? targetYawOverride = null,
                               float? targetDistanceOverride = null)
        {
            // 取消正在进行的归位任务
            if (resetCts != null)
            {
                resetCts.Cancel();
                resetCts.Dispose();
            }
            
            // 设置目标值
            float resetPitch = targetPitchOverride ?? settings.DefaultPitch;
            float resetYaw = targetYawOverride ?? settings.DefaultYaw;
            float resetDistance = targetDistanceOverride ?? settings.DefaultDistance;
            
            Vector3 resetPose = new Vector3(resetPitch, resetYaw, resetDistance);
            
            if (useUniTask)
            {
                // 使用UniTask进行异步归位
                resetCts = new CancellationTokenSource();
                ResetCameraAsync(resetPose, resetCts.Token).Forget();
            }
            else
            {
                // 直接设置相机角度
                currentPose = resetPose;
                targetPose = resetPose;
                
                // 重置速度
                rotationVelocity = Vector2.zero;
                zoomVelocity = 0f;
            }
        }
        
        /// <summary>
        /// 异步归位相机
        /// </summary>
        /// <param name="targetPose">目标姿态（x=Pitch, y=Yaw, z=Distance）</param>
        /// <param name="cancellationToken">取消令牌</param>
        private async UniTaskVoid ResetCameraAsync(Vector3 targetPose,
                                                   CancellationToken cancellationToken)
        {
            this.targetPose = targetPose;
            
            // 等待相机归位完成
            while (!Mathf.Approximately(currentPose.x, targetPose.x) ||
                   !Mathf.Approximately(currentPose.y, targetPose.y) ||
                   !Mathf.Approximately(currentPose.z, targetPose.z))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                await UniTask.Yield(cancellationToken);
            }
        }
        
        /// <summary>
        /// 设置相机角度
        /// </summary>
        /// <param name="pitch">俯仰角（度）</param>
        /// <param name="yaw">偏航角（度）</param>
        /// <param name="distance">距离</param>
        public void SetCameraAngles(float pitch, float yaw, float distance)
        {
            currentPose = new Vector3(
                Mathf.Clamp(pitch, settings.MinPitch, settings.MaxPitch),
                yaw,
                Mathf.Clamp(distance, settings.MinDistance, settings.MaxDistance)
            );
            
            targetPose = currentPose;
            
            // 重置速度
            rotationVelocity = Vector2.zero;
            zoomVelocity = 0f;
        }
    }
}