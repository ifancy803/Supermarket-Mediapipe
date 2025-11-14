using System;
using UnityEngine;
using UnityEngine.Events;

public class BaseEventListener<T> : MonoBehaviour
{
    public BaseEventSO<T> baseEventSO;  //事件发出者
    public UnityEvent<T> response;     //回复事件   'Event'

    private void OnEnable()
    {
        if(baseEventSO!=null)
            baseEventSO.OnEventRaised += OnEventRaised;
    }

    private void OnDisable()
    {
        if(baseEventSO!=null)
            baseEventSO.OnEventRaised -= OnEventRaised;
    }

    private void OnEventRaised(T value)
    {
        response.Invoke(value);
    }
}
