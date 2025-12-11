using UnityEngine;

namespace HybridToolkit.CameraController
{
    public class AutoAlignMotionStrategy : ICameraMotionStrategy
    {
        private readonly CameraSettings _settings;
        private CameraPose _startPose;
        private CameraPose _targetPose;
        private float _timer;
        private float _duration;
    
        // 标记是否处于激活状态（防止计算错误）
        private bool _active;

        public bool IsFinished => !_active || _timer >= _duration;

        public AutoAlignMotionStrategy(CameraSettings settings)
        {
            _settings = settings;
        }

        // ★ 新增：初始化数据，代替构造函数
        public void Reset(CameraPose start, CameraPose target)
        {
            _startPose = start;
            _targetPose = target;
            _timer = 0f;
            _duration = _settings.AutoMoveDuration;
            _active = true;

            if (_duration < 0.01f) _timer = _duration; 
        }

        public CameraPose CalculateNextPose(CameraPose currentPose, Vector2 inputDelta, float zoomDelta, float dt)
        {
            if (!_active) return currentPose;

            _timer += dt;
            float progress = Mathf.Clamp01(_timer / _duration);
        
            float rotT = _settings.RotationCurve.Evaluate(progress);
            float zoomT = _settings.ZoomCurve.Evaluate(progress);

            return CameraPose.Lerp(_startPose, _targetPose, rotT, zoomT);
        }
    }
}