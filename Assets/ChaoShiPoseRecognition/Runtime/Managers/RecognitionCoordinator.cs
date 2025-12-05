using System.Collections.Generic;
using ChaoShiPoseRecognition.Recognition;
using ChaoShiPoseRecognition.Recognition.Base;

namespace ChaoShiPoseRecognition.Managers
{
    /// <summary>
    /// 识别协调器
    /// 处理动作优先级和独占性管理
    /// </summary>
    public class RecognitionCoordinator
    {
        private List<ActionRecognizerBase> navigationRecognizers;
        private List<ActionRecognizerBase> grabbingRecognizers;
        private List<ActionRecognizerBase> flowControlRecognizers;
        
        public RecognitionCoordinator()
        {
            navigationRecognizers = new List<ActionRecognizerBase>();
            grabbingRecognizers = new List<ActionRecognizerBase>();
            flowControlRecognizers = new List<ActionRecognizerBase>();
        }
        
        /// <summary>
        /// 注册识别器
        /// </summary>
        public void RegisterRecognizer(ActionRecognizerBase recognizer, ActionCategory category)
        {
            switch (category)
            {
                case ActionCategory.Navigation:
                    navigationRecognizers.Add(recognizer);
                    break;
                case ActionCategory.Grabbing:
                    grabbingRecognizers.Add(recognizer);
                    break;
                case ActionCategory.FlowControl:
                    flowControlRecognizers.Add(recognizer);
                    break;
            }
        }
        
        /// <summary>
        /// 更新所有识别器（处理优先级和独占性）
        /// </summary>
        public void Update()
        {
            // 先处理功能类动作（最高优先级）
            foreach (var recognizer in flowControlRecognizers)
            {
                recognizer.Update();
            }
            
            // 移动类和交互类动作互斥
            // 先检查移动类动作
            bool navigationTriggered = false;
            foreach (var recognizer in navigationRecognizers)
            {
                recognizer.Update();
                if (recognizer.State == RecognitionState.Triggered)
                {
                    navigationTriggered = true;
                    break;
                }
            }
            
            // 如果移动类动作未触发，才处理交互类动作
            if (!navigationTriggered)
            {
                foreach (var recognizer in grabbingRecognizers)
                {
                    recognizer.Update();
                }
            }
            else
            {
                // 移动类动作触发时，重置交互类动作状态
                foreach (var recognizer in grabbingRecognizers)
                {
                    recognizer.Reset();
                }
            }
        }
        
        /// <summary>
        /// 重置所有识别器
        /// </summary>
        public void ResetAll()
        {
            foreach (var recognizer in navigationRecognizers)
            {
                recognizer.Reset();
            }
            foreach (var recognizer in grabbingRecognizers)
            {
                recognizer.Reset();
            }
            foreach (var recognizer in flowControlRecognizers)
            {
                recognizer.Reset();
            }
        }
    }
    
    /// <summary>
    /// 动作类别
    /// </summary>
    public enum ActionCategory
    {
        Navigation,
        Grabbing,
        FlowControl
    }
}


