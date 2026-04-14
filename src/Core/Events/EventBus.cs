using System;
using System.Collections.Generic;
using MelonLoader;

namespace AstralPartyMod.Core.Events
{
    /// <summary>
    /// 全局事件总线 - 支持Mod间通信和事件广播
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _eventHandlers = new Dictionary<Type, Delegate>();

        /// <summary>
        /// 订阅事件
        /// </summary>
        public static void Subscribe<T>(Action<T> handler)
        {
            var eventType = typeof(T);
            if (!_eventHandlers.TryGetValue(eventType, out var handlers))
            {
                _eventHandlers[eventType] = handler;
            }
            else
            {
                _eventHandlers[eventType] = Delegate.Combine(handlers, handler);
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler)
        {
            var eventType = typeof(T);
            if (_eventHandlers.TryGetValue(eventType, out var handlers))
            {
                var newHandlers = Delegate.Remove(handlers, handler);
                if (newHandlers == null)
                {
                    _eventHandlers.Remove(eventType);
                }
                else
                {
                    _eventHandlers[eventType] = newHandlers;
                }
            }
        }

        /// <summary>
        /// 广播事件
        /// </summary>
        public static void Publish<T>(T evt)
        {
            var eventType = typeof(T);
            if (_eventHandlers.TryGetValue(eventType, out var handlers) && handlers is Action<T> typedHandlers)
            {
                try
                {
                    typedHandlers.Invoke(evt);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[EventBus] 处理事件 {typeof(T).Name} 时发生错误: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 获取当前订阅者数量
        /// </summary>
        public static int GetSubscriberCount<T>()
        {
            var eventType = typeof(T);
            if (_eventHandlers.TryGetValue(eventType, out var handlers))
            {
                return handlers.GetInvocationList().Length;
            }
            return 0;
        }

        /// <summary>
        /// 清除所有订阅
        /// </summary>
        public static void ClearAll()
        {
            _eventHandlers.Clear();
            MelonLogger.Msg("[EventBus] 所有事件订阅已清除");
        }

        /// <summary>
        /// 清除特定事件的所有订阅
        /// </summary>
        public static void Clear<T>()
        {
            var eventType = typeof(T);
            _eventHandlers.Remove(eventType);
        }
    }
}
