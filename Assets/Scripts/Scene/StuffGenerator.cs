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
    
    // 新增：二维数组存储行列信息
    public GameObject[,] currentStuffGrid;
    private List<float> distinctRows;
    private List<float> distinctCols;
    
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
        
        // 如果二维数组不为空，先清理
        if (currentStuffGrid != null)
        {
            Array.Clear(currentStuffGrid, 0, currentStuffGrid.Length);
        }
        
        // 创建新物品并组织成二维数组
        OrganizeStuffIntoGrid();
    }

    /// <summary>
    /// 将物品按位置组织成二维数组
    /// </summary>
    private void OrganizeStuffIntoGrid()
    {
        if (stuffPositionList.Count == 0)
            return;
        
        // 获取所有生成点的Y坐标（行）和X坐标（列）
        // 使用精确的坐标值而不是四舍五入，以处理非整数坐标
        distinctRows = stuffPositionList.Select(p => p.y).Distinct().OrderBy(y => y).ToList();
        distinctCols = stuffPositionList.Select(p => p.x).Distinct().OrderBy(x => x).ToList();
        
        // 创建二维数组
        currentStuffGrid = new GameObject[distinctRows.Count, distinctCols.Count];
        
        // 生成物品并放入对应位置
        foreach (var pos in stuffPositionList)
        {
            var stuff = Instantiate(stuffPrefab, transform);
            
            //var currentStuffDataList = stuffDataList.FindAll(data => data.stuffType == stuffType);
            var currentStuffDataList = stuffDataList;
            if (currentStuffDataList.Count > 0)
            {
                var currentStuffData = currentStuffDataList[Random.Range(0, currentStuffDataList.Count)];
                stuff.SetUpStuff(currentStuffData, pos);
                
                // 计算行列索引
                int rowIndex = GetRowIndex(pos.y);
                int colIndex = GetColIndex(pos.x);
                
                // 放入二维数组
                if (rowIndex >= 0 && rowIndex < distinctRows.Count && 
                    colIndex >= 0 && colIndex < distinctCols.Count)
                {
                    currentStuffGrid[rowIndex, colIndex] = stuff.gameObject;
                    
                    // 重要：为架子命名为二维数组索引
                    stuff.gameObject.name = $"Shelf_[{rowIndex},{colIndex}]";
                    
                    // 可选：添加一个组件来存储行列信息
                    var shelfInfo = stuff.gameObject.AddComponent<ShelfInfo>();
                    shelfInfo.Row = rowIndex;
                    shelfInfo.Col = colIndex;
                }
                
                currentStuffList.Add(stuff.gameObject);
            }
            else
            {
                Debug.LogWarning("没有找到对应的StuffData！");
            }
        }
        
        PrintGridInfo(); // 打印调试信息
        

    }

    /// <summary>
    /// 根据Y坐标获取行索引
    /// </summary>
    private int GetRowIndex(float y)
    {
        // 使用容差值来处理浮点数精度问题
        float tolerance = 0.01f;
        for (int i = 0; i < distinctRows.Count; i++)
        {
            if (Mathf.Abs(distinctRows[i] - y) < tolerance)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 根据X坐标获取列索引
    /// </summary>
    private int GetColIndex(float x)
    {
        // 使用容差值来处理浮点数精度问题
        float tolerance = 0.01f;
        for (int i = 0; i < distinctCols.Count; i++)
        {
            if (Mathf.Abs(distinctCols[i] - x) < tolerance)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 根据行列获取物品
    /// </summary>
    public GameObject GetStuffAt(int row, int col)
    {
        if (currentStuffGrid == null || 
            row < 0 || row >= currentStuffGrid.GetLength(0) || 
            col < 0 || col >= currentStuffGrid.GetLength(1))
        {
            Debug.LogWarning($"无效的行列索引: ({row}, {col})");
            return null;
        }
        
        return currentStuffGrid[row, col];
    }

    /// <summary>
    /// 根据位置获取行列索引
    /// </summary>
    public bool TryGetGridIndex(Vector3 position, out int row, out int col)
    {
        row = -1;
        col = -1;
        
        if (currentStuffGrid == null || distinctRows == null || distinctCols == null)
            return false;
        
        row = GetRowIndex(position.y);
        col = GetColIndex(position.x);
        
        return row >= 0 && col >= 0;
    }

    /// <summary>
    /// 根据游戏对象获取其行列索引
    /// </summary>
    public bool TryGetGridIndex(GameObject shelfObject, out int row, out int col)
    {
        row = -1;
        col = -1;
        
        if (shelfObject == null)
            return false;
        
        // 方法1：通过名称解析
        string name = shelfObject.name;
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
                // 名称解析失败，尝试其他方法
            }
        }
        
        // 方法2：通过ShelfInfo组件
        var shelfInfo = shelfObject.GetComponent<ShelfInfo>();
        if (shelfInfo != null)
        {
            row = shelfInfo.Row;
            col = shelfInfo.Col;
            return true;
        }
        
        // 方法3：遍历二维数组查找
        if (currentStuffGrid != null)
        {
            for (int r = 0; r < currentStuffGrid.GetLength(0); r++)
            {
                for (int c = 0; c < currentStuffGrid.GetLength(1); c++)
                {
                    if (currentStuffGrid[r, c] == shelfObject)
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
    /// 打印二维数组内容（用于调试）
    /// </summary>
    public void PrintGridInfo()
    {
        if (currentStuffGrid == null)
        {
            Debug.Log("二维数组为空");
            return;
        }
        
        int rows = currentStuffGrid.GetLength(0);
        int cols = currentStuffGrid.GetLength(1);
        
        
        for (int row = 0; row < rows; row++)
        {
            string rowInfo = $"行 {row}: ";
            for (int col = 0; col < cols; col++)
            {
                if (currentStuffGrid[row, col] != null)
                {
                    rowInfo += $"[{row},{col}]";
                }
                else
                {
                    rowInfo += "[   ]";
                }
                if (col < cols - 1) rowInfo += " ";
            }
        }
    }

    /// <summary>
    /// 获取所有非空架子的行列索引
    /// </summary>
    public List<Vector2Int> GetAllShelfIndices()
    {
        List<Vector2Int> indices = new List<Vector2Int>();
        
        if (currentStuffGrid == null)
            return indices;
        
        for (int row = 0; row < currentStuffGrid.GetLength(0); row++)
        {
            for (int col = 0; col < currentStuffGrid.GetLength(1); col++)
            {
                if (currentStuffGrid[row, col] != null)
                {
                    indices.Add(new Vector2Int(row, col));
                }
            }
        }
        
        return indices;
    }

    /// <summary>
    /// 根据行列索引获取相邻架子
    /// </summary>
    public List<GameObject> GetAdjacentShelves(int row, int col, bool includeDiagonals = false)
    {
        List<GameObject> adjacent = new List<GameObject>();
        
        // 上
        if (row > 0 && currentStuffGrid[row - 1, col] != null)
            adjacent.Add(currentStuffGrid[row - 1, col]);
        
        // 下
        if (row < currentStuffGrid.GetLength(0) - 1 && currentStuffGrid[row + 1, col] != null)
            adjacent.Add(currentStuffGrid[row + 1, col]);
        
        // 左
        if (col > 0 && currentStuffGrid[row, col - 1] != null)
            adjacent.Add(currentStuffGrid[row, col - 1]);
        
        // 右
        if (col < currentStuffGrid.GetLength(1) - 1 && currentStuffGrid[row, col + 1] != null)
            adjacent.Add(currentStuffGrid[row, col + 1]);
        
        if (includeDiagonals)
        {
            // 左上
            if (row > 0 && col > 0 && currentStuffGrid[row - 1, col - 1] != null)
                adjacent.Add(currentStuffGrid[row - 1, col - 1]);
            
            // 右上
            if (row > 0 && col < currentStuffGrid.GetLength(1) - 1 && currentStuffGrid[row - 1, col + 1] != null)
                adjacent.Add(currentStuffGrid[row - 1, col + 1]);
            
            // 左下
            if (row < currentStuffGrid.GetLength(0) - 1 && col > 0 && currentStuffGrid[row + 1, col - 1] != null)
                adjacent.Add(currentStuffGrid[row + 1, col - 1]);
            
            // 右下
            if (row < currentStuffGrid.GetLength(0) - 1 && col < currentStuffGrid.GetLength(1) - 1 && currentStuffGrid[row + 1, col + 1] != null)
                adjacent.Add(currentStuffGrid[row + 1, col + 1]);
        }
        
        return adjacent;
    }

    public void UpdateStuff(object value)
    {
        SetUpAllStuff();
        SelectorManager.Instance.UpdateSelectors();
        SelectorManager.Instance.AutoSelectNextAvailableShelf(0,0);
    }
}

/// <summary>
/// 架子信息组件，用于存储行列索引
/// </summary>
public class ShelfInfo : MonoBehaviour
{
    [SerializeField] private int row;
    [SerializeField] private int col;
    
    public int Row 
    { 
        get => row; 
        set => row = value; 
    }
    
    public int Col 
    { 
        get => col; 
        set => col = value; 
    }
}
