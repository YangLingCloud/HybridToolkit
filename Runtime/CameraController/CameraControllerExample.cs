using UnityEngine;
using HybridToolkit.CameraController;

namespace HybridToolkit.CameraController.Example
{
    /// <summary>
    /// 相机控制器示例脚本
    /// </summary>
    public class CameraControllerExample : MonoBehaviour
    {
        [Header("相机控制器设置")]
        /// <summary>
        /// 相机控制器
        /// </summary>
        [SerializeField] private CameraController cameraController;
        
        /// <summary>
        /// 重置按钮
        /// </summary>
        [SerializeField] private UnityEngine.UI.Button resetButton;
        
        /// <summary>
        /// 异步重置按钮
        /// </summary>
        [SerializeField] private UnityEngine.UI.Button asyncResetButton;
        
        /// <summary>
        /// 自定义角度按钮
        /// </summary>
        [SerializeField] private UnityEngine.UI.Button customAngleButton;
        
        /// <summary>
        /// 自定义角度参数
        /// </summary>
        [Header("自定义角度参数")]
        [SerializeField] private float customPitch = 60f;
        [SerializeField] private float customYaw = 45f;
        [SerializeField] private float customDistance = 15f;
        
        private void Start()
        {
            // 初始化相机控制器
            if (cameraController != null)
            {
                cameraController.Initialize();
            }
            
            // 添加按钮事件监听
            if (resetButton != null)
            {
                resetButton.onClick.AddListener(() => ResetCamera(false));
            }
            
            if (asyncResetButton != null)
            {
                asyncResetButton.onClick.AddListener(() => ResetCamera(true));
            }
            
            if (customAngleButton != null)
            {
                customAngleButton.onClick.AddListener(SetCustomAngle);
            }
        }
        
        /// <summary>
        /// 重置相机
        /// </summary>
        /// <param name="useUniTask">是否使用UniTask</param>
        private void ResetCamera(bool useUniTask)
        {
            if (cameraController != null)
            {
                cameraController.ResetCamera(useUniTask);
            }
        }
        
        /// <summary>
        /// 设置自定义角度
        /// </summary>
        private void SetCustomAngle()
        {
            if (cameraController != null)
            {
                cameraController.SetCameraAngles(customPitch, customYaw, customDistance);
            }
        }
        
        private void OnDestroy()
        {
            // 移除按钮事件监听
            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners();
            }
            
            if (asyncResetButton != null)
            {
                asyncResetButton.onClick.RemoveAllListeners();
            }
            
            if (customAngleButton != null)
            {
                customAngleButton.onClick.RemoveAllListeners();
            }
        }
    }
}