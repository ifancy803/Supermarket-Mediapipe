using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ============================================================
    // 统一数据结构（你以后所有要保存的数据都可以放在这里）
    // ============================================================
    
    [Serializable]
    public class RegisterData
    {
        public string PlayerID;
        public string Name;
        public string Age;
        public string Gender;
        public string Education;
        public string MedicalHistory;
        public string DiseaseState;
        public string RegisterTime;
    }

    [Serializable]
    public class StageData
    {
        public string PlayerID;
        public int StageIndex;
        public int Score;
        public float UsedTime;
        public float UpdateGap;
        public bool Win;
        public string RecordTime;
    }

    // ============================================================
    // 路径管理（Windows Documents）
    // ============================================================
    
    public string GetBasePath()
    {
        string doc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string folder = Path.Combine(doc, "SupermarketGame");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        return folder;
    }

    public string GetPlayerFolder(string playerId)
    {
        string date = DateTime.Now.ToString("yyyyMMdd");
        string folder = Path.Combine(GetBasePath(), $"{date}_{playerId}");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        return folder;
    }

    // ============================================================
    // 泛型 CSV 保存器
    // ============================================================
    
    private void SaveToCSV<T>(string folder, string fileName, T data)
    {
        string path = Path.Combine(folder, fileName);
        StringBuilder sb = new StringBuilder();

        // 写表头
        if (!File.Exists(path))
        {
            string header = string.Join(",", typeof(T).GetFields().Select(f => f.Name));
            sb.AppendLine(header);
        }

        // 写行数据
        string row = string.Join(",", typeof(T).GetFields().Select(f => f.GetValue(data)));
        sb.AppendLine(row);

        File.AppendAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log($"[SaveDataManager] 数据已保存 → {path}");
    }

    // ============================================================
    // 提供友好接口（给 RegisterHandler / GameManager 使用）
    // ============================================================

    public void SaveRegister(RegisterData data)
    {
        string folder = GetPlayerFolder(data.PlayerID);
        SaveToCSV(folder, "Register.csv", data);
    }

    public void SaveStage(StageData data)
    {
        string folder = GetPlayerFolder(data.PlayerID);
        SaveToCSV(folder, "StageData.csv", data);
    }
}
