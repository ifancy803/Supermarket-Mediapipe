using UnityEngine;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Tasks.Components.Containers;
using System.Collections.Generic;
using System;

namespace ChaoShiPoseRecognition.Core
{
    /// <summary>
    /// MediaPipe 姿态数据提供者
    /// 实现 IPoseDataProvider 接口，从 MediaPipe Unity Plugin 获取姿态数据
    /// </summary>
    public class MediaPipePoseProvider : MonoBehaviour, IPoseDataProvider
    {
        [Header("MediaPipe 引用")]
        [Tooltip("PoseLandmarkerRunner 组件引用（如果为空会自动查找）")]
        [SerializeField] private MonoBehaviour poseLandmarkerRunnerComponent;
        
        [Header("配置")]
        [Tooltip("使用世界坐标系（true）还是归一化坐标系（false）。推荐使用世界坐标系")]
        [SerializeField] private bool useWorldCoordinates = true;
        
        [Tooltip("可见性阈值（0-1），低于此值的关键点视为无效")]
        [Range(0f, 1f)]
        [SerializeField] private float visibilityThreshold = 0.5f;
        
        [Tooltip("使用第几个检测到的姿态（通常为0，表示第一个/主要的姿态）")]
        [SerializeField] private int poseIndex = 0;
        
        [Header("调试")]
        [SerializeField] private bool enableDebugLog = false;
        
        private PoseLandmarkerResult currentResult;
        private readonly object resultLock = new object();
        private bool isInitialized = false;
        
        public bool HasValidPose
        {
            get
            {
                if (!isInitialized)
                {
                    return false;
                }
                
                lock (resultLock)
                {
                    if (currentResult.poseLandmarks == null || currentResult.poseLandmarks.Count == 0)
                    {
                        return false;
                    }
                    
                    if (poseIndex >= currentResult.poseLandmarks.Count)
                    {
                        return false;
                    }
                    
                    if (useWorldCoordinates)
                    {
                        if (currentResult.poseWorldLandmarks != null && poseIndex < currentResult.poseWorldLandmarks.Count)
                        {
                            var landmarks = currentResult.poseWorldLandmarks[poseIndex].landmarks;
                            return landmarks != null && landmarks.Count > 0;
                        }
                        return false;
                    }
                    else
                    {
                        var landmarks = currentResult.poseLandmarks[poseIndex].landmarks;
                        return landmarks != null && landmarks.Count > 0;
                    }
                }
            }
        }
        
        public Vector3? GetLandmark(int index)
        {
            if (!IsLandmarkValid(index))
            {
                return null;
            }
            
            lock (resultLock)
            {
                if (currentResult.poseLandmarks == null || currentResult.poseLandmarks.Count == 0)
                {
                    return null;
                }
                
                if (poseIndex >= currentResult.poseLandmarks.Count)
                {
                    return null;
                }
                
                if (useWorldCoordinates)
                {
                    if (currentResult.poseWorldLandmarks == null || poseIndex >= currentResult.poseWorldLandmarks.Count)
                    {
                        return null;
                    }
                    
                    var worldLandmarks = currentResult.poseWorldLandmarks[poseIndex].landmarks;
                    if (worldLandmarks == null || index < 0 || index >= worldLandmarks.Count)
                    {
                        return null;
                    }
                    
                    var landmark = worldLandmarks[index];
                    // 世界坐标系：x, y, z 单位为米
                    // MediaPipe 坐标系：x 向右，y 向上，z 向前（相机方向）
                    // Unity 坐标系：x 向右，y 向上，z 向前
                    // 直接使用即可
                    return new Vector3(landmark.x, landmark.y, landmark.z);
                }
                else
                {
                    var normalizedLandmarks = currentResult.poseLandmarks[poseIndex].landmarks;
                    if (normalizedLandmarks == null || index < 0 || index >= normalizedLandmarks.Count)
                    {
                        return null;
                    }
                    
                    var landmark = normalizedLandmarks[index];
                    // 归一化坐标系：x, y, z 范围 [0, 1]
                    // 注意：归一化坐标需要根据图像尺寸转换为世界坐标
                    // 这里直接返回归一化坐标，调用方需要根据实际需求转换
                    return new Vector3(landmark.x, landmark.y, landmark.z);
                }
            }
        }
        
        public bool IsLandmarkValid(int index)
        {
            if (!isInitialized)
            {
                return false;
            }
            
            lock (resultLock)
            {
                if (currentResult.poseLandmarks == null || currentResult.poseLandmarks.Count == 0)
                {
                    return false;
                }
                
                if (poseIndex >= currentResult.poseLandmarks.Count)
                {
                    return false;
                }
                
                if (index < 0)
                {
                    return false;
                }
                
                if (useWorldCoordinates)
                {
                    if (currentResult.poseWorldLandmarks == null || poseIndex >= currentResult.poseWorldLandmarks.Count)
                    {
                        return false;
                    }
                    
                    var worldLandmarks = currentResult.poseWorldLandmarks[poseIndex].landmarks;
                    if (worldLandmarks == null || index >= worldLandmarks.Count)
                    {
                        return false;
                    }
                    
                    var landmark = worldLandmarks[index];
                    return landmark.visibility.HasValue && landmark.visibility.Value >= visibilityThreshold;
                }
                else
                {
                    var normalizedLandmarks = currentResult.poseLandmarks[poseIndex].landmarks;
                    if (normalizedLandmarks == null || index >= normalizedLandmarks.Count)
                    {
                        return false;
                    }
                    
                    var landmark = normalizedLandmarks[index];
                    return landmark.visibility.HasValue && landmark.visibility.Value >= visibilityThreshold;
                }
            }
        }
        
        /// <summary>
        /// 设置当前的姿态检测结果
        /// 由扩展的 PoseLandmarkerRunner 或自定义组件调用
        /// </summary>
        public void SetPoseResult(PoseLandmarkerResult result)
        {
            lock (resultLock)
            {
                result.CloneTo(ref currentResult);
            }
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            if (isInitialized)
            {
                return;
            }
            
            // 尝试查找 PoseLandmarkerRunner
            if (poseLandmarkerRunnerComponent == null)
            {
                // 使用反射查找，因为类型在不同的命名空间中
                var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var mb in allMonoBehaviours)
                {
                    var type = mb.GetType();
                    if (type.Name == "PoseLandmarkerRunner")
                    {
                        poseLandmarkerRunnerComponent = mb;
                        break;
                    }
                }
            }
            
            if (poseLandmarkerRunnerComponent != null)
            {
                // 尝试通过反射订阅结果
                TrySubscribeToPoseLandmarker();
            }
            else
            {
                Debug.LogWarning("MediaPipePoseProvider: PoseLandmarkerRunner not found. " +
                    "Please assign it manually in Inspector, or ensure SetPoseResult() is called externally.");
            }
            
            isInitialized = true;
        }
        
        private void TrySubscribeToPoseLandmarker()
        {
            // 由于 PoseLandmarkerRunner 在不同的命名空间中，我们使用反射
            // 或者创建一个扩展类来桥接
            // 这里提供一个基础实现，实际使用时可能需要调整
            
            if (enableDebugLog)
            {
                Debug.Log("MediaPipePoseProvider: Attempting to subscribe to PoseLandmarkerRunner");
            }
            
            // 注意：实际的订阅需要通过扩展 PoseLandmarkerRunner 或使用事件系统
            // 这里提供一个占位实现
        }
        
        /// <summary>
        /// 手动设置 PoseLandmarkerRunner 引用
        /// </summary>
        public void SetPoseLandmarkerRunner(MonoBehaviour runner)
        {
            poseLandmarkerRunnerComponent = runner;
            TrySubscribeToPoseLandmarker();
        }
    }
}

