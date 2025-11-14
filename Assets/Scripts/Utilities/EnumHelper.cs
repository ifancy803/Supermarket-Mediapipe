using System;
using UnityEngine; // 为了使用 Random

public static class EnumHelper
{
    /// <summary>
    /// 获取任意枚举的一个随机值。
    /// </summary>
    /// <typeparam name="T">要获取随机值的枚举类型。</typeparam>
    /// <returns>该枚举的一个随机成员。</returns>
    public static T GetRandomEnum<T>() where T : Enum // where T : Enum 是一个约束，确保T必须是枚举类型
    {
        // 获取指定枚举的所有值
        Array values = Enum.GetValues(typeof(T));
        
        // 返回一个随机的值
        return (T)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }
}