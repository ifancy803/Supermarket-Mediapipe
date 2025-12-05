using UnityEngine;

namespace ChaoShiPoseRecognition.Utils
{
    /// <summary>
    /// 姿态计算工具类
    /// </summary>
    public static class PoseMath
    {
        /// <summary>
        /// MediaPipe 关键点索引常量
        /// </summary>
        public static class LandmarkIndices
        {
            public const int LeftShoulder = 11;
            public const int RightShoulder = 12;
            public const int LeftWrist = 15;
            public const int RightWrist = 16;
            public const int LeftHip = 23;
            public const int RightHip = 24;
        }
        
        /// <summary>
        /// 计算两点之间的距离
        /// </summary>
        public static float Distance(Vector3 p1, Vector3 p2)
        {
            return Vector3.Distance(p1, p2);
        }
        
        /// <summary>
        /// 计算两点之间的水平距离（仅 X 轴）
        /// </summary>
        public static float HorizontalDistance(Vector3 p1, Vector3 p2)
        {
            return Mathf.Abs(p1.x - p2.x);
        }
        
        /// <summary>
        /// 计算两点之间的垂直距离（仅 Y 轴）
        /// </summary>
        public static float VerticalDistance(Vector3 p1, Vector3 p2)
        {
            return Mathf.Abs(p1.y - p2.y);
        }
        
        /// <summary>
        /// 计算两个点的中点
        /// </summary>
        public static Vector3 Midpoint(Vector3 p1, Vector3 p2)
        {
            return (p1 + p2) * 0.5f;
        }
        
        /// <summary>
        /// 计算身体尺度（肩宽）
        /// </summary>
        public static float CalculateScale(Vector3 leftShoulder, Vector3 rightShoulder)
        {
            return Distance(leftShoulder, rightShoulder);
        }
        
        /// <summary>
        /// 计算身体中心（臀部中点）
        /// </summary>
        public static Vector3 CalculateCenter(Vector3 leftHip, Vector3 rightHip)
        {
            return Midpoint(leftHip, rightHip);
        }
        
        /// <summary>
        /// 计算肩膀平均高度
        /// </summary>
        public static float CalculateShoulderY(Vector3 leftShoulder, Vector3 rightShoulder)
        {
            return Midpoint(leftShoulder, rightShoulder).y;
        }
    }
}


