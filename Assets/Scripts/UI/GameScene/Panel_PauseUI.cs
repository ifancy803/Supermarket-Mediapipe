using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Panel_PauseUI : MonoBehaviour
{
    public GameObject PanelPauseUI;
    public Button RestartButton;
    public Button ResumeButton;
    public Button QuitButton;

    private void Awake()
    {
        RestartButton.onClick.AddListener(OnRestartClicked);
        ResumeButton.onClick.AddListener(OnResumeClicked);
        QuitButton.onClick.AddListener(OnQuitClicked);
    }

    /// <summary>
    /// Restart：卸载 GameScene1，重新加载 Persistent 场景
    /// </summary>
    private void OnRestartClicked()
    {
        Time.timeScale = 1f;

        if (SceneManager.GetSceneByName("GameScene1").isLoaded)
        {
            SceneManager.UnloadSceneAsync("GameScene1");
        }

        SceneManager.LoadScene("Persistent", LoadSceneMode.Single);
    }

    /// <summary>
    /// Resume：继续游戏，隐藏暂停面板
    /// </summary>
    private void OnResumeClicked()
    {
        Time.timeScale = 1f;
        PanelPauseUI.SetActive(false);
    }

    /// <summary>
    /// Quit：退出游戏
    /// </summary>
    private void OnQuitClicked()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}