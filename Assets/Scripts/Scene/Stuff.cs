using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stuff : MonoBehaviour
{
    public StuffData stuffData;
    public StuffType stuffType;

    public object stuffSpecies;
    
    public void SetUpStuff(StuffData stuffData,Vector3 stuffPosition)
    {
        this.stuffData = stuffData;
        stuffType = stuffData.stuffType;
        transform.position = stuffPosition;
        Instantiate(stuffData.stuffModel, transform);

        stuffSpecies = stuffType switch
        {
            StuffType.水果 => StaticTools.GetRandomEnumValue<水果>(),
            StuffType.玩具 => StaticTools.GetRandomEnumValue<玩具>(),
            _ => throw new ArgumentOutOfRangeException()
        };
        stuffData.stuffSpecies = stuffSpecies;
    }
}

public enum StuffType
{
    水果,
    玩具,
}

public enum 水果
{
    圣女果,
    樱桃,
    苹果
}

public enum 玩具
{
    小汽车,
    小马,
    小熊
}



