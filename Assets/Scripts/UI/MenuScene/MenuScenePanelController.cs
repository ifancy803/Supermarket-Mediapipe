using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MenuScenePanelController : Singleton<MenuScenePanelController>
{
    public GameObject menuPanel;
    public GameObject registerPanel;

    public void OpenRegisterPanel()
    {
        UIManager.Instance.ScaleUp(registerPanel,Ease.InOutQuart,0.5f);
    }
}
