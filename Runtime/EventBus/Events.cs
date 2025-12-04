using UnityEngine;

namespace HybridToolkit.Events
{
    /// <summary>
    /// 事件接口，所有事件类型都必须实现此接口
    /// </summary>
    public interface IEvent
    {
    }

    /// <summary>
    /// 测试事件结构体，用于演示基本事件功能
    /// </summary>
    public struct TestEvent : IEvent
    {
    }

    /// <summary>
    /// 玩家事件结构体，包含玩家健康值和魔法值信息
    /// </summary>
    public struct PlayerEvent : IEvent
    {
        /// <summary>
        /// 玩家当前健康值
        /// </summary>
        public int health;

        /// <summary>
        /// 玩家当前魔法值
        /// </summary>
        public int mana;
    }

    /// <summary>
    /// 双指缩放事件结构体，包含缩放信息
    /// </summary>
    public struct PinchEvent : IEvent
    {
        /// <summary>
        /// 双指中点位置
        /// </summary>
        public Vector2 midpoint;

        /// <summary>
        /// 缩放因子
        /// </summary>
        public float scaleFactor;
    }
}