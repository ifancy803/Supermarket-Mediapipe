using ChaoShiPoseRecognition.Recognition;

namespace ChaoShiPoseRecognition.Events
{
    /// <summary>
    /// 动作事件数据结构
    /// </summary>
    public class ActionEvent
    {
        /// <summary>
        /// 动作类型
        /// </summary>
        public ActionType ActionType { get; set; }
        
        /// <summary>
        /// 触发时间戳（Unity Time.time）
        /// </summary>
        public float Timestamp { get; set; }
        
        /// <summary>
        /// 置信度（0-1）
        /// </summary>
        public float Confidence { get; set; }
        
        /// <summary>
        /// 额外数据（可选）
        /// </summary>
        public object Data { get; set; }
        
        public ActionEvent(ActionType actionType, float timestamp, float confidence = 1.0f)
        {
            ActionType = actionType;
            Timestamp = timestamp;
            Confidence = confidence;
        }
    }
}


