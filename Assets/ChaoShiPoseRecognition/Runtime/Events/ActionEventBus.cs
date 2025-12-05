using System.Collections.Generic;
using UnityEngine;

namespace ChaoShiPoseRecognition.Events
{
    /// <summary>
    /// 动作事件总线
    /// 统一的事件分发中心，支持多个监听器订阅
    /// </summary>
    public class ActionEventBus
    {
        private List<IActionListener> listeners;
        private Queue<ActionEvent> eventQueue;
        
        // 事件委托（供外部项目直接订阅）
        public System.Action<ActionEvent> OnActionEvent;
        
        public ActionEventBus()
        {
            listeners = new List<IActionListener>();
            eventQueue = new Queue<ActionEvent>();
        }
        
        /// <summary>
        /// 注册监听器
        /// </summary>
        public void RegisterListener(IActionListener listener)
        {
            if (listener != null && !listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }
        
        /// <summary>
        /// 取消注册监听器
        /// </summary>
        public void UnregisterListener(IActionListener listener)
        {
            listeners.Remove(listener);
        }
        
        /// <summary>
        /// 发布事件（立即处理）
        /// </summary>
        public void Publish(ActionEvent actionEvent)
        {
            if (actionEvent == null)
            {
                return;
            }
            
            // 通知所有监听器
            foreach (var listener in listeners)
            {
                try
                {
                    listener.OnActionTriggered(actionEvent);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error in action listener: {e.Message}");
                }
            }
            
            // 触发委托事件
            OnActionEvent?.Invoke(actionEvent);
        }
        
        /// <summary>
        /// 发布事件（延迟处理，在下一帧处理）
        /// </summary>
        public void PublishDeferred(ActionEvent actionEvent)
        {
            if (actionEvent != null)
            {
                eventQueue.Enqueue(actionEvent);
            }
        }
        
        /// <summary>
        /// 处理延迟事件队列（应在 Update 中调用）
        /// </summary>
        public void ProcessDeferredEvents()
        {
            while (eventQueue.Count > 0)
            {
                var actionEvent = eventQueue.Dequeue();
                Publish(actionEvent);
            }
        }
        
        /// <summary>
        /// 清空所有监听器和事件队列
        /// </summary>
        public void Clear()
        {
            listeners.Clear();
            eventQueue.Clear();
            OnActionEvent = null;
        }
    }
}


