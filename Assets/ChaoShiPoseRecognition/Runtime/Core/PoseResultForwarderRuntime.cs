using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe;
using UnityEngine;

namespace ChaoShiPoseRecognition.Core
{
    /// <summary>
    /// 将 MediaPipe PoseLandmarkerRunner 的结果转发给 MediaPipePoseProvider
    /// 放在 Runtime 程序集中，供其它程序集（如 Mediapipe.Unity.Sample）引用，避免循环依赖
    /// </summary>
    public class PoseResultForwarderRuntime : MonoBehaviour
    {
        [Header("引用")]
        [Tooltip("MediaPipePoseProvider 组件（如果为空会自动查找）")]
        [SerializeField] private MediaPipePoseProvider poseProvider;

        private void Awake()
        {
            if (poseProvider == null)
            {
                poseProvider = FindFirstObjectByType<MediaPipePoseProvider>();
                if (poseProvider == null)
                {
                    Debug.LogWarning("PoseResultForwarderRuntime: MediaPipePoseProvider not found in scene.");
                }
            }
        }

        /// <summary>
        /// 由 PoseLandmarkerRunner 的回调调用
        /// </summary>
        public void OnPoseResult(PoseLandmarkerResult result, Image image, long timestamp)
        {
            if (poseProvider != null)
            {
                poseProvider.SetPoseResult(result);
            }
        }

        /// <summary>
        /// 简化版本，只需要结果
        /// </summary>
        public void OnPoseResult(PoseLandmarkerResult result)
        {
            OnPoseResult(result, default, 0);
        }
    }
}

