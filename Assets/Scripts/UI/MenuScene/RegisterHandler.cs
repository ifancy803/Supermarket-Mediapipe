using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;

public class RegisterHandler : MonoBehaviour
{
    [Header("UI InputFields输入框")]
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
    public GameObject dialogPanel;
    public TMP_Text dialogMessageText;
    public Button dialogOkButton;

    [Header("Disclaimer Panel (免责声明)")]
    public GameObject disclaimerPanel;
    public Button agreeButton;
    public Text countdownText;
    public int countdownTime = 10;

    void Start()
    {
        CheckInputValid();

        playerIdInput.onValueChanged.AddListener(delegate { CheckInputValid(); });
        nameInput.onValueChanged.AddListener(delegate { CheckInputValid(); });
        ageInput.onValueChanged.AddListener(delegate { CheckInputValid(); });
        genderInput.onValueChanged.AddListener(delegate { CheckInputValid(); });
        educationInput.onValueChanged.AddListener(delegate { CheckInputValid(); });
        diseaseStateInput.onValueChanged.AddListener(delegate { CheckInputValid(); });

        if (disclaimerPanel != null) disclaimerPanel.SetActive(false);
        if (dialogPanel != null) dialogPanel.SetActive(false);
    }

    // 检查信息是否填写完整，决定是否显示确认按钮
    void CheckInputValid()
    {
        bool allFilled =
            !string.IsNullOrEmpty(playerIdInput.text) &&
            !string.IsNullOrEmpty(nameInput.text) &&
            !string.IsNullOrEmpty(ageInput.text) &&
            !string.IsNullOrEmpty(genderInput.text) &&
            !string.IsNullOrEmpty(educationInput.text) &&
            !string.IsNullOrEmpty(diseaseStateInput.text);

        confirmButton.gameObject.SetActive(allFilled);
    }

    public void OnConfirmButtonClick()
    {
        if (medicalHistoryInput.text.Contains("冠心病"))
        {
            ShowDialog("冠心病患者不得使用");
            return;
        }

        if (diseaseStateInput.text.Contains("急性患病中"))
        {
            ShowDialog("急性患病者不得使用");
            return;
        }

        SaveRegisterData();      // ✔ 保存注册信息
        ShowDisclaimer();        // ✔ 显示免责声明
    }

    // 替代原来 CSV 写法 — 使用 SaveDataManager
    void SaveRegisterData()
    {
        var data = new SaveDataManager.RegisterData()
        {
            PlayerID = playerIdInput.text,
            Name = nameInput.text,
            Age = ageInput.text,
            Gender = genderInput.text,
            Education = educationInput.text,
            MedicalHistory = medicalHistoryInput.text,
            DiseaseState = diseaseStateInput.text,
            RegisterTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        SaveDataManager.Instance.SaveRegister(data);
    }

    void ShowDisclaimer()
    {
        disclaimerPanel.SetActive(true);
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

        agreeButton.interactable = true;
        countdownText.text = "您可以点击继续";

        agreeButton.onClick.RemoveAllListeners();
        agreeButton.onClick.AddListener(() =>
        {
            SceneLoadManager.Instance.LoadGameSceneForUI();
        });
    }

    void ShowDialog(string message)
    {
        dialogPanel.SetActive(true);
        dialogMessageText.text = message;

        dialogOkButton.onClick.RemoveAllListeners();
        dialogOkButton.onClick.AddListener(() =>
        {
            dialogPanel.SetActive(false);
        });
    }
}
