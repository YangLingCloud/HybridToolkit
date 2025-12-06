using UnityEngine;
using UnityEngine.Serialization;

namespace HybridToolkit.CameraController
{
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "HybridToolkit/Camera/CameraSettings", order = 1)]
    public class CameraSettings : ScriptableObject
    {
        [FormerlySerializedAs("DefaultAngleDistance")] [Header("默认参数")]
        public Vector3 DefaultPose = new Vector3(45f, 0f, 10f); // Pitch, Yaw, Distance
        
        [Header("Sensitivity & Physics")]
        public float RotateSensitivity = 5f;
        public float ZoomSensitivity = 2f;
        public float Damping = 5f; // 物理惯性的阻尼 (用于手动操作后的停止)
        
        [Header("Limits")]
        public Vector2 PitchLimit = new Vector2(-10f, 80f); 
        public Vector2 DistanceLimit = new Vector2(2f, 50f);

        [Header("Auto Move Animation")]
        [Tooltip("自动归位时的旋转曲线 (0-1)")]
        public AnimationCurve RotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Tooltip("自动归位时的缩放曲线 (0-1)")]
        public AnimationCurve ZoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Tooltip("自动归位需要的时间 (秒)")]
        public float AutoMoveDuration = 0.8f; 
        
        [Tooltip("自动归位时间间隔 (秒)")]
        public float AutoMoveTimeInterval = 5f; 
    }
}