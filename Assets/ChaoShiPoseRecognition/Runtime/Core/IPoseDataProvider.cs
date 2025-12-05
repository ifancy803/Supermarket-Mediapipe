using UnityEngine;

namespace ChaoShiPoseRecognition.Core
{
    /// <summary>
    /// 姿态数据提供者接口
    /// </summary>
    public interface IPoseDataProvider
    {
        /// <summary>
        /// 获取关键点位置
        /// </summary>
        /// <param name="index">MediaPipe 关键点索引</param>
        /// <returns>关键点位置，如果无效返回 null</returns>
        Vector3? GetLandmark(int index);
        
        /// <summary>
        /// 检查关键点是否可见/有效
        /// </summary>
        /// <param name="index">MediaPipe 关键点索引</param>
        /// <returns>是否有效</returns>
        bool IsLandmarkValid(int index);
        
        /// <summary>
        /// 是否有有效的姿态数据
        /// </summary>
        bool HasValidPose { get; }
    }
}


