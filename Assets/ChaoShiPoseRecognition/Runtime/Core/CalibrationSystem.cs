using UnityEngine;
using ChaoShiPoseRecognition.Utils;

namespace ChaoShiPoseRecognition.Core
{
    /// <summary>
    /// 校准系统
    /// 管理初始姿态记录和身体尺度计算
    /// </summary>
    public class CalibrationSystem
    {
        private PoseDataProcessor dataProcessor;
        
        // 校准数据
        private float scale;
        private Vector3 center;
        private float neutralX;
        private float neutralShoulderY;
        
        private bool isCalibrated;
        
        public bool IsCalibrated => isCalibrated;
        public float Scale => scale;
        public Vector3 Center => center;
        public float NeutralX => neutralX;
        public float NeutralShoulderY => neutralShoulderY;
        
        public CalibrationSystem(PoseDataProcessor dataProcessor)
        {
            this.dataProcessor = dataProcessor;
            this.isCalibrated = false;
        }
        
        /// <summary>
        /// 执行校准
        /// </summary>
        public bool Calibrate()
        {
            var leftShoulder = dataProcessor.GetLeftShoulder();
            var rightShoulder = dataProcessor.GetRightShoulder();
            var leftHip = dataProcessor.GetLeftHip();
            var rightHip = dataProcessor.GetRightHip();
            
            if (!leftShoulder.HasValue || !rightShoulder.HasValue || 
                !leftHip.HasValue || !rightHip.HasValue)
            {
                return false;
            }
            
            // 计算身体尺度（肩宽）
            scale = PoseMath.CalculateScale(leftShoulder.Value, rightShoulder.Value);
            
            // 计算身体中心（臀部中点）
            center = PoseMath.CalculateCenter(leftHip.Value, rightHip.Value);
            
            // 记录初始 X 轴中心
            neutralX = center.x;
            
            // 记录初始 Y 轴肩高
            neutralShoulderY = PoseMath.CalculateShoulderY(leftShoulder.Value, rightShoulder.Value);
            
            isCalibrated = true;
            return true;
        }
        
        /// <summary>
        /// 更新当前身体中心（用于实时计算）
        /// </summary>
        public bool UpdateCenter()
        {
            var leftHip = dataProcessor.GetLeftHip();
            var rightHip = dataProcessor.GetRightHip();
            
            if (!leftHip.HasValue || !rightHip.HasValue)
            {
                return false;
            }
            
            center = PoseMath.CalculateCenter(leftHip.Value, rightHip.Value);
            return true;
        }
        
        /// <summary>
        /// 重置校准状态
        /// </summary>
        public void Reset()
        {
            isCalibrated = false;
            scale = 0f;
            center = Vector3.zero;
            neutralX = 0f;
            neutralShoulderY = 0f;
        }
    }
}


