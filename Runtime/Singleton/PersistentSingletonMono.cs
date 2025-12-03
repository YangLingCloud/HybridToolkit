using UnityEngine;

namespace HybridToolkit
{
    /// <summary>
    /// 持久化的泛型单例MonoBehaviour基类
    /// 确保在场景切换时单例实例不会被销毁
    /// </summary>
    /// <typeparam name="T">要创建单例的组件类型，必须继承自MonoBehaviour</typeparam>
    public class PersistentSingletonMono<T> : MonoBehaviour where T : Component
    {
        /// <summary>
        /// 是否在Awake时自动将游戏对象设置为无父对象
        /// </summary>
        public bool AutoUnparentOnAwake = false;
        
        /// <summary>
        /// 单例实例引用
        /// </summary>
        protected static T instance;

        /// <summary>
        /// 检查单例实例是否存在
        /// </summary>
        public static bool HasInstance => instance != null;

        /// <summary>
        /// 尝试获取单例实例，如果实例不存在则返回null
        /// </summary>
        /// <returns>单例实例或null</returns>
        public static T TryGetInstance() => HasInstance ? instance : null;

        /// <summary>
        /// 获取单例实例，如果实例不存在则自动创建
        /// </summary>
        public static T Instance
        {
            get
            {
                if (!instance)
                {
                    // 尝试在场景中查找现有实例
                    instance = FindAnyObjectByType<T>();
                    if (!instance)
                    {
                        // 如果找不到实例，则创建一个新的游戏对象并添加组件
                        var go = new GameObject($"{typeof(T).Name}_Singleton");
                        instance = go.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// MonoBehaviour的Awake方法，用于初始化单例
        /// </summary>
        protected void Awake()
        {
            Initialization();
        }

        /// <summary>
        /// 初始化单例实例
        /// 可以被子类重写以添加自定义初始化逻辑
        /// </summary>
        protected virtual void Initialization()
        {
            // 仅在运行时初始化
            if (!Application.isPlaying)
                return;

            // 如果启用了自动取消父对象，则将游戏对象设置为无父对象
            if (AutoUnparentOnAwake)
            {
                transform.SetParent(null);
            }

            // 如果实例不存在，则将当前对象设置为实例并标记为DontDestroyOnLoad
            if (!instance)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // 如果实例已存在且不是当前对象，则销毁当前对象
                if (instance != this)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}