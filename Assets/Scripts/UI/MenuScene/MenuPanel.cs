using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MenuPanel : MonoBehaviour
{
    public Button openRegisterPanelButton;
    public Button quitButton;
    public GameObject menuPanel;
    public GameObject registerPanel;

    public void OpenRegisterPanel()
    {
        //UIManager.Instance.ScaleUp(registerPanel,Ease.InOutQuart,0.5f);
        if(registerPanel == null) return;
        registerPanel.SetActive(true);
        registerPanel.transform.localScale = Vector3.zero;
        // 停止当前可能在播放的动画
        registerPanel.transform.DOKill();
        
        registerPanel.transform.DOScale(Vector3.one,0.5f).SetEase(Ease.InOutQuart);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
