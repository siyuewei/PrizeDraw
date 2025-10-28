using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 事件系统 - 基于事件ID的自定义事件系统
/// 特点：
/// 1. 通过EventId枚举直接查找事件的订阅和发布位置
/// 2. 类型安全的事件参数传递
/// 3. 支持立即执行和队列执行
/// </summary>
public class MEventSystem : MonoBehaviour, ISystem
{
    #region 私有字段
    private Dictionary<EventId, Delegate> eventHandlers = new Dictionary<EventId, Delegate>();
    private Queue<QueuedEvent> eventQueue = new Queue<QueuedEvent>();
    private bool isProcessingQueue = false;
    #endregion
    
    #region 内部结构
    private struct QueuedEvent
    {
        public EventId eventId;
        public Action executeAction;
        
        public QueuedEvent(EventId eventId, Action executeAction)
        {
            this.eventId = eventId;
            this.executeAction = executeAction;
        }
    }
    #endregion
    
    #region 系统接口实现
    public void Initialize()
    {
        Debug.Log("[MEventSystem] 初始化事件系统");
    }
    
    public void Cleanup()
    {
        Debug.Log("[MEventSystem] 清理事件系统");
        
        // 清空所有事件订阅
        eventHandlers.Clear();
        eventQueue.Clear();
    }
    #endregion
    
    #region Unity生命周期
    void Update()
    {
        ProcessEventQueue();
    }
    #endregion
    
    #region 订阅和取消订阅
    /// <summary>
    /// 订阅事件（类型安全）
    /// 使用示例：eventSystem.Subscribe&lt;IntEventArg&gt;(EventId.PrizeIndexUpdated, OnPrizeIndexUpdated);
    /// </summary>
    public void Subscribe<T>(EventId eventId, Action<T> handler) where T : EventArgBase
    {
        if (handler == null)
        {
            Debug.LogError($"[MEventSystem] 尝试订阅空的处理器 for event {eventId}");
            return;
        }
        
        if (eventHandlers.TryGetValue(eventId, out var existing))
        {
            // 检查是否重复订阅
            var invocationList = existing.GetInvocationList();
            foreach (var registeredHandler in invocationList)
            {
                if (AreHandlersEqual(registeredHandler, handler))
                {
                    Debug.LogWarning($"[MEventSystem] 尝试重复订阅事件 {eventId}");
                    return;
                }
            }
            
            eventHandlers[eventId] = Delegate.Combine(existing, handler);
            Debug.Log($"[MEventSystem] 添加订阅: {eventId} (共{eventHandlers[eventId].GetInvocationList().Length}个订阅者)");
        }
        else
        {
            eventHandlers[eventId] = handler;
            Debug.Log($"[MEventSystem] 首次订阅: {eventId}");
        }
    }
    
    /// <summary>
    /// 取消订阅事件
    /// </summary>
    public void Unsubscribe<T>(EventId eventId, Action<T> handler) where T : EventArgBase
    {
        if (handler == null)
        {
            Debug.LogError($"[MEventSystem] 尝试取消订阅空的处理器 for event {eventId}");
            return;
        }
        
        if (eventHandlers.TryGetValue(eventId, out var existing))
        {
            var newHandler = Delegate.Remove(existing, handler);
            if (newHandler == null)
            {
                eventHandlers.Remove(eventId);
                Debug.Log($"[MEventSystem] 移除最后一个订阅者: {eventId}");
            }
            else
            {
                eventHandlers[eventId] = newHandler;
                Debug.Log($"[MEventSystem] 取消订阅: {eventId} (剩余{newHandler.GetInvocationList().Length}个订阅者)");
            }
        }
        else
        {
            Debug.LogWarning($"[MEventSystem] 尝试取消订阅不存在的事件: {eventId}");
        }
    }
    #endregion
    
    #region 发布事件
    /// <summary>
    /// 发布事件（立即执行）
    /// 使用示例：eventSystem.Publish(EventId.PrizeIndexUpdated, new IntEventArg(prizeIndex));
    /// </summary>
    public void Publish<T>(EventId eventId, T data) where T : EventArgBase
    {
        ExecuteEventImmediate(eventId, data);
    }
    
    /// <summary>
    /// 发布事件（添加到队列，下一帧执行）
    /// </summary>
    public void PublishQueued<T>(EventId eventId, T data) where T : EventArgBase
    {
        AddToQueue(eventId, data);
    }
    #endregion
    
    #region 私有方法
    private void ExecuteEventImmediate<T>(EventId eventId, T data) where T : EventArgBase
    {
        if (eventHandlers.TryGetValue(eventId, out var handler))
        {
            if (handler is Action<T> action)
            {
                try
                {
                    action(data);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[MEventSystem] 执行事件 {eventId} 时发生错误: {e.Message}\n{e.StackTrace}");
                }
            }
            else
            {
                var expectedType = handler.Method.GetParameters().Length > 0
                    ? handler.Method.GetParameters()[0].ParameterType.Name
                    : "Unknown";
                Debug.LogError($"[MEventSystem] 事件类型不匹配 {eventId}. 期望: {expectedType}, 实际: {typeof(T).Name}");
            }
        }
        else
        {
            // 没有订阅者是正常情况，不需要警告
            // Debug.Log($"[MEventSystem] 没有订阅者的事件: {eventId}");
        }
    }
    
    private void AddToQueue<T>(EventId eventId, T data) where T : EventArgBase
    {
        var queuedEvent = new QueuedEvent(
            eventId,
            () => ExecuteEventImmediate(eventId, data)
        );
        
        eventQueue.Enqueue(queuedEvent);
    }
    
    private void ProcessEventQueue()
    {
        if (isProcessingQueue || eventQueue.Count == 0)
        {
            return;
        }
        
        isProcessingQueue = true;
        
        int processedCount = 0;
        int maxEventsPerFrame = 100; // 防止一帧处理太多事件
        
        while (eventQueue.Count > 0 && processedCount < maxEventsPerFrame)
        {
            var queuedEvent = eventQueue.Dequeue();
            
            try
            {
                queuedEvent.executeAction();
                processedCount++;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MEventSystem] 处理队列事件 {queuedEvent.eventId} 时发生错误: {e.Message}");
            }
        }
        
        isProcessingQueue = false;
    }
    
    private bool AreHandlersEqual(Delegate handler1, Delegate handler2)
    {
        if (ReferenceEquals(handler1, handler2))
        {
            return true;
        }
        
        if (handler1.Method != handler2.Method)
        {
            return false;
        }
        
        if (handler1.Target == null && handler2.Target == null)
        {
            return true;
        }
        
        return ReferenceEquals(handler1.Target, handler2.Target);
    }
    #endregion
    
    #region 调试方法
    /// <summary>
    /// 获取指定事件的订阅者数量
    /// </summary>
    public int GetSubscriberCount(EventId eventId)
    {
        if (eventHandlers.TryGetValue(eventId, out var handler))
        {
            return handler.GetInvocationList().Length;
        }
        return 0;
    }
    
    /// <summary>
    /// 获取队列中的事件数量
    /// </summary>
    public int GetQueuedEventCount()
    {
        return eventQueue.Count;
    }
    #endregion
}
