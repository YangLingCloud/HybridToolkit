using UnityEngine;

namespace HybridToolkit
{
    /// <summary>
    /// 调节型泛型单例MonoBehaviour基类
    /// 通过初始化时间来管理多个实例，只保留最新创建的实例
    /// </summary>
    /// <typeparam name="T">要创建单例的组件类型，必须继承自MonoBehaviour</typeparam>
    public class RegulatorSingletonMono<T> : MonoBehaviour where T : Component
    {
        /// <summary>
        /// 单例实例引用
        /// </summary>
        protected static T instance;

        /// <summary>
        /// 检查单例实例是否存在
        /// </summary>
        public static bool HasInstance => instance != null;

        /// <summary>
        /// 获取当前实例的初始化时间
        /// </summary>
        public float InitialzationTime { get; private set; }

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
                        // 设置游戏对象的隐藏标志，使其在Hierarchy中不可见且不保存到场景中
                        go.hideFlags = HideFlags.HideAndDontSave;
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

            // 记录当前实例的初始化时间
            InitialzationTime = Time.time;
            // 标记游戏对象为场景切换时不销毁
            DontDestroyOnLoad(gameObject);

            // 查找所有同类型的实例
            T[] oldInstances = FindObjectsByType<T>(FindObjectsSortMode.None);
            // 遍历所有实例，销毁初始化时间早于当前实例的实例
            foreach (var oldInstance in oldInstances)
            {
                if (oldInstance.GetComponent<RegulatorSingletonMono<T>>().InitialzationTime < InitialzationTime)
                {
                    Destroy(oldInstance.gameObject);
                }
            }

            // 如果实例引用为空，则将当前对象设置为实例
            if (!instance)
            {
                instance = this as T;
            }
        }
    }
}