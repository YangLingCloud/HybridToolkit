using UnityEngine;

namespace HybridToolkit.CameraController
{
    public class AutoRotateStrategy : ICameraMotionStrategy
    {
        private readonly CameraSettings _settings;
        private float _rotationSpeed;
        private bool _active;
        private float _yawOffset;

        public bool IsFinished => !_active;

        public AutoRotateStrategy(CameraSettings settings)
        {
            _settings = settings;
            _rotationSpeed = settings.AutoRotateSpeed; // 从设置中获取默认旋转速度
            _active = false;
            _yawOffset = 0f;
        }

        /// <summary>
        /// 重置并激活自动旋转策略
        /// </summary>
        /// <param name="rotationSpeed">旋转速度（度/秒），如果为0则使用默认速度</param>
        public void Reset(float rotationSpeed = 0f)
        {
            _rotationSpeed = rotationSpeed > 0 ? rotationSpeed : _settings.AutoRotateSpeed;
            _active = true;
            _yawOffset = 0f;
        }

        /// <summary>
        /// 停止自动旋转
        /// </summary>
        public void Stop()
        {
            _active = false;
        }

        public CameraPose CalculateNextPose(CameraPose currentPose, Vector2 inputDelta, float zoomDelta, float dt)
        {
            if (!_active) return currentPose;

            // 累加偏移量
            _yawOffset += _rotationSpeed * dt;
            
            // 应用偏移到当前的Yaw角度
            currentPose.Yaw += _rotationSpeed * dt;
            
            // 可选：限制Yaw在0-360范围内（防止数值过大）
            // currentPose.Yaw = Mathf.Repeat(currentPose.Yaw, 360f);
            
            return currentPose;
        }
    }
}