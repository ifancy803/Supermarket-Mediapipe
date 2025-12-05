using UnityEngine;
using ChaoShiPoseRecognition.Core;
using ChaoShiPoseRecognition.Events;
using ChaoShiPoseRecognition.Recognition;
using ChaoShiPoseRecognition.Recognition.Base;
using ChaoShiPoseRecognition.Config;
using ChaoShiPoseRecognition.Utils;

namespace ChaoShiPoseRecognition.Recognition.Grabbing
{
    /// <summary>
    /// 右手抓取识别器
    /// </summary>
    public class GrabRightRecognizer : ActionRecognizerBase
    {
        protected override ActionType ActionType => ActionType.GrabRight;
        
        private HysteresisFilter hysteresisFilter;
        
        public GrabRightRecognizer(
            PoseDataProcessor dataProcessor,
            CalibrationSystem calibrationSystem,
            RecognitionConfig config,
            ActionEventBus eventBus) 
            : base(dataProcessor, calibrationSystem, config, eventBus)
        {
            float baseThreshold = config.grabYThreshold;
            hysteresisFilter = new HysteresisFilter(
                baseThreshold * config.hysteresisEnterMultiplier,
                baseThreshold * config.hysteresisExitMultiplier
            );
        }
        
        protected override bool CheckTriggerCondition()
        {
            var rightWrist = dataProcessor.GetRightWrist();
            var rightShoulder = dataProcessor.GetRightShoulder();
            
            if (!rightWrist.HasValue || !rightShoulder.HasValue)
            {
                return false;
            }
            
            float scale = calibrationSystem.Scale;
            
            // 计算手腕相对于肩膀的 Y 轴距离（手腕高于肩膀为负值）
            float offsetY = rightShoulder.Value.y - rightWrist.Value.y;
            
            // 转换为相对于 Scale 的值
            float normalizedOffset = offsetY / scale;
            
            // 使用防抖滤波器
            return hysteresisFilter.Update(normalizedOffset);
        }
    }
}


