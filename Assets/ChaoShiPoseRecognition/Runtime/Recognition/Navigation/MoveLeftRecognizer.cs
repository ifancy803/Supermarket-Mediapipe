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
    /// 向左移动识别器
    /// </summary>
    public class MoveLeftRecognizer : ActionRecognizerBase
    {
        protected override ActionType ActionType => ActionType.MoveLeft;
        
        private HysteresisFilter hysteresisFilter;
        
        public MoveLeftRecognizer(
            PoseDataProcessor dataProcessor,
            CalibrationSystem calibrationSystem,
            RecognitionConfig config,
            ActionEventBus eventBus) 
            : base(dataProcessor, calibrationSystem, config, eventBus)
        {
            float baseThreshold = config.navXThreshold;
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
            float neutralX = calibrationSystem.NeutralX;
            float scale = calibrationSystem.Scale;
            
            // 计算 X 轴偏移（向左为负）
            float offsetX = neutralX - center.x;
            
            // 转换为相对于 Scale 的值
            float normalizedOffset = offsetX / scale;
            
            // 使用防抖滤波器
            return hysteresisFilter.Update(normalizedOffset);
        }
    }
}


