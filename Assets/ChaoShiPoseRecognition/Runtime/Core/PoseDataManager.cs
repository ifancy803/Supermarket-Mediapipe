using UnityEngine;
using ChaoShiPoseRecognition.Utils;

namespace ChaoShiPoseRecognition.Core
{
    /// <summary>
    /// MediaPipe 数据管理器
    /// 封装 MediaPipe 数据获取，提供统一的数据接口
    /// </summary>
    public class PoseDataManager : IPoseDataProvider
    {
        private IPoseDataProvider dataProvider;
        
        public bool HasValidPose => dataProvider != null && dataProvider.HasValidPose;
        
        public PoseDataManager(IPoseDataProvider provider)
        {
            this.dataProvider = provider;
        }
        
        public Vector3? GetLandmark(int index)
        {
            if (dataProvider == null || !dataProvider.IsLandmarkValid(index))
            {
                return null;
            }
            
            return dataProvider.GetLandmark(index);
        }
        
        public bool IsLandmarkValid(int index)
        {
            return dataProvider != null && dataProvider.IsLandmarkValid(index);
        }
        
        /// <summary>
        /// 获取左肩位置
        /// </summary>
        public Vector3? GetLeftShoulder()
        {
            return GetLandmark(PoseMath.LandmarkIndices.LeftShoulder);
        }
        
        /// <summary>
        /// 获取右肩位置
        /// </summary>
        public Vector3? GetRightShoulder()
        {
            return GetLandmark(PoseMath.LandmarkIndices.RightShoulder);
        }
        
        /// <summary>
        /// 获取左手腕位置
        /// </summary>
        public Vector3? GetLeftWrist()
        {
            return GetLandmark(PoseMath.LandmarkIndices.LeftWrist);
        }
        
        /// <summary>
        /// 获取右手腕位置
        /// </summary>
        public Vector3? GetRightWrist()
        {
            return GetLandmark(PoseMath.LandmarkIndices.RightWrist);
        }
        
        /// <summary>
        /// 获取左臀位置
        /// </summary>
        public Vector3? GetLeftHip()
        {
            return GetLandmark(PoseMath.LandmarkIndices.LeftHip);
        }
        
        /// <summary>
        /// 获取右臀位置
        /// </summary>
        public Vector3? GetRightHip()
        {
            return GetLandmark(PoseMath.LandmarkIndices.RightHip);
        }
    }
}


