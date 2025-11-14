using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    [Header("游戏数据相关")]
    public int score = 0;
    public int currentRoomIndex = 0;
    public List<Room> rooms = new();
    public GameObject currentShelf = null;

    [Header("计时器相关")] public bool gameStart = false;
    public bool isCalculatingTime = false;
    public float updateDuration = 3f;
    public float gameStartTime = 0f;
    public bool isWin = false;    
    private Coroutine timerCoroutine;
 
    [Header("广播")] public ObjectEventSO updateStuffEvent;

    protected override void Awake()
    {
        base.Awake();
        currentRoomIndex = 1;
        
    }

    private void Update()
    {
        // 只在需要时启动计时器，避免重复启动
        if (!isCalculatingTime && !isWin && timerCoroutine == null && gameStart)
        {
            StartTimer();
        }

        // 游戏时间统计（如果游戏正在进行）
        if (!isWin && isCalculatingTime && gameStart)
        {
            gameStartTime += Time.deltaTime;
        }

        if (score >= rooms[currentRoomIndex-1].winScore)
        {
            CurrentGameWin();
            //TODO:当前关卡游戏胜利
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
        currentShelf = Instantiate(rooms[currentRoomIndex-1].shelfPrefab);
        StartNewStage();
    }
    
    
    /// <summary>
    /// 刷新物品,重新开始计时器
    /// </summary>
    private IEnumerator TimerCoroutine(float duration)
    {
        isCalculatingTime = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        //计时结束，刷新物品
        updateStuffEvent.RaiseEvent(null,this);
        isCalculatingTime = false;
        timerCoroutine = null;  // 重要：重置协程引用
    }

    [ContextMenu("开始计时")]
    public void StartTimer()
    {
        if (timerCoroutine != null)
        {
            return;
        }
        
        if (isWin)
        {
            return;
        }

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

    [ContextMenu("游戏胜利")]
    public void CurrentGameWin()
    {
        if(currentRoomIndex != rooms.Count)
            currentRoomIndex++;
        else
        {
            //TODO:最后一关胜利通知ui等。。
            StopTimer();
            isWin = true;
            return;
        }
        isWin = true;
        StopTimer();
        
        //TODO:将score提交给ui
        StartNewStage();
    }

    [ContextMenu("开始新关卡")]
    public void StartNewStage()
    {
        score = 0;
        updateDuration = rooms[currentRoomIndex-1].updateGap;
        
        gameStart  = true;
        isWin = false;
        gameStartTime = 0f;
        isCalculatingTime = false;
        
        //TODO: 根据关卡index创建架子 立即刷新一次物品
        
        Destroy(currentShelf);
        currentShelf = Instantiate(rooms[currentRoomIndex-1].shelfPrefab);
        updateStuffEvent.RaiseEvent(null,this);
    }

    // 调试信息
    private void OnGUI()
    {
        GUILayout.Label($"计时状态: {isCalculatingTime}");
        GUILayout.Label($"游戏时间: {gameStartTime:F1}");
        GUILayout.Label($"协程状态: {(timerCoroutine != null ? "运行中" : "未运行")}");
    }
    
    [ContextMenu("分数加5")]
    public void TestAddScore()
    {
        score+=5;
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
