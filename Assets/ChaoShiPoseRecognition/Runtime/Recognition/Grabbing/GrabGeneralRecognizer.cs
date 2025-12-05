using ChaoShiPoseRecognition.Core;
using ChaoShiPoseRecognition.Events;
using ChaoShiPoseRecognition.Recognition;
using ChaoShiPoseRecognition.Recognition.Base;
using ChaoShiPoseRecognition.Config;

namespace ChaoShiPoseRecognition.Recognition.Grabbing
{
    /// <summary>
    /// 通用抓取识别器（左右手通用）
    /// </summary>
    public class GrabGeneralRecognizer : ActionRecognizerBase
    {
        protected override ActionType ActionType => ActionType.GrabGeneral;
        
        private GrabLeftRecognizer leftRecognizer;
        private GrabRightRecognizer rightRecognizer;
        
        public GrabGeneralRecognizer(
            PoseDataProcessor dataProcessor,
            CalibrationSystem calibrationSystem,
            RecognitionConfig config,
            ActionEventBus eventBus) 
            : base(dataProcessor, calibrationSystem, config, eventBus)
        {
            // 复用左右手识别器的逻辑
            leftRecognizer = new GrabLeftRecognizer(dataProcessor, calibrationSystem, config, null);
            rightRecognizer = new GrabRightRecognizer(dataProcessor, calibrationSystem, config, null);
        }
        
        protected override bool CheckTriggerCondition()
        {
            // 左右手任一满足条件即可
            // 注意：这里需要手动检查，因为 leftRecognizer 和 rightRecognizer 的 eventBus 为 null
            var leftWrist = dataProcessor.GetLeftWrist();
            var rightWrist = dataProcessor.GetRightWrist();
            var leftShoulder = dataProcessor.GetLeftShoulder();
            var rightShoulder = dataProcessor.GetRightShoulder();
            
            if (!leftWrist.HasValue || !leftShoulder.HasValue)
            {
                // 检查右手
                if (!rightWrist.HasValue || !rightShoulder.HasValue)
                {
                    return false;
                }
                
                float scale = calibrationSystem.Scale;
                float offsetY = rightShoulder.Value.y - rightWrist.Value.y;
                float normalizedOffset = offsetY / scale;
                return normalizedOffset > config.grabYThreshold;
            }
            
            if (!rightWrist.HasValue || !rightShoulder.HasValue)
            {
                // 检查左手
                float scale = calibrationSystem.Scale;
                float offsetY = leftShoulder.Value.y - leftWrist.Value.y;
                float normalizedOffset = offsetY / scale;
                return normalizedOffset > config.grabYThreshold;
            }
            
            // 检查左右手任一
            float scaleLeft = calibrationSystem.Scale;
            float offsetYLeft = leftShoulder.Value.y - leftWrist.Value.y;
            float normalizedOffsetLeft = offsetYLeft / scaleLeft;
            
            float scaleRight = calibrationSystem.Scale;
            float offsetYRight = rightShoulder.Value.y - rightWrist.Value.y;
            float normalizedOffsetRight = offsetYRight / scaleRight;
            
            return normalizedOffsetLeft > config.grabYThreshold || 
                   normalizedOffsetRight > config.grabYThreshold;
        }
    }
}

