using UnityEngine;
using UnityEngine.Events;

//发出者泛型事件基类
public class BaseEventSO<T> : ScriptableObject
{
    [TextArea] 
    public string description;   //发出者事件描述

    public string lastSender;   //发出者

    public UnityAction<T> OnEventRaised;

    /// <summary>
    /// 发出者呼叫事件方法
    /// </summary>
    /// <param name="value">呼叫的事件类型</param>
    /// <param name="sender">事件发出者</param>
    public void RaiseEvent(T value, object sender)
    {
        OnEventRaised?.Invoke(value);
        lastSender = sender.ToString();
    }
}
