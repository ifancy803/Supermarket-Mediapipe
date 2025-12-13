using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("游戏数据相关")]
    public int score = 0;
    public int currentRoomIndex = 0;
    public List<Room> rooms = new();
    public GameObject currentShelf = null;

    [Header("计时器相关")]
    public bool gameStart = false;
    public bool isCalculatingTime = false;
    public float updateDuration = 3f;
    public float gameStartTime = 0f;
    public bool isWin = false;
    private Coroutine timerCoroutine;

    [Header("广播")]
    public ObjectEventSO updateStuffEvent;

    protected override void Awake()
    {
        base.Awake();
        currentRoomIndex = 1;
    }

    private void Update()
    {
        if (!isCalculatingTime && !isWin && timerCoroutine == null && gameStart)
            StartTimer();

        if (!isWin && isCalculatingTime && gameStart)
            gameStartTime += Time.deltaTime;

        if (score >= rooms[currentRoomIndex - 1].winScore)
        {
            CurrentGameWin();
        }
    }

    private void OnEnable()
    {
        isCalculatingTime = false;
        gameStartTime = 0f;

        GameStart();
    }

    public void GameStart()
    {
        currentShelf = Instantiate(rooms[currentRoomIndex - 1].shelfPrefab);
        StartNewStage();
    }

    private IEnumerator TimerCoroutine(float duration)
    {
        isCalculatingTime = true;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        updateStuffEvent.RaiseEvent(null, this);
        isCalculatingTime = false;
        timerCoroutine = null;
    }

    [ContextMenu("开始计时")]
    public void StartTimer()
    {
        if (timerCoroutine != null || isWin)
            return;

        timerCoroutine = StartCoroutine(TimerCoroutine(updateDuration));
    }

    private void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        isCalculatingTime = false;
    }

    // 保存关卡数据
    void SaveStage()
    {
        var data = new SaveDataManager.StageData()
        {
            PlayerID = SaveDataManager.Instance.CurrentPlayerID,   // 使用正确的玩家ID
            StageIndex = currentRoomIndex,
            Score = score,
            UsedTime = gameStartTime,
            UpdateGap = updateDuration,
            Win = isWin,
            RecordTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        SaveDataManager.Instance.SaveStage(data);
    }

    [ContextMenu("游戏胜利")]
    public void CurrentGameWin()
    {
        isWin = true;

        SaveStage();

        if (currentRoomIndex != rooms.Count)
            currentRoomIndex++;
        else
        {
            StopTimer();
            return;
        }

        StopTimer();
        StartNewStage();
    }

    [ContextMenu("开始新关卡")]
    public void StartNewStage()
    {
        score = 0;
        updateDuration = rooms[currentRoomIndex - 1].updateGap;

        gameStart = true;
        isWin = false;
        gameStartTime = 0f;
        isCalculatingTime = false;

        if (currentShelf != null)
            Destroy(currentShelf);

        currentShelf = Instantiate(rooms[currentRoomIndex - 1].shelfPrefab,new Vector3(-8,0,11.5f), Quaternion.identity);

        updateStuffEvent.RaiseEvent(null, this);
    }

    private void OnGUI()
    {
        GUILayout.Label($"计时状态: {isCalculatingTime}");
        GUILayout.Label($"游戏时间: {gameStartTime:F1}");
        GUILayout.Label($"协程状态: {(timerCoroutine != null ? "运行中" : "未运行")}");
    }

    [ContextMenu("分数加5")]
    public void TestAddScore()
    {
        score += 5;
    }
}

[System.Serializable]
public class Room
{
    public int id;
    public int winScore;
    public float updateGap;
    public GameObject shelfPrefab;
}
