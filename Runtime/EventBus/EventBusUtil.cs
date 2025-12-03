using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 事件总线工具类，用于管理所有事件总线和事件类型
/// </summary>
public static class EventBusUtil {
    /// <summary>
    /// 所有事件类型的只读列表
    /// </summary>
    public static IReadOnlyList<Type> EventTypes { get; set; }
    
    /// <summary>
    /// 所有事件总线类型的只读列表
    /// </summary>
    public static IReadOnlyList<Type> EventBusTypes { get; set; }
    
#if UNITY_EDITOR
    /// <summary>
    /// 当前编辑器播放模式状态
    /// </summary>
    public static PlayModeStateChange PlayModeState { get; set; }
    
    /// <summary>
    /// 初始化EventBusUtil的Unity编辑器相关组件。
    /// [InitializeOnLoadMethod]属性使该方法在每次加载脚本或游戏在编辑器中进入播放模式时被调用。
    /// 这对于初始化编辑状态下必要的类字段或状态非常有用，这些状态在游戏进入播放模式时也适用。
    /// 该方法设置playModeStateChanged事件的订阅者，以便在编辑器的播放模式更改时执行操作。
    /// </summary>    
    [InitializeOnLoadMethod]
    public static void InitializeEditor() {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    static void OnPlayModeStateChanged(PlayModeStateChange state) {
        PlayModeState = state;
        if (state == PlayModeStateChange.ExitingPlayMode) {
            ClearAllBuses();
        }
    }
#endif

    /// <summary>
    /// 在运行时加载任何场景之前初始化EventBusUtil类。
    /// [RuntimeInitializeOnLoadMethod]属性指示Unity在游戏加载后但在加载任何场景之前执行此方法，
    /// 无论是在播放模式下还是在构建运行后。这保证了在任何游戏对象、脚本或组件启动之前，
    /// 与总线相关的类型和事件的必要初始化已经完成。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize() {
        EventTypes = PredefinedAssemblyUtil.GetTypes(typeof(IEvent));
        EventBusTypes = InitializeAllBuses();
    }

    /// <summary>
    /// 初始化所有事件总线
    /// </summary>
    /// <returns>所有事件总线类型的列表</returns>
    static List<Type> InitializeAllBuses() {
        List<Type> eventBusTypes = new List<Type>();
        
        var typedef = typeof(EventBus<>);
        foreach (var eventType in EventTypes) {
            var busType = typedef.MakeGenericType(eventType);
            eventBusTypes.Add(busType);
            Debug.Log($"Initialized EventBus<{eventType.Name}>");
        }
        
        return eventBusTypes;
    }

    /// <summary>
    /// 清除应用程序中所有事件总线上的所有监听器。
    /// </summary>
    public static void ClearAllBuses() {
        Debug.Log("Clearing all buses...");
        for (int i = 0; i < EventBusTypes.Count; i++) {
            var busType = EventBusTypes[i];
            var clearMethod = busType.GetMethod("Clear", BindingFlags.Static | BindingFlags.NonPublic);
            clearMethod?.Invoke(null, null);
        }
    }
}