using System;
using UnityEngine;

public class ItemSelector : MonoBehaviour
{
    [Tooltip("拖入你的选择器UI预制体")]
    public GameObject selectorUIPrefab; // 在Inspector面板中拖入预制体

    public Canvas selectCanvas;

    [Tooltip("UI相对于3D物体的本地位置偏移")]
    public Vector3 positionOffset = new Vector3(0, 0, -3f); // 例如，在物品上方0.5米

    private GameObject currentSelectorUI;

    private void Awake()
    {
        selectCanvas = GameObject.FindGameObjectWithTag("SelectorCanvas").GetComponent<Canvas>();
    }

    void Start()
    {
        if (selectorUIPrefab != null)
        {
            // 实例化UI，并作为当前物体的子物体
            currentSelectorUI = Instantiate(selectorUIPrefab, selectCanvas.transform);
            // 设置UI的本地位置和旋转
            currentSelectorUI.transform.localPosition = positionOffset;
            currentSelectorUI.transform.localRotation = Quaternion.identity;
            // 初始设置为隐藏
            SetSelectorVisible(false);
        }
    }

    // 外部调用此方法来显示/隐藏选择器
    public void SetSelectorVisible(bool isVisible)
    {
        if (currentSelectorUI != null)
        {
            currentSelectorUI.SetActive(isVisible);
        }
    }

    [ContextMenu("选择")]
    public void SetSelector()
    {
        SetSelectorVisible(true);
    }

    // 如果需要每帧更新（例如需要始终面向摄像机），可以在LateUpdate中处理
    // void LateUpdate()
    // {
    //     if (currentSelectorUI != null && currentSelectorUI.activeInHierarchy)
    //     {
    //         // 让选择器UI始终面向摄像机（Billboard效果）
    //         if (Camera.main != null)
    //             currentSelectorUI.transform.LookAt(
    //                 currentSelectorUI.transform.position +
    //                 Camera.main.transform.rotation * Vector3.forward,
    //                 Camera.main.transform.rotation * Vector3.up
    //             );
    //     }
    // }
}