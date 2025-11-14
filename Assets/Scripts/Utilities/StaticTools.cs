using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticTools
{
    public static T GetRandomEnumValue<T>() where T : Enum
    {
        Array values = Enum.GetValues(typeof(T));
        int randomIndex = UnityEngine.Random.Range(0, values.Length);
        return (T)values.GetValue(randomIndex);
    }
}
