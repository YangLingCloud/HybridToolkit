using UnityEngine;

namespace HybridToolkit.CameraController
{
    public interface ICameraMotionStrategy
    {
        // 核心：给我当前状态和输入数据，我告诉你下一帧在哪里
        // inputDelta: 这一帧用户的鼠标/手势输入量
        CameraPose CalculateNextPose(CameraPose currentPose, Vector2 inputDelta, float zoomDelta, float dt);
        
        // 用于判断这个策略是否"做完了"（主要用于自动归位动画）
        bool IsFinished { get; }
    }
}