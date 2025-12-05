using UnityEngine;
using ChaoShiPoseRecognition.Core;
using ChaoShiPoseRecognition.Events;
using ChaoShiPoseRecognition.Recognition;
using ChaoShiPoseRecognition.Recognition.Base;
using ChaoShiPoseRecognition.Config;
using ChaoShiPoseRecognition.Utils;

namespace ChaoShiPoseRecognition.Recognition.Navigation
{
    /// <summary>
    /// 向前移动识别器（身体前倾）
    /// </summary>
    public class MoveForwardRecognizer : ActionRecognizerBase
    {
        protected override ActionType ActionType => ActionType.MoveForward;
        
        private HysteresisFilter hysteresisFilter;
        
        public MoveForwardRecognizer(
            PoseDataProcessor dataProcessor,
            CalibrationSystem calibrationSystem,
            RecognitionConfig config,
            ActionEventBus eventBus) 
            : base(dataProcessor, calibrationSystem, config, eventBus)
        {
            float baseThreshold = config.forwardYThreshold;
            hysteresisFilter = new HysteresisFilter(
                baseThreshold * config.hysteresisEnterMultiplier,
                baseThreshold * config.hysteresisExitMultiplier
            );
        }
        
        protected override bool CheckTriggerCondition()
        {
            if (!calibrationSystem.UpdateCenter())
            {
                return false;
            }
            
            Vector3 center = calibrationSystem.Center;
            float neutralShoulderY = calibrationSystem.NeutralShoulderY;
            float scale = calibrationSystem.Scale;
            
            // 计算 Y 轴偏移（Y 坐标增大表示前倾，即低于初始高度）
            // 注意：Unity 坐标系中 Y 向下为正，所以前倾时 center.y 会增大
            float offsetY = center.y - neutralShoulderY;
            
            // 转换为相对于 Scale 的值
            float normalizedOffset = offsetY / scale;
            
            // 使用防抖滤波器
            return hysteresisFilter.Update(normalizedOffset);
        }
    }
}


