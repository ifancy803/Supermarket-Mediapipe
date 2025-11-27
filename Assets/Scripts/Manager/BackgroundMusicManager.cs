using UnityEngine;

public class BackgroundMusicManager : Singleton<BackgroundMusicManager>
{
    [Header("背景音乐")]
    public AudioSource audioSource;
    public AudioClip bgmClip;

    private void Start()
    {
        if (audioSource != null && bgmClip != null)
        {
            audioSource.clip = bgmClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}