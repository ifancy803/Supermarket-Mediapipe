using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//为事件添加描述Editor
[CustomEditor(typeof(BaseEventSO<>))]
public class BaseEventSOEditor<T> : Editor
{
    private BaseEventSO<T> baseEventSO;

    //初始化
    private void OnEnable()
    {
        if (baseEventSO == null)
            baseEventSO = target as BaseEventSO<T>;
    }

    //获取订阅事件的所有监听者
    private List<MonoBehaviour> GetListeners()
    {
        List<MonoBehaviour> listeners = new List<MonoBehaviour>();

        if (baseEventSO == null || baseEventSO.OnEventRaised == null)
            return listeners;

        //获取订阅事件的所有类型为delegate的监听者数组
        var subscribers = baseEventSO.OnEventRaised.GetInvocationList();

        foreach (var subscriber in subscribers)
        {
            var obj = subscriber.Target as MonoBehaviour;
            if(!listeners.Contains(obj))
                listeners.Add(obj);
        }

        return listeners;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        //Inspector窗口显示事件订阅数量
        EditorGUILayout.LabelField("订阅数量:"+GetListeners().Count);

        //循环显示订阅者列表
        foreach (var listener in GetListeners())
        {
            EditorGUILayout.LabelField(listener.ToString());    
        }
    }
}
