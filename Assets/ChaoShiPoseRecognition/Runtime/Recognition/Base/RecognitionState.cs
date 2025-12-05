namespace ChaoShiPoseRecognition.Recognition
{
    /// <summary>
    /// 识别状态枚举
    /// </summary>
    public enum RecognitionState
    {
        /// <summary>
        /// 未触发
        /// </summary>
        Idle,
        
        /// <summary>
        /// 检测中（条件满足但未达到触发阈值）
        /// </summary>
        Detecting,
        
        /// <summary>
        /// 已触发（边缘触发状态）
        /// </summary>
        Triggered,
        
        /// <summary>
        /// 冷却中
        /// </summary>
        Cooldown
    }
}


