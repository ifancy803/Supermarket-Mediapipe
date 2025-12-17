using System.Collections.Generic;
using UnityEngine;

public class SelectorManager : Singleton<SelectorManager>
{
    public List<GameObject> selectors = new();
    public GameObject currentShelf = null;
    
    // 当前选中的行列
    private int currentRow = 0;
    private int currentCol = 0;
    
    // 引用StuffGenerator以获取二维数组
    private StuffGenerator stuffGenerator;
    
    private void Update()
    {
        HandleKeyboardInput();
    }
    
    /// <summary>
    /// 处理键盘输入
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (stuffGenerator == null || stuffGenerator.currentStuffGrid == null)
            return;
            
        bool inputHandled = false;
        
        //TODO:上下左右位移根据视觉修改
        
        // 上键
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            inputHandled = MoveSelector(1, 0); // 向上移动（行-1）
        }
        // 下键
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            inputHandled = MoveSelector(-1, 0); // 向下移动（行+1）
        }
        // 左键
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            inputHandled = MoveSelector(0, -1); // 向左移动（列-1）
        }
        // 右键
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            inputHandled = MoveSelector(0, 1); // 向右移动（列+1）
        }
        
        // 数字键选择
        // if (Input.GetKeyDown(KeyCode.Alpha1))
        // {
        //     inputHandled = SelectSelector(0, 0);
        // }
        // else if (Input.GetKeyDown(KeyCode.Alpha2))
        // {
        //     inputHandled = SelectSelector(0, 1);
        // }
        // else if (Input.GetKeyDown(KeyCode.Alpha3))
        // {
        //     inputHandled = SelectSelector(1, 0);
        // }
        // else if (Input.GetKeyDown(KeyCode.Alpha4))
        // {
        //     inputHandled = SelectSelector(1, 1);
        // }
    }
    
    /// <summary>
    /// 移动选择器
    /// </summary>
    /// <param name="rowDelta">行变化量</param>
    /// <param name="colDelta">列变化量</param>
    /// <returns>是否成功移动</returns>
    private bool MoveSelector(int rowDelta, int colDelta)
    {
        int newRow = currentRow + rowDelta;
        int newCol = currentCol + colDelta;
        
        return SelectSelector(newRow, newCol);
    }
    
    /// <summary>
    /// 选择指定行列的选择器
    /// </summary>
    /// <param name="row">目标行</param>
    /// <param name="col">目标列</param>
    /// <returns>是否成功选择</returns>
    public bool SelectSelector(int row, int col)
    {
        // 检查边界
        if (stuffGenerator == null || stuffGenerator.currentStuffGrid == null)
        {
            Debug.LogWarning("StuffGenerator或二维数组未初始化");
            return false;
        }
        
        int maxRows = stuffGenerator.currentStuffGrid.GetLength(0);
        int maxCols = stuffGenerator.currentStuffGrid.GetLength(1);
        
        if (row < 0 || row >= maxRows || col < 0 || col >= maxCols)
        {
            Debug.Log($"目标位置[{row},{col}]超出边界，有效范围：[0,{maxRows-1}]x[0,{maxCols-1}]");
            return false;
        }
        
        // 检查该位置是否有物品
        GameObject targetShelf = stuffGenerator.GetStuffAt(row, col);
        if (targetShelf == null)
        {
            Debug.Log($"目标位置[{row},{col}]没有物品");
            return false;
        }
        
        // 获取ItemSelector组件
        ItemSelector itemSelector = targetShelf.GetComponent<ItemSelector>();
        if (itemSelector == null)
        {
            Debug.LogWarning($"目标物品[{row},{col}]没有ItemSelector组件");
            return false;
        }
        
        // 更新当前选中位置
        currentRow = row;
        currentCol = col;
        currentShelf = targetShelf;
        
        // 显示新的选择器
        itemSelector.SetSelectorVisible(true);
        
        Debug.Log($"已选择位置[{currentRow},{currentCol}]");
        
        return true;
    }
    
    /// <summary>
    /// 获取当前选中的相邻物品
    /// </summary>
    /// <param name="includeDiagonals">是否包含斜对角</param>
    /// <returns>相邻物品列表</returns>
    public List<GameObject> GetAdjacentSelectedShelves(bool includeDiagonals = false)
    {
        if (stuffGenerator == null)
            return new List<GameObject>();
            
        return stuffGenerator.GetAdjacentShelves(currentRow, currentCol, includeDiagonals);
    }
    
    /// <summary>
    /// 获取当前选中位置
    /// </summary>
    public Vector2Int GetCurrentPosition()
    {
        return new Vector2Int(currentRow, currentCol);
    }
    
    /// <summary>
    /// 获取当前选中物品
    /// </summary>
    public GameObject GetCurrentShelf()
    {
        return currentShelf;
    }
    
    /// <summary>
    /// 选择相邻的架子（方便外部调用）
    /// </summary>
    public bool SelectAdjacent(int rowDelta, int colDelta)
    {
        return MoveSelector(rowDelta, colDelta);
    }
    
    /// <summary>
    /// 强制选择指定位置的Selector（不检查是否有物品）
    /// </summary>
    public bool ForceSelectSelector(int row, int col)
    {
        currentRow = row;
        currentCol = col;
        
        // 尝试查找该位置的物品
        GameObject targetShelf = stuffGenerator?.GetStuffAt(row, col);
        if (targetShelf != null)
        {
            currentShelf = targetShelf;
            var itemSelector = targetShelf.GetComponent<ItemSelector>();
            if (itemSelector != null)
            {
                itemSelector.SetSelectorVisible(true);
                return true;
            }
        }
        
        // 如果没有物品，至少更新位置信息
        Debug.Log($"位置[{row},{col}]没有物品或ItemSelector");
        return false;
    }
    
    // 原有的方法保持不变
    public void UpdateSelectors()
    {
        selectors.Clear();
        foreach (var selector in GameObject.FindGameObjectsWithTag("Stuff"))
        {
            if(selector!=null)
                selectors.Add(selector.gameObject);
        }
    }
    
    public void OnNewGameState()
    {

            // 查找StuffGenerator
            stuffGenerator = FindObjectOfType<StuffGenerator>();
            if (stuffGenerator == null)
            {
                Debug.LogError("找不到StuffGenerator！");
            }
        
            // 初始选择第一个Selector
            SelectSelector(currentRow, currentCol);
        //
        // GameObject.Find("Shelf_[0,0]").GetComponent<ItemSelector>().SetSelectorVisible(true);
    }
    
}