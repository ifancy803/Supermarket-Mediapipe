using UnityEngine;
using ChaoShiPoseRecognition.Utils;

namespace ChaoShiPoseRecognition.Core
{
    /// <summary>
    /// 姿态数据处理器
    /// 负责数据预处理和平滑
    /// </summary>
    public class PoseDataProcessor
    {
        private PoseDataManager dataManager;
        private SmoothingFilterManager filterManager;
        private const int MaxLandmarkIndex = 32; // MediaPipe Pose 最大关键点索引
        
        public PoseDataProcessor(PoseDataManager dataManager, float smoothingFactor = 0.7f)
        {
            this.dataManager = dataManager;
            this.filterManager = new SmoothingFilterManager(MaxLandmarkIndex + 1, smoothingFactor);
        }
        
        /// <summary>
        /// 获取平滑后的关键点位置
        /// </summary>
        public Vector3? GetSmoothedLandmark(int index)
        {
            Vector3? raw = dataManager.GetLandmark(index);
            if (!raw.HasValue)
            {
                return null;
            }
            
            Vector3 smoothed = filterManager.Filter(index, raw.Value);
            return smoothed;
        }
        
        /// <summary>
        /// 获取左肩（平滑后）
        /// </summary>
        public Vector3? GetLeftShoulder()
        {
            return GetSmoothedLandmark(PoseMath.LandmarkIndices.LeftShoulder);
        }
        
        /// <summary>
        /// 获取右肩（平滑后）
        /// </summary>
        public Vector3? GetRightShoulder()
        {
            return GetSmoothedLandmark(PoseMath.LandmarkIndices.RightShoulder);
        }
        
        /// <summary>
        /// 获取左手腕（平滑后）
        /// </summary>
        public Vector3? GetLeftWrist()
        {
            return GetSmoothedLandmark(PoseMath.LandmarkIndices.LeftWrist);
        }
        
        /// <summary>
        /// 获取右手腕（平滑后）
        /// </summary>
        public Vector3? GetRightWrist()
        {
            return GetSmoothedLandmark(PoseMath.LandmarkIndices.RightWrist);
        }
        
        /// <summary>
        /// 获取左臀（平滑后）
        /// </summary>
        public Vector3? GetLeftHip()
        {
            return GetSmoothedLandmark(PoseMath.LandmarkIndices.LeftHip);
        }
        
        /// <summary>
        /// 获取右臀（平滑后）
        /// </summary>
        public Vector3? GetRightHip()
        {
            return GetSmoothedLandmark(PoseMath.LandmarkIndices.RightHip);
        }
        
        /// <summary>
        /// 重置所有滤波器
        /// </summary>
        public void Reset()
        {
            filterManager.Reset();
        }
    }
}


