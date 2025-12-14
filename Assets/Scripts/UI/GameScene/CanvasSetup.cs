using UnityEngine;
using UnityEngine.UI;

public class CanvasSetup : MonoBehaviour
{
    public static CanvasSetup Instance { get; private set; }
    
    [Header("Canvas Settings")]
    public float canvasScale = 1f;
    public Vector3 canvasPosition = Vector3.zero;
    
    private Canvas worldCanvas;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        SetupCanvas();
    }
    
    void SetupCanvas()
    {
        worldCanvas = GetComponent<Canvas>();
        if (worldCanvas == null)
            worldCanvas = gameObject.AddComponent<Canvas>();
            
        worldCanvas.renderMode = RenderMode.WorldSpace;
        
        // 设置Canvas大小和位置
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(1000, 1000); // 设置足够大的画布
            rt.position = canvasPosition;
            rt.localScale = Vector3.one * canvasScale;
        }
        
        // 确保有相机
        if (worldCanvas.worldCamera == null)
            worldCanvas.worldCamera = Camera.main;
            
        // 添加Canvas Scaler
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;
        }
        
        // 添加Graphic Raycaster
        GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
            gameObject.AddComponent<GraphicRaycaster>();
    }
    
    void Update()
    {
        // 确保Canvas始终有相机
        if (worldCanvas.worldCamera == null)
            worldCanvas.worldCamera = Camera.main;
    }
    
    public void SetCanvasCamera(Camera cam)
    {
        if (worldCanvas != null)
            worldCanvas.worldCamera = cam;
    }
}