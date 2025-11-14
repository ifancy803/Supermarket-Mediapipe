using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class StuffGenerator : MonoBehaviour
{
    public Stuff stuffPrefab;
    public StuffType stuffType;
    
    public List<StuffData> stuffDataList = new();
    public List<Vector3> stuffPositionList = new();
    public List<GameObject> currentStuffList = new();
    
    private void OnEnable()
    {
        stuffPositionList.Clear();
        
        var instantiatePoint = GetComponentsInChildren<Transform>()
            .Where(t => t.CompareTag("InstantiatePoint"))
            .Select(t => t.gameObject)
            .ToArray();
        
        foreach (var point in instantiatePoint)
        {
            stuffPositionList.Add(point.transform.position);
        }
        
        InitializeStuff();
    }


    /// <summary>
    /// 初始化调用
    /// </summary>
    public void InitializeStuff()
    {
        SetUpAllStuff();
    }

    // 刷新创建物品
    public void SetUpAllStuff()
    {
        // 清理现有物品
        if (currentStuffList.Count > 0)
        {
            foreach (var stuff in currentStuffList.ToList())
            {
                if (stuff != null)
                    Destroy(stuff);
            }
            currentStuffList.Clear();
        }
        
        // 创建新物品
        foreach (var pos in stuffPositionList)
        {
            var stuff = Instantiate(stuffPrefab, transform);
            
            var currentStuffDataList = stuffDataList.FindAll(data => data.stuffType == stuffType);
            if (currentStuffDataList.Count > 0)
            {
                var currentStuffData = currentStuffDataList[Random.Range(0, currentStuffDataList.Count)];
                stuff.SetUpStuff(currentStuffData, pos);
                currentStuffList.Add(stuff.gameObject);
            }
            else
            {
                Debug.LogWarning("没有找到对应的StuffData！");
            }
        }
    }

    public void UpdateStuff(object value)
    {
        SetUpAllStuff();
    }
}