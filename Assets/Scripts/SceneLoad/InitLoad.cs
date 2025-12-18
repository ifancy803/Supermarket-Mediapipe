using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class InitLoad : MonoBehaviour
{
    public AssetReference persistent;

    private void Awake()
    {
        Addressables.LoadSceneAsync(persistent);
    }
}
