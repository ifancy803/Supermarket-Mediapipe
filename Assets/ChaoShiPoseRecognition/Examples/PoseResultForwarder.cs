using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe;
using ChaoShiPoseRecognition.Core;
using UnityEngine;

namespace ChaoShiPoseRecognition.Examples
{
    /// <summary>
    /// 姿态结果转发器
    /// 用于将 MediaPipe PoseLandmarkerRunner 的结果转发给 MediaPipePoseProvider
    /// 
    /// 使用方法：
    /// 1. 将此脚本添加到包含 PoseLandmarkerRunner 的 GameObject 上
    /// 2. 在 Inspector 中设置 MediaPipePoseProvider 引用（或留空自动查找）
    /// 3. 修改 PoseLandmarkerRunner 的 OnPoseLandmarkDetectionOutput 方法，调用此脚本的 OnPoseResult 方法
    /// </summary>
    public class PoseResultForwarder : MonoBehaviour
    {
        [Header("引用")]
        [Tooltip("MediaPipePoseProvider 组件（如果为空会自动查找）")]
        [SerializeField] private MediaPipePoseProvider poseProvider;
        
        [Tooltip("PoseLandmarkerRunner 组件（如果为空会自动查找）")]
        [SerializeField] private MonoBehaviour poseLandmarkerRunner;
        
        private void Start()
        {
            if (poseProvider == null)
            {
                poseProvider = FindFirstObjectByType<MediaPipePoseProvider>();
                if (poseProvider == null)
                {
                    Debug.LogWarning("PoseResultForwarder: MediaPipePoseProvider not found. " +
                        "Please ensure it exists in the scene.");
                }
            }
            
            if (poseLandmarkerRunner == null)
            {
                // 使用反射查找 PoseLandmarkerRunner（因为它在不同的命名空间）
                var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var mb in allMonoBehaviours)
                {
                    if (mb.GetType().Name == "PoseLandmarkerRunner")
                    {
                        poseLandmarkerRunner = mb;
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// 由 PoseLandmarkerRunner 的回调调用
        /// 需要在 PoseLandmarkerRunner 的 OnPoseLandmarkDetectionOutput 方法中调用此方法
        /// </summary>
        /// <param name="result">姿态检测结果</param>
        /// <param name="image">图像（可选）</param>
        /// <param name="timestamp">时间戳（可选）</param>
        public void OnPoseResult(PoseLandmarkerResult result, Image image, long timestamp)
        {
            if (poseProvider != null)
            {
                poseProvider.SetPoseResult(result);
            }
            else
            {
                Debug.LogWarning("PoseResultForwarder: MediaPipePoseProvider is null. Cannot forward result.");
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

