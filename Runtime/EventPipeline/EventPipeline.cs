using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks; // 必须依赖 UniTask 插件

namespace HybridToolkit.Events
{
    /// <summary>
    /// 基于 UniTask 的异步事件管线。
    /// <para>支持优先级排序、异步等待、拦截机制及阶段性锁定。</para>
    /// </summary>
    /// <typeparam name="T">具体的事件类型</typeparam>
    public static class EventPipeline<T> where T : PipelineEvent
    {
        private struct Listener
        {
            public Func<T, UniTask> AsyncCallback;
            public Action<T> SyncCallback;
            public int Priority;
            public object Target; // 用于生命周期检查
        }

        private static readonly List<Listener> _listeners = new List<Listener>(8);
        private static bool _isDirty = false;

        #region 注册 API

        /// <summary>
        /// 注册【异步】监听者。
        /// </summary>
        /// <param name="callback">异步回调方法 (async UniTask)</param>
        /// <param name="priority">优先级</param>
        /// <returns>注销句柄 (IDisposable)</returns>
        public static IDisposable Subscribe(Func<T, UniTask> callback, int priority = (int)EventPriority.Normal)
        {
            if (!Contains(callback))
            {
                _listeners.Add(new Listener
                {
                    AsyncCallback = callback,
                    Priority = priority,
                    Target = callback.Target
                });
                _isDirty = true;
            }
            return new Unsubscriber(() => Unsubscribe(callback));
        }

        /// <summary>
        /// 注册【同步】监听者。
        /// </summary>
        /// <param name="callback">同步回调方法 (void)</param>
        /// <param name="priority">优先级</param>
        /// <returns>注销句柄 (IDisposable)</returns>
        public static IDisposable Subscribe(Action<T> callback, int priority = (int)EventPriority.Normal)
        {
            if (!Contains(callback))
            {
                _listeners.Add(new Listener
                {
                    SyncCallback = callback,
                    Priority = priority,
                    Target = callback.Target
                });
                _isDirty = true;
            }
            return new Unsubscriber(() => Unsubscribe(callback));
        }

        #endregion

        #region 注销 API

        public static void Unsubscribe(Func<T, UniTask> callback)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                if (_listeners[i].AsyncCallback == callback)
                {
                    _listeners.RemoveAt(i);
                    return;
                }
            }
        }

        public static void Unsubscribe(Action<T> callback)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                if (_listeners[i].SyncCallback == callback)
                {
                    _listeners.RemoveAt(i);
                    return;
                }
            }
        }

        #endregion

        #region 触发 API

        /// <summary>
        /// 触发事件管线。
        /// <para>注意：必须使用 await 等待管线执行完毕。</para>
        /// </summary>
        public static async UniTask Raise(T eventData)
        {
            // 1. 排序 (升序：Low -> High)
            if (_isDirty)
            {
                _listeners.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                _isDirty = false;
            }

            // 2. 获取当前事件定义的锁定阈值 (多态调用，无反射)
            int lockThreshold = eventData.LockThreshold;

            // 3. 倒序遍历 (High -> Low)
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                if (eventData.IsConsumed) break;

                var listener = _listeners[i];

                // Unity 对象存活检查
                if (listener.Target is UnityEngine.Object unityObj && unityObj == null)
                {
                    _listeners.RemoveAt(i);
                    continue;
                }

                // 阶段性锁定检查：如果进入了只读阶段（优先级 <= 阈值），则锁定事件
                if (!eventData.IsLocked && listener.Priority <= lockThreshold)
                {
                    eventData.Lock();
                }

                // 执行回调 (含异常隔离)
                try
                {
                    if (listener.SyncCallback != null)
                    {
                        listener.SyncCallback(eventData);
                    }
                    else if (listener.AsyncCallback != null)
                    {
                        // 异步等待：高优先级任务必须执行完，才会轮到低优先级
                        await listener.AsyncCallback(eventData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventPipeline] Error processing {typeof(T).Name}: {e}");
                }
            }
        }

        #endregion

        #region 内部辅助

        private static bool Contains(Func<T, UniTask> cb)
        {
            for (int i = 0; i < _listeners.Count; i++) if (_listeners[i].AsyncCallback == cb) return true;
            return false;
        }

        private static bool Contains(Action<T> cb)
        {
            for (int i = 0; i < _listeners.Count; i++) if (_listeners[i].SyncCallback == cb) return true;
            return false;
        }

        private class Unsubscriber : IDisposable
        {
            private Action _action;
            public Unsubscriber(Action action) => _action = action;
            public void Dispose() { _action?.Invoke(); _action = null; }
        }

        #endregion
    }
}