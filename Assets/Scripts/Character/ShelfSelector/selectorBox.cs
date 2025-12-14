using System;
using UnityEngine;

public class selectorBox : MonoBehaviour
{
    private void Start()
    {
        gameObject.SetActive(false);
        SelectorManager.Instance.OnNewGameState();
    }
}
