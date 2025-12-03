using UnityEngine;

namespace HybridToolkit.CameraController
{
    /// <summary>
    /// 相机控制设置类
    /// </summary>
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "HybridToolkit/Camera/CameraSettings", order = 1)]
    public class CameraSettings : ScriptableObject
    {
        /// <summary>
        /// 默认俯仰角（度）
        /// </summary>
        [Header("默认参数")]
        [SerializeField] private float defaultPitch = 45f;
        
        /// <summary>
        /// 默认偏航角（度）
        /// </summary>
        [SerializeField] private float defaultYaw = 0f;
        
        /// <summary>
        /// 默认距离
        /// </summary>
        [SerializeField] private float defaultDistance = 10f;
        
        /// <summary>
        /// 最小俯仰角（度）
        /// </summary>
        [Header("角度限制")]
        [SerializeField] private float minPitch = -90f;
        
        /// <summary>
        /// 最大俯仰角（度）
        /// </summary>
        [SerializeField] private float maxPitch = 90f;
        
        /// <summary>
        /// 最小距离
        /// </summary>
        [Header("距离限制")]
        [SerializeField] private float minDistance = 2f;
        
        /// <summary>
        /// 最大距离
        /// </summary>
        [SerializeField] private float maxDistance = 20f;
        
        /// <summary>
        /// 旋转灵敏度
        /// </summary>
        [Header("灵敏度")]
        [SerializeField] private float rotationSensitivity = 2f;
        
        /// <summary>
        /// 缩放灵敏度
        /// </summary>
        [SerializeField] private float zoomSensitivity = 1f;
        
        /// <summary>
        /// 旋转阻尼
        /// </summary>
        [Header("阻尼")]
        [SerializeField] private float rotationDamping = 0.1f;
        
        /// <summary>
        /// 缩放阻尼
        /// </summary>
        [SerializeField] private float zoomDamping = 0.1f;
        
        /// <summary>
        /// 归位速度
        /// </summary>
        [Header("归位设置")]
        [SerializeField] private float resetSpeed = 5f;
        
        // 属性访问器
        public float DefaultPitch => defaultPitch;
        public float DefaultYaw => defaultYaw;
        public float DefaultDistance => defaultDistance;
        public float MinPitch => minPitch;
        public float MaxPitch => maxPitch;
        public float MinDistance => minDistance;
        public float MaxDistance => maxDistance;
        public float RotationSensitivity => rotationSensitivity;
        public float ZoomSensitivity => zoomSensitivity;
        public float RotationDamping => rotationDamping;
        public float ZoomDamping => zoomDamping;
        public float ResetSpeed => resetSpeed;
    }
}