namespace HybridToolkit.Events
{
    /// <summary>
    /// 事件监听的优先级定义。数值越大，越早收到事件。
    /// </summary>
    public enum EventPriority
    {
        /// <summary>
        /// 最高优先级 (100)。通常用于：拦截器、作弊检测、无敌判断。
        /// </summary>
        High = 100,

        /// <summary>
        /// 普通优先级 (0)。通常用于：一般的逻辑处理、数值计算。
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 低优先级 (-100)。通常用于：最终结果结算（如扣血）、核心状态变更。
        /// </summary>
        Low = -100,
        
        /// <summary>
        /// 监控级 (-1000)。通常用于：UI 显示、音效播放、日志记录。
        /// <para>注意：默认情况下，事件在此阶段会自动变为只读。</para>
        /// </summary>
        Monitor = -1000
    }
}