namespace ChaoShiPoseRecognition.Utils
{
    /// <summary>
    /// 防抖阈值滤波器（滞后滤波器）
    /// 用于避免在临界值附近频繁切换状态
    /// </summary>
    public class HysteresisFilter
    {
        private bool currentState;
        private float enterThreshold;
        private float exitThreshold;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="enterThreshold">进入状态的阈值（较高）</param>
        /// <param name="exitThreshold">退出状态的阈值（较低）</param>
        public HysteresisFilter(float enterThreshold, float exitThreshold)
        {
            this.enterThreshold = enterThreshold;
            this.exitThreshold = exitThreshold;
            this.currentState = false;
        }
        
        /// <summary>
        /// 更新滤波器状态
        /// </summary>
        /// <param name="value">当前值</param>
        /// <returns>是否满足触发条件</returns>
        public bool Update(float value)
        {
            if (currentState)
            {
                // 当前处于触发状态，需要低于退出阈值才退出
                if (value < exitThreshold)
                {
                    currentState = false;
                }
            }
            else
            {
                // 当前未触发，需要高于进入阈值才触发
                if (value > enterThreshold)
                {
                    currentState = true;
                }
            }
            
            return currentState;
        }
        
        /// <summary>
        /// 重置滤波器状态
        /// </summary>
        public void Reset()
        {
            currentState = false;
        }
        
        /// <summary>
        /// 获取当前状态
        /// </summary>
        public bool CurrentState => currentState;
    }
}


