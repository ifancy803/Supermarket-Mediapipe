using UnityEngine;

using Unity.Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    public CinemachineVirtualCamera gameplayVCam;
    public CinemachineVirtualCamera cutsceneVCam;
    public Camera mainCamera;

    [ContextMenu("切换到过场")]
    public void SwitchToCutscene()
    {
        cutsceneVCam.Priority = 15;
        gameplayVCam.Priority = 10;
    }

    [ContextMenu("切换回游戏")]
    public void SwitchToGameplay()
    {
        gameplayVCam.Priority = 15;
        cutsceneVCam.Priority = 10;
    }
}
