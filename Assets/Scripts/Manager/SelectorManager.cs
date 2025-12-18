using System.Collections.Generic;
using UnityEngine;

public class SelectorManager : Singleton<SelectorManager>
{
    public List<GameObject> selectors = new();
    public GameObject currentShelf = null;
    
    // 当前选中的行列
    private int currentRow = 0;
    private int currentCol = 0;
    
    // 新增：得分/扣分事件（可选）
    public delegate void ScoreChangedHandler(int changeAmount, bool isCorrect, string message);
    public event ScoreChangedHandler OnScoreChanged;
    
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

        #region 按键控制

        // 上键
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            inputHandled = MoveSelector(1, 0); 
        }
        // 下键
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            inputHandled = MoveSelector(-1, 0); 
        }
        // 左键
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            inputHandled = MoveSelector(0, -1); 
        }
        // 右键
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            inputHandled = MoveSelector(0, 1); 
        }
        // Enter键 - 销毁当前选中的物品并加分
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            CheckAndDestroyCurrentShelf();
        }

        #endregion

        //TODO:姿态识别方法接入
        #region 姿势识别

        // // 上键
        // if (MediapipeUp())
        // {
        //     inputHandled = MoveSelector(1, 0); 
        // }
        // // 下键
        // else if (MediapipeDown())
        // {
        //     inputHandled = MoveSelector(-1, 0); 
        // }
        // // 左键
        // else if (MediapipeLeft())
        // {
        //     inputHandled = MoveSelector(0, -1); 
        // }
        // // 右键
        // else if (MediapipeRight())
        // {
        //     inputHandled = MoveSelector(0, 1); 
        // }
        // // Enter键 - 销毁当前选中的物品并加分
        // else if (MediapipeAffirm())
        // {
        //     CheckAndDestroyCurrentShelf();
        // }

        #endregion
    }

    

    

    
        /// <summary>
    /// 检查并销毁当前选中的架子，根据类型加减分
    /// </summary>
    private void CheckAndDestroyCurrentShelf()
    {
        if (currentShelf == null)
        {
            Debug.Log("当前没有选中的架子");
            ShowMessage("未选中任何架子", Color.yellow);
            return;
        }
        
        // 获取当前架子的Stuff组件
        Stuff currentStuff = currentShelf.GetComponent<Stuff>();
        if (currentStuff == null)
        {
            Debug.LogWarning("当前选中的架子没有Stuff组件");
            ShowMessage("错误：该物品无效", Color.red);
            return;
        }
        
        // 获取目标枚举类型
        if (GameManager.Instance == null || 
            GameManager.Instance.rooms == null || 
            GameManager.Instance.currentRoomIndex < 1 ||
            GameManager.Instance.currentRoomIndex > GameManager.Instance.rooms.Count)
        {
            Debug.LogError("GameManager或关卡数据无效");
            return;
        }
        
        Room currentRoom = GameManager.Instance.rooms[GameManager.Instance.currentRoomIndex - 1];
        System.Enum aimEnum = currentRoom.GetAimEnumType();
        
        if (aimEnum == null)
        {
            Debug.LogError("无法获取目标枚举类型");
            ShowMessage("错误：目标类型未设置", Color.red);
            return;
        }
        
        // 比较枚举类型
        System.Enum currentEnum = currentStuff.stuffType as System.Enum;
        bool isCorrect = currentEnum != null && currentEnum.Equals(aimEnum);
        
        // 计算分数变化
        int scoreChange = isCorrect ? 1 : -1;
        int newScore = Mathf.Max(0, GameManager.Instance.score + scoreChange);
        
        // 更新分数
        GameManager.Instance.score = newScore;
        
        // 显示提示信息
        string itemName = currentEnum?.ToString() ?? "未知类型";
        Debug.Log(currentEnum?.ToString());
        
        string aimName = aimEnum.ToString();
        string message = isCorrect ? 
            $"正确！{itemName} +1分" : 
            $"错误！{itemName} ≠ {aimName} -1分";
        
        Color messageColor = isCorrect ? Color.green : Color.red;
        ShowMessage(message, messageColor);
        
        Debug.Log($"{message}，当前分数: {GameManager.Instance.score}");
        
        // 触发得分事件
        OnScoreChanged?.Invoke(scoreChange, isCorrect, message);
        
        // 销毁当前架子
        DestroyCurrentShelf();
    }
    
    /// <summary>
    /// 销毁当前选中的架子
    /// </summary>
    private void DestroyCurrentShelf()
    {
        if (currentShelf == null) return;
        
        // 从二维数组中获取行列索引
        if (TryGetCurrentGridPosition(out int row, out int col))
        {
            // 从二维数组中移除引用
            if (stuffGenerator != null && stuffGenerator.currentStuffGrid != null)
            {
                if (row >= 0 && row < stuffGenerator.currentStuffGrid.GetLength(0) &&
                    col >= 0 && col < stuffGenerator.currentStuffGrid.GetLength(1))
                {
                    // 销毁游戏对象
                    Debug.Log($"销毁架子 [{row},{col}]");
                    Destroy(currentShelf);
                    
                    // 清空二维数组中的引用
                    stuffGenerator.currentStuffGrid[row, col] = null;
                    
                    // 从列表中移除
                    if (stuffGenerator.currentStuffList.Contains(currentShelf))
                    {
                        stuffGenerator.currentStuffList.Remove(currentShelf);
                    }
                }
            }
        }
        
        // 自动选择下一个可用的架子
        if (TryGetCurrentGridPosition(out int currentRow, out int currentCol))
        {
            AutoSelectNextAvailableShelf(currentRow, currentCol);
        }
        else
        {
            // 如果无法获取当前位置，尝试从(0,0)开始查找
            AutoSelectNextAvailableShelf(0, 0);
        }
    }
    
    /// <summary>
    /// 显示提示消息（可以扩展为UI显示）
    /// </summary>
    private void ShowMessage(string message, Color color)
    {
        // 这里可以替换为你自己的UI显示逻辑
        Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>");
        
        // 示例：如果你有UI管理器，可以这样调用
        // UIManager.Instance.ShowMessage(message, color, 2f);
    }
    
    /// <summary>
    /// 获取当前选中架子的网格位置
    /// </summary>
    private bool TryGetCurrentGridPosition(out int row, out int col)
    {
        row = -1;
        col = -1;
        
        if (currentShelf == null)
            return false;
            
        // 方法1：从ShelfInfo组件获取
        var shelfInfo = currentShelf.GetComponent<ShelfInfo>();
        if (shelfInfo != null)
        {
            row = shelfInfo.Row;
            col = shelfInfo.Col;
            return true;
        }
        
        // 方法2：从名称解析
        string name = currentShelf.name;
        if (name.Contains("[") && name.Contains(",") && name.Contains("]"))
        {
            try
            {
                int start = name.IndexOf("[") + 1;
                int end = name.IndexOf("]");
                string indexStr = name.Substring(start, end - start);
                string[] parts = indexStr.Split(',');
                
                if (parts.Length == 2 && 
                    int.TryParse(parts[0], out row) && 
                    int.TryParse(parts[1], out col))
                {
                    return true;
                }
            }
            catch
            {
                // 名称解析失败
            }
        }
        
        // 方法3：遍历二维数组查找
        if (stuffGenerator != null && stuffGenerator.currentStuffGrid != null)
        {
            for (int r = 0; r < stuffGenerator.currentStuffGrid.GetLength(0); r++)
            {
                for (int c = 0; c < stuffGenerator.currentStuffGrid.GetLength(1); c++)
                {
                    if (stuffGenerator.currentStuffGrid[r, c] == currentShelf)
                    {
                        row = r;
                        col = c;
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 自动选择下一个可用的架子
    /// </summary>
    private void AutoSelectNextAvailableShelf(int destroyedRow, int destroyedCol)
    {
        if (stuffGenerator == null || stuffGenerator.currentStuffGrid == null)
            return;
        
        int rows = stuffGenerator.currentStuffGrid.GetLength(0);
        int cols = stuffGenerator.currentStuffGrid.GetLength(1);
        
        // 尝试先选择右侧的架子
        for (int col = destroyedCol + 1; col < cols; col++)
        {
            if (stuffGenerator.currentStuffGrid[destroyedRow, col] != null)
            {
                SelectSelector(destroyedRow, col);
                return;
            }
        }
        
        // 尝试选择左侧的架子
        for (int col = destroyedCol - 1; col >= 0; col--)
        {
            if (stuffGenerator.currentStuffGrid[destroyedRow, col] != null)
            {
                SelectSelector(destroyedRow, col);
                return;
            }
        }
        
        // 尝试选择下方的架子
        for (int row = destroyedRow + 1; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (stuffGenerator.currentStuffGrid[row, col] != null)
                {
                    SelectSelector(row, col);
                    return;
                }
            }
        }
        
        // 尝试选择上方的架子
        for (int row = destroyedRow - 1; row >= 0; row--)
        {
            for (int col = 0; col < cols; col++)
            {
                if (stuffGenerator.currentStuffGrid[row, col] != null)
                {
                    SelectSelector(row, col);
                    return;
                }
            }
        }
        
        // 如果没有找到其他架子，清空当前选中
        currentShelf = null;
        currentRow = -1;
        currentCol = -1;
        Debug.Log("所有架子都已被销毁");
        
        // 可以触发游戏结束或关卡完成事件
        // GameManager.Instance.CheckLevelComplete();
    }
    
    /// <summary>
    /// 销毁当前选中的架子并增加分数
    /// </summary>
    private void DestroyCurrentShelfAndAddScore()
    {
        if (currentShelf == null)
        {
            Debug.Log("当前没有选中的架子");
            return;
        }
        
        // 从二维数组中获取行列索引
        if (TryGetCurrentGridPosition(out int row, out int col))
        {
            // 从二维数组中移除引用
            if (stuffGenerator != null && stuffGenerator.currentStuffGrid != null)
            {
                if (row >= 0 && row < stuffGenerator.currentStuffGrid.GetLength(0) &&
                    col >= 0 && col < stuffGenerator.currentStuffGrid.GetLength(1))
                {
                    // 销毁游戏对象
                    if (currentShelf != null)
                    {
                        Debug.Log($"销毁架子 [{row},{col}]");
                        Destroy(currentShelf);
                        
                        // 清空二维数组中的引用
                        stuffGenerator.currentStuffGrid[row, col] = null;
                        
                        // 从列表中移除
                        if (stuffGenerator.currentStuffList.Contains(currentShelf))
                        {
                            stuffGenerator.currentStuffList.Remove(currentShelf);
                        }
                    }
                    
                    // 增加分数
                    GameManager.Instance.score += 1;
                    Debug.Log($"分数增加，当前分数: {GameManager.Instance.score}");
                    
                    // 自动选择下一个可用的架子
                    AutoSelectNextAvailableShelf(row, col);
                    
                    //TODO: 触发物品更新事件（如果需要）
                    //GameManager.Instance.updateStuffEvent?.RaiseEvent(null, GameManager.Instance);
                }
            }
        }
        else
        {
            Debug.LogWarning("无法获取当前架子的网格位置");
        }
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
        
        //Debug.Log($"已选择位置[{currentRow},{currentCol}]");
        
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
        
        // GameObject.Find("Shelf_[0,0]").GetComponent<ItemSelector>().SetSelectorVisible(true);
    }
    
}