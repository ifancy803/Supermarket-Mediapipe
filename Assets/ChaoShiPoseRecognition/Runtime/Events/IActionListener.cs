namespace ChaoShiPoseRecognition.Events
{
    /// <summary>
    /// 动作事件监听接口（供外部项目实现）
    /// </summary>
    public interface IActionListener
    {
        /// <summary>
        /// 处理动作事件
        /// </summary>
        /// <param name="actionEvent">动作事件</param>
        void OnActionTriggered(ActionEvent actionEvent);
    }
}


