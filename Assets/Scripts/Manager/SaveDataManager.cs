using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance;

    // 当前玩家ID（仅保存已注册的玩家ID）
    public string CurrentPlayerID { get; private set; }

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

    private void SaveToCSV<T>(string folder, string fileName, T data)
    {
        string path = Path.Combine(folder, fileName);
        StringBuilder sb = new StringBuilder();

        if (!File.Exists(path))
        {
            string header = string.Join(",", typeof(T).GetFields().Select(f => f.Name));
            sb.AppendLine(header);
        }

        string row = string.Join(",", typeof(T).GetFields().Select(f => f.GetValue(data)));
        sb.AppendLine(row);

        File.AppendAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log($"[SaveDataManager] 数据已保存 → {path}");
    }

    public void SaveRegister(RegisterData data)
    {
        CurrentPlayerID = data.PlayerID;   // 记录当前玩家
        string folder = GetPlayerFolder(data.PlayerID);
        SaveToCSV(folder, "Register.csv", data);
    }

    public void SaveStage(StageData data)
    {
        string folder = GetPlayerFolder(data.PlayerID);
        SaveToCSV(folder, "StageData.csv", data);
    }
}
