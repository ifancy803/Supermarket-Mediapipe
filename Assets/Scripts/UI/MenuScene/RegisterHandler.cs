using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;
using System.Text;

public class RegisterHandler : MonoBehaviour
{
    [Header("UI InputFields输入框 (TMP)")]
    public TMP_InputField playerIdInput;
    public TMP_InputField nameInput;
    public TMP_InputField ageInput;
    public TMP_InputField genderInput;
    public TMP_InputField educationInput;
    public TMP_InputField medicalHistoryInput;
    public TMP_InputField diseaseStateInput;

    [Header("Buttons")]
    public Button confirmButton;

    [Header("Dialog Panel (提示面板)")]
    public GameObject dialogPanel; // ✅ 改为在场景中放置的提醒面板
    public TMP_Text dialogMessageText;
    public Button dialogOkButton;

    [Header("Scene Names")]
    public string menuSceneName = "MenuScene";
    public string gameSceneName = "GameScene";

    [Header("Disclaimer Panel (免责声明)")]
    public GameObject disclaimerPanel;
    public Button agreeButton;
    public Text countdownText;
    public int countdownTime = 10;

    private string csvFilePath;

    void Start()
    {
        // 初始化文件路径
        csvFilePath = Path.Combine(Application.persistentDataPath, "RegisterData.csv");

        // 如果文件不存在则创建表头
        if (!File.Exists(csvFilePath))
        {
            string header = "PlayerID,Name,Age,Gender,Education,MedicalHistory,DiseaseState,RegisterTime\n";
            File.WriteAllText(csvFilePath, header, Encoding.UTF8);
            Debug.Log($"✅ 创建新表文件: {csvFilePath}");
        }

        // 默认测试数据
        playerIdInput.text = "001";
        nameInput.text = "张三";
        ageInput.text = "1990-01-01";
        genderInput.text = "男";
        educationInput.text = "本科";
        medicalHistoryInput.text = "";
        diseaseStateInput.text = "康复期";

        CheckInputValid();

        // 监听TMP输入框变化事件
        playerIdInput.onValueChanged.AddListener(delegate { CheckInputValid(); });
        nameInput.onValueChanged.AddListener(delegate { CheckInputValid(); });
        ageInput.onValueChanged.AddListener(delegate { CheckInputValid(); });
        genderInput.onValueChanged.AddListener(delegate { CheckInputValid(); });
        educationInput.onValueChanged.AddListener(delegate { CheckInputValid(); });
        diseaseStateInput.onValueChanged.AddListener(delegate { CheckInputValid(); });

        // 确保所有面板默认隐藏
        if (disclaimerPanel != null) disclaimerPanel.SetActive(false);
        if (dialogPanel != null) dialogPanel.SetActive(false);
    }

    void CheckInputValid()
    {
        bool allFilled = !string.IsNullOrEmpty(playerIdInput.text) &&
                         !string.IsNullOrEmpty(nameInput.text) &&
                         !string.IsNullOrEmpty(ageInput.text) &&
                         !string.IsNullOrEmpty(genderInput.text) &&
                         !string.IsNullOrEmpty(educationInput.text) &&
                         !string.IsNullOrEmpty(diseaseStateInput.text);

        confirmButton.gameObject.SetActive(allFilled);
    }

    public void OnConfirmButtonClick()
    {
        Debug.Log("✅ 点击确认按钮");

        string medicalHistory = medicalHistoryInput.text;
        string diseaseState = diseaseStateInput.text;

        if (medicalHistory.Contains("冠心病"))
        {
            ShowDialog("冠心病患者不得使用");
            return;
        }

        if (diseaseState.Contains("急性患病中"))
        {
            ShowDialog("急性患病者不得使用");
            return;
        }

        // ✅ 保存玩家注册数据
        SaveRegisterDataToCSV();

        // ✅ 弹出免责声明
        ShowDisclaimer();
    }

    /// <summary>
    /// 以CSV格式保存注册信息，每次新注册追加到文件末尾
    /// </summary>
    void SaveRegisterDataToCSV()
    {
        string registerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string newLine = $"{playerIdInput.text},{nameInput.text},{ageInput.text},{genderInput.text}," +
                         $"{educationInput.text},{medicalHistoryInput.text},{diseaseStateInput.text},{registerTime}\n";

        File.AppendAllText(csvFilePath, newLine, Encoding.UTF8);

        Debug.Log($"✅ 玩家注册信息已追加到Excel文件：{csvFilePath}");
    }

    void ShowDisclaimer()
    {
        if (disclaimerPanel == null)
        {
            Debug.LogError("❌ 未绑定 DisclaimerPanel！");
            return;
        }

        disclaimerPanel.SetActive(true); // ✅ 场景中已有，直接显示
        Debug.Log("✅ 显示免责声明面板");

        agreeButton.interactable = false;
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        int timeLeft = countdownTime;

        while (timeLeft > 0)
        {
            countdownText.text = $"请阅读免责声明（{timeLeft}）秒后可同意";
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        countdownText.text = "您可以点击按钮继续";
        agreeButton.interactable = true;

        agreeButton.onClick.RemoveAllListeners();
        agreeButton.onClick.AddListener(() =>
        {
            Debug.Log("✅ 玩家已同意免责声明，准备切换场景");
            SceneLoadManager.Instance.LoadGameSceneForUI();
        });
    }

    void ShowDialog(string message)
    {
        if (dialogPanel == null)
        {
            Debug.LogError("❌ 未绑定 DialogPanel！");
            return;
        }

        dialogPanel.SetActive(true); // ✅ 场景中已有，直接显示
        dialogMessageText.text = message;
        Debug.Log($"✅ 显示提醒Panel: {message}");

        dialogOkButton.onClick.RemoveAllListeners();
        dialogOkButton.onClick.AddListener(() =>
        {
            dialogPanel.SetActive(false);
            Debug.Log("✅ 关闭提醒Panel");
        });
    }
}
