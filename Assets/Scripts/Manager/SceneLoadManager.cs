using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SceneLoadManager : Singleton<SceneLoadManager>
{
    private AssetReference currentScene;
    public AssetReference menuScene;
    public AssetReference gameScene;

    protected override void Awake()
    {
        base.Awake();
        //  初始化菜单场景
        _ = InitializeMenu();
    }

    private async Task InitializeMenu()
    {
        try
        {
            await LoadMenu();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"初始化菜单失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    private async Task LoadSceneTask()
    {
        if (currentScene == null)
        {
            Debug.LogError("当前场景引用为 null!");
            return;
        }

        try
        {
            // 异步加载场景
            var asyncOperation = currentScene.LoadSceneAsync(LoadSceneMode.Additive);
            // 等待加载完成
            await asyncOperation.Task;

            if (asyncOperation.Status == AsyncOperationStatus.Succeeded)
            {
                // 加载完激活场景
                SceneManager.SetActiveScene(asyncOperation.Result.Scene);
            }
            else
            {
                Debug.LogError("场景加载失败!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"加载场景时出错: {ex.Message}");
        }
    }
    
    private Task UnloadSceneTask()
    {
        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(activeScene);
        }

        return Task.CompletedTask;
    }
    
    public async Task LoadMenu()
    {
        try
        {
            if (currentScene != null)
            {
                await UnloadSceneTask();
            }
            
            currentScene = menuScene;
            await LoadSceneTask();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"加载菜单失败: {ex.Message}");
        }
    }

    // 重要修改：将 async void 改为 async Task
    public async Task LoadGameScene()
    {
        try
        {
            if (currentScene != null)
            {
                await UnloadSceneTask();
            }
            
            if (gameScene == null)
            {
                Debug.LogError("游戏场景引用为 null! 请在 Inspector 中分配 gameScene。");
                return;
            }
            
            currentScene = gameScene;
            await LoadSceneTask();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"加载游戏场景失败: {ex.Message}");
            // 这里可以添加失败后的回退逻辑
        }
    }

    // 为 UI 按钮提供的兼容方法（保持 async void）
    public async void LoadGameSceneForUI()
    {
        await LoadGameScene();
    }
}