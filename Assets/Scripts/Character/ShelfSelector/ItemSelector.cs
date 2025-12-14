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

    private GameObject currentSelectorUI;
    private Canvas selectCanvas;
    private RectTransform uiRectTransform;

    void Start()
    {
        // 找到Canvas
        selectCanvas = GameObject.FindGameObjectWithTag("SelectorCanvas")?.GetComponent<Canvas>();
        if (selectCanvas == null)
        {
            Debug.LogError("找不到 SelectorCanvas!");
            return;
        }
        selectCanvas.worldCamera = Camera.main;

        // 检查Canvas的渲染模式
        if (selectCanvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogWarning("SelectorCanvas 不是 World Space 模式！");
        }

        // 实例化UI
        if (selectorUIPrefab != null)
        {
            currentSelectorUI = Instantiate(selectorUIPrefab, selectCanvas.transform);
            
            // 获取RectTransform
            uiRectTransform = currentSelectorUI.GetComponent<RectTransform>();
            
            if (uiRectTransform == null)
            {
                Debug.LogError("UI预制体缺少RectTransform组件！");
                return;
            }
            
            // 设置UI初始属性
            InitializeUI();
            
            // 初始隐藏
            SetSelectorVisible(false);
            //currentSelectorUI.SetActive(false);
        }
    }

    void InitializeUI()
    {
        // 重置本地变换
        uiRectTransform.localRotation = Quaternion.identity;
        uiRectTransform.localScale = Vector3.one * uiScale;
        
        // 设置初始位置
        UpdateUIPosition();
    }

    void Update()
    {
        // 如果需要UI始终面向相机（可选）
        // if (currentSelectorUI != null && currentSelectorUI.activeSelf)
        // {
        //     UpdateUIPosition();
        //     //FaceToCamera();
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

    // void FaceToCamera()
    // {
    //     // 让UI始终面向主相机
    //     if (Camera.main != null)
    //     {
    //         currentSelectorUI.transform.LookAt(currentSelectorUI.transform.position + 
    //                                          Camera.main.transform.rotation * Vector3.forward,
    //                                          Camera.main.transform.rotation * Vector3.up);
    //     }
    // }

    public void SetSelectorVisible(bool isVisible)
    {
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
                //UpdateUIPosition();
                //FaceToCamera();
                
                // 调试：检查UI状态
                Debug.Log($"Selector 状态: 位置={uiRectTransform.position}, " +
                         $"缩放={uiRectTransform.localScale}, " +
                         $"激活={currentSelectorUI.activeSelf}");
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
            Debug.Log($"Canvas: {selectCanvas?.name}, " +
                     $"渲染模式: {selectCanvas?.renderMode}, " +
                     "层级: " + currentSelectorUI.transform.GetSiblingIndex());
        }
    }

    // 在编辑器中可视化偏移点（可选）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + positionOffset, 0.1f);
    }
}