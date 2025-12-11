using UnityEngine;

namespace HybridToolkit.CameraController
{
    public class ManualMotionStrategy : ICameraMotionStrategy
    {
        private readonly CameraSettings _settings;
        private Vector3 _velocity;

        public bool IsFinished => false;

        public ManualMotionStrategy(CameraSettings settings)
        {
            _settings = settings;
        }

        // ★ 新增：用于重置状态，代替 new
        public void Reset()
        {
            _velocity = Vector3.zero;
        }

        // 甚至可以支持继承动量
        public void Reset(Vector3 inheritedVelocity)
        {
            _velocity = inheritedVelocity;
        }

        public CameraPose CalculateNextPose(CameraPose currentPose, Vector2 inputDelta, float zoomDelta, float dt)
        {
            // ... (逻辑不变)
            _velocity.y += inputDelta.x; 
            _velocity.x += inputDelta.y; 
            _velocity.z += zoomDelta;

            if (_velocity.sqrMagnitude > 0.0001f)
            {
                currentPose.Pitch += _velocity.x * dt;
                currentPose.Yaw += _velocity.y * dt;
                currentPose.Distance += _velocity.z * dt;
                _velocity = Vector3.Lerp(_velocity, Vector3.zero, _settings.Damping * dt);
                if (_velocity.sqrMagnitude < 0.0001f) _velocity = Vector3.zero;
            }
            return currentPose;
        }
    }
}