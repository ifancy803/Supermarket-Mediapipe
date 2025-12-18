using UnityEngine;
using UnityEngine.UI; // 如果使用TextMeshPro，请改为 using TMPro;
using System.Collections;
using TMPro;

public class Panel_ScreenUI : MonoBehaviour
{
    [Header("UI 组件引用")]
    public TMP_Text countdownText;    // 倒计时文本
    public TMP_Text scoreText;        // 得分文本
    public TMP_Text targetText;       // 左侧目标名称文本 (如: 水果)
    public TMP_Text tipText;          // 下方长条提示文本
    public GameObject panelSetting; // 设置界面

    [Header("提示配置")]
    [SerializeField] private string defaultTargetName = "水果";
    [SerializeField] private float warningDisplayTime = 2f; // 错误提示显示时长

    private int lastScore = -1;
    private float localTimer;
    private Coroutine tipCoroutine;

    private void Start()
    {
        // 初始化显示
        UpdateTargetAndScore();
        if (GameManager.Instance != null)
        {
            localTimer = GameManager.Instance.updateDuration;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // 1. 处理倒计时逻辑 (同步 GameManager 的刷新周期)
        HandleCountdown();

        // 2. 处理得分和关卡目标显示
        HandleScoreAndTarget();
    }

    /// <summary>
    /// 处理倒计时显示逻辑
    /// </summary>
    private void HandleCountdown()
    {
        if (GameManager.Instance.gameStart && GameManager.Instance.isCalculatingTime && !GameManager.Instance.isStop)
        {
            localTimer -= Time.deltaTime;
            if (localTimer < 0) localTimer = 0;
            countdownText.text = $"倒计时：{Mathf.CeilToInt(localTimer)}秒";
        }
        else
        {
            // 当计时器停止或刷新时，重置本地计时器
            localTimer = GameManager.Instance.updateDuration;
            countdownText.text = $"倒计时：{Mathf.CeilToInt(localTimer)}秒";
        }
    }

    /// <summary>
    /// 处理得分更新及目标文本显示
    /// </summary>
    private void HandleScoreAndTarget()
    {
        int currentScore = GameManager.Instance.score;
        int currentRoom = GameManager.Instance.currentRoomIndex;

        // 更新关卡目标名 (1-10关为水果)
        if (currentRoom >= 1 && currentRoom <= 10)
        {
            targetText.text = defaultTargetName;
        }

        // 更新得分显示
        scoreText.text = $"得分：{currentScore}";

        // 检测得分变化，更新提示语
        if (currentScore != lastScore)
        {
            OnScoreChanged(currentScore);
            lastScore = currentScore;
        }
    }

    /// <summary>
    /// 当分数改变时更新下方提示
    /// </summary>
    private void OnScoreChanged(int newScore)
    {
        if (GameManager.Instance.rooms.Count < GameManager.Instance.currentRoomIndex) return;

        var currentRoomData = GameManager.Instance.rooms[GameManager.Instance.currentRoomIndex - 1];
        int totalNeeded = currentRoomData.winScore;
        int remaining = totalNeeded - newScore;

        if (remaining > 1)
        {
            SetTipText($"已收集 {newScore} 个水果，还需收集 {remaining} 个");
        }
        else if (remaining == 1)
        {
            SetTipText($"已收集 {newScore} 个水果，继续加油！");
        }
        else
        {
            SetTipText("目标达成！准备进入下一关...");
        }
    }

    /// <summary>
    /// 设置下方提示文字 (内部调用)
    /// </summary>
    private void SetTipText(string message)
    {
        // 如果正在显示错误警告，不立即覆盖，除非是得分更新
        if (tipCoroutine == null)
        {
            tipText.text = $"提示：{message}";
        }
    }

    /// <summary>
    /// 公共接口：显示错误提示 (如误触蔬菜时调用)
    /// 调用示例：FindObjectOfType<Panel_ScreenUI>().ShowWarningTip("这是蔬菜哦，找水果试试看～");
    /// </summary>
    public void ShowWarningTip(string warningMsg)
    {
        if (tipCoroutine != null) StopCoroutine(tipCoroutine);
        tipCoroutine = StartCoroutine(WarningDisplayRoutine(warningMsg));
    }

    private IEnumerator WarningDisplayRoutine(string msg)
    {
        tipText.text = $"提示：{msg}";
        yield return new WaitForSeconds(warningDisplayTime);
        tipCoroutine = null;
        // 恢复成当前的进度提示
        OnScoreChanged(GameManager.Instance.score);
    }

    private void UpdateTargetAndScore()
    {
        if (GameManager.Instance == null) return;
        scoreText.text = $"得分：{GameManager.Instance.score}";
        targetText.text = defaultTargetName;
    }

    
    #region 设置界面控制
    public void OpenSettingPanel()
    {
        if (panelSetting != null)
        {
            Time.timeScale = 0f;
            panelSetting.SetActive(true);
        }
    }

    public void CloseSettingPanel()
    {
        if (panelSetting != null)
        {
            Time.timeScale = 1f;
            panelSetting.SetActive(false);
        }
    }
    #endregion
    
}