using System;
using UnityEngine;

public class ItemSelector : MonoBehaviour
{
    [Tooltip("拖入你的选择器UI预制体")]
    public GameObject selectorUIPrefab;

    [Tooltip("UI相对于3D物体的世界位置偏移")]
    public Vector3 positionOffset = new Vector3(0, 0.25f, 0.3f);

    [Tooltip("UI大小缩放")]
    public float uiScale = 0.1f; // World Canvas 通常需要缩放

    public GameObject currentSelectorUI;
    private Canvas selectCanvas;
    private RectTransform uiRectTransform;
    
    // 新增：缓存行列信息
    private int shelfRow = -1;
    private int shelfCol = -1;

    void Start()
    {
        // 找到Canvas
        selectCanvas = GameObject.FindGameObjectWithTag("SelectorCanvas")?.GetComponent<Canvas>();
        if (selectCanvas == null)
        {
            Debug.LogError("找不到 SelectorCanvas!");
            return;
        }

        // 检查Canvas的渲染模式
        if (selectCanvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogWarning("SelectorCanvas 不是 World Space 模式！");
        }

        // 尝试从ShelfInfo获取行列信息
        var shelfInfo = GetComponent<ShelfInfo>();
        if (shelfInfo != null)
        {
            shelfRow = shelfInfo.Row;
            shelfCol = shelfInfo.Col;
        }
        
        // 如果ShelfInfo不存在，尝试从名称解析
        if (shelfRow < 0 || shelfCol < 0)
        {
            ParseRowColFromName();
        }

        // 在实例化之前，检查是否已经存在相同行列的Selector
        string selectorName = $"{shelfRow},{shelfCol}";
        GameObject existingSelector = FindSelectorByName(selectorName);
        
        if (existingSelector != null)
        {
            // 如果已经存在相同行列的Selector，则直接使用现有的
            Debug.Log($"发现已存在的Selector: {selectorName}，将复用该Selector");
            currentSelectorUI = existingSelector;
            uiRectTransform = currentSelectorUI.GetComponent<RectTransform>();
        }
        else if (selectorUIPrefab != null)
        {
            // 如果不存在相同行列的Selector，则创建新的
            currentSelectorUI = Instantiate(selectorUIPrefab, selectCanvas.transform);
            
            // 设置UI名称与shelf的行列信息一致
            UpdateUIName();
            
            // 获取RectTransform
            uiRectTransform = currentSelectorUI.GetComponent<RectTransform>();
            
            if (uiRectTransform == null)
            {
                Debug.LogError("UI预制体缺少RectTransform组件！");
                return;
            }
            
            // 设置UI初始属性
            InitializeUI();
        }
        
        // 初始隐藏
        //SetSelectorVisible(false);
    }
    
    /// <summary>
    /// 查找指定名称的Selector
    /// </summary>
    private GameObject FindSelectorByName(string name)
    {
        // 在Canvas下查找
        if (selectCanvas != null)
        {
            foreach (Transform child in selectCanvas.transform)
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }
        }
        
        // 也可以通过GameObject.Find查找
        GameObject foundObject = GameObject.Find(name);
        if (foundObject != null && foundObject.CompareTag("Selector"))
        {
            return foundObject;
        }
        
        return null;
    }
    
    /// <summary>
    /// 从游戏对象名称解析行列信息
    /// </summary>
    private void ParseRowColFromName()
    {
        string name = gameObject.name;
        
        // 方法1：从Shelf命名格式解析
        if (name.Contains("[") && name.Contains(",") && name.Contains("]"))
        {
            try
            {
                int start = name.IndexOf("[") + 1;
                int end = name.IndexOf("]");
                string indexStr = name.Substring(start, end - start);
                string[] parts = indexStr.Split(',');
                
                if (parts.Length == 2 && 
                    int.TryParse(parts[0], out int row) && 
                    int.TryParse(parts[1], out int col))
                {
                    shelfRow = row;
                    shelfCol = col;
                    Debug.Log($"从名称解析到行列: [{shelfRow},{shelfCol}]");
                    return;
                }
            }
            catch
            {
                // 名称解析失败，继续尝试其他方法
            }
        }
        
        // 方法2：从StuffGenerator的二维数组中查找
        var stuffGenerator = FindObjectOfType<StuffGenerator>();
        if (stuffGenerator != null)
        {
            if (stuffGenerator.TryGetGridIndex(gameObject, out int row, out int col))
            {
                shelfRow = row;
                shelfCol = col;
                Debug.Log($"从StuffGenerator获取到行列: [{shelfRow},{shelfCol}]");
                return;
            }
        }
        
        Debug.LogWarning($"无法解析对象 {name} 的行列信息");
    }
    
    /// <summary>
    /// 手动设置行列信息
    /// </summary>
    public void SetShelfInfo(int row, int col)
    {
        shelfRow = row;
        shelfCol = col;
        
        // 检查是否已经存在相同行列的Selector
        string selectorName = $"{row},{col}";
        GameObject existingSelector = FindSelectorByName(selectorName);
        
        if (existingSelector != null && existingSelector != currentSelectorUI)
        {
            // 如果已经存在相同行列的Selector，则销毁当前UI，使用现有的
            if (currentSelectorUI != null)
            {
                Destroy(currentSelectorUI);
            }
            currentSelectorUI = existingSelector;
            uiRectTransform = currentSelectorUI.GetComponent<RectTransform>();
            Debug.Log($"切换到已存在的Selector: {selectorName}");
        }
        else if (currentSelectorUI != null)
        {
            // 更新当前UI的名称
            UpdateUIName();
        }
    }
    
    /// <summary>
    /// 更新UI名称以匹配shelf的行列信息
    /// </summary>
    private void UpdateUIName()
    {
        if (currentSelectorUI != null)
        {
            currentSelectorUI.name = $"{shelfRow},{shelfCol}";
        }
    }

    void InitializeUI()
    {
        if (uiRectTransform != null)
        {
            // 重置本地变换
            uiRectTransform.localRotation = Quaternion.identity;
            uiRectTransform.localScale = Vector3.one * uiScale;
            
            // 设置初始位置
            UpdateUIPosition();
        }
    }

    void Update()
    {
        if(selectCanvas.worldCamera == null)
            selectCanvas.worldCamera = Camera.main;
            
        // 如果UI已激活，实时更新位置
        // if (currentSelectorUI != null && currentSelectorUI.activeSelf)
        // {
        //     UpdateUIPosition();
        // }
    }

    void UpdateUIPosition()
    {
        if (uiRectTransform != null)
        {
            // 设置UI的世界位置（相对于父物体）
            uiRectTransform.position = transform.position + positionOffset;
        }
    }

    public void SetSelectorVisible(bool isVisible)
    {
        if (currentSelectorUI == null)
            return;
            
        // 关闭所有Selector
        var allSelectors = GameObject.FindGameObjectsWithTag("Selector");
        foreach (var selector in allSelectors)
        {
            if (selector != currentSelectorUI)
            {
                selector.SetActive(false);
            }
        }
        
        // 设置当前Selector
        if (currentSelectorUI != null)
        {
            currentSelectorUI.SetActive(isVisible);
            
            if (isVisible)
            {
                // 更新UI位置和旋转
                // UpdateUIPosition();
                // FaceToCamera();
                
                // 调试信息
                Debug.Log($"显示Selector: {currentSelectorUI.name} " +
                         $"位置: {transform.position}, " +
                         $"行列: [{shelfRow},{shelfCol}]");
            }
        }
    }
    
    /// <summary>
    /// 使UI面向相机
    /// </summary>
    private void FaceToCamera()
    {
        if (selectCanvas != null && selectCanvas.worldCamera != null && uiRectTransform != null)
        {
            Vector3 directionToCamera = selectCanvas.worldCamera.transform.position - uiRectTransform.position;
            directionToCamera.y = 0; // 保持水平旋转
            if (directionToCamera != Vector3.zero)
            {
                uiRectTransform.rotation = Quaternion.LookRotation(-directionToCamera);
            }
        }
    }

    [ContextMenu("测试显示Selector")]
    public void TestShowSelector()
    {
        SetSelectorVisible(true);
        
        // 添加调试信息
        if (currentSelectorUI != null)
        {
            Debug.Log($"Selector名称: {currentSelectorUI.name}, " +
                     $"父物体: {transform.name}, " +
                     $"Canvas: {selectCanvas?.name}");
        }
    }
    
    /// <summary>
    /// 获取当前shelf的行列信息
    /// </summary>
    public Vector2Int GetShelfGridPosition()
    {
        return new Vector2Int(shelfRow, shelfCol);
    }
    
    /// <summary>
    /// 获取当前Selector的名称
    /// </summary>
    public string GetSelectorName()
    {
        return currentSelectorUI != null ? currentSelectorUI.name : "无Selector";
    }
    
    /// <summary>
    /// 清理和销毁Selector
    /// </summary>
    // private void OnDestroy()
    // {
    //     // 注意：这里要小心处理，因为Selector可能被多个架子共享
    //     // 只有当前Selector没有被其他架子引用时才能销毁
    //     if (currentSelectorUI != null)
    //     {
    //         // 检查是否有其他架子在使用这个Selector
    //         bool isSelectorUsedByOthers = false;
    //         
    //         var allSelectors = GameObject.FindGameObjectsWithTag("Selector");
    //         foreach (var selector in allSelectors)
    //         {
    //             if (selector == currentSelectorUI)
    //             {
    //                 // 查找是否有其他ItemSelector组件引用这个UI
    //                 var allItemSelectors = FindObjectsOfType<ItemSelector>();
    //                 foreach (var itemSelector in allItemSelectors)
    //                 {
    //                     if (itemSelector != this && itemSelector.currentSelectorUI == currentSelectorUI)
    //                     {
    //                         isSelectorUsedByOthers = true;
    //                         break;
    //                     }
    //                 }
    //                 break;
    //             }
    //         }
    //         
    //         // 如果没有其他架子使用这个Selector，则销毁
    //         if (!isSelectorUsedByOthers)
    //         {
    //             Destroy(currentSelectorUI);
    //             Debug.Log($"销毁Selector: {currentSelectorUI.name}");
    //         }
    //     }
    // }

    // 在编辑器中可视化偏移点（可选）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + positionOffset, 0.1f);
    }
}