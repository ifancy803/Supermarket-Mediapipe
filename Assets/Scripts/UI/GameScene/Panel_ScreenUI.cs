using UnityEngine;

public class Panel_ScreenUI : MonoBehaviour
{
    [Header("设置界面")]
    public GameObject panelSetting;

    public void OpenSettingPanel()
    {
        if (panelSetting != null)
        {
            Time.timeScale = 0f;
            panelSetting.SetActive(true);
        }
    }
}