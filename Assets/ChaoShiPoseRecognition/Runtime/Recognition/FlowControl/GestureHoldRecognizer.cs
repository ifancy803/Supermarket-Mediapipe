using UnityEngine;
using ChaoShiPoseRecognition.Core;
using ChaoShiPoseRecognition.Events;
using ChaoShiPoseRecognition.Recognition;
using ChaoShiPoseRecognition.Recognition.Base;
using ChaoShiPoseRecognition.Config;
using ChaoShiPoseRecognition.Utils;

namespace ChaoShiPoseRecognition.Recognition.FlowControl
{
    /// <summary>
    /// 双手抱拳识别器（刷新手势）
    /// 需要持续保持一定时间才能触发
    /// </summary>
    public class GestureHoldRecognizer : ActionRecognizerBase
    {
        protected override ActionType ActionType => ActionType.GestureHold;
        
        private float holdStartTime;
        private bool isHolding;
        
        public float HoldProgress
        {
            get
            {
                if (!isHolding)
                {
                    return 0f;
                }
                
                float elapsed = Time.time - holdStartTime;
                return Mathf.Clamp01(elapsed / config.fistHoldTime);
            }
        }
        
        public GestureHoldRecognizer(
            PoseDataProcessor dataProcessor,
            CalibrationSystem calibrationSystem,
            RecognitionConfig config,
            ActionEventBus eventBus) 
            : base(dataProcessor, calibrationSystem, config, eventBus)
        {
            isHolding = false;
            holdStartTime = 0f;
        }
        
        protected override bool CheckTriggerCondition()
        {
            var leftWrist = dataProcessor.GetLeftWrist();
            var rightWrist = dataProcessor.GetRightWrist();
            var leftShoulder = dataProcessor.GetLeftShoulder();
            var rightHip = dataProcessor.GetRightHip();
            
            if (!leftWrist.HasValue || !rightWrist.HasValue || 
                !leftShoulder.HasValue || !rightHip.HasValue)
            {
                isHolding = false;
                return false;
            }
            
            float scale = calibrationSystem.Scale;
            
            // 条件1：距离近（双手距离小于阈值）
            float distance = PoseMath.Distance(leftWrist.Value, rightWrist.Value);
            float normalizedDistance = distance / scale;
            bool distanceOk = normalizedDistance < config.fistDistThreshold;
            
            // 条件2：高度适中（手腕 Y 位于肩膀和臀部之间）
            float wristY = (leftWrist.Value.y + rightWrist.Value.y) * 0.5f;
            float shoulderY = leftShoulder.Value.y;
            float hipY = rightHip.Value.y;
            bool heightOk = wristY >= hipY && wristY <= shoulderY;
            
            bool conditionMet = distanceOk && heightOk;
            
            if (conditionMet)
            {
                if (!isHolding)
                {
                    // 开始计时
                    isHolding = true;
                    holdStartTime = Time.time;
                }
                
                // 检查是否达到持续时间
                float elapsed = Time.time - holdStartTime;
                if (elapsed >= config.fistHoldTime)
                {
                    return true;
                }
            }
            else
            {
                // 条件不满足，重置
                isHolding = false;
            }
            
            return false;
        }
        
        protected override void Trigger()
        {
            base.Trigger();
            isHolding = false; // 触发后重置
        }
        
        public override void Reset()
        {
            base.Reset();
            isHolding = false;
            holdStartTime = 0f;
        }
    }
}


