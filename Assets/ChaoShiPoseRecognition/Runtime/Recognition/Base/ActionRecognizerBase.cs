using UnityEngine;
using ChaoShiPoseRecognition.Core;
using ChaoShiPoseRecognition.Events;
using ChaoShiPoseRecognition.Recognition;
using ChaoShiPoseRecognition.Config;

namespace ChaoShiPoseRecognition.Recognition.Base
{
    /// <summary>
    /// 动作识别器基类
    /// 提供识别器通用功能：边缘触发、冷却时间、状态管理
    /// </summary>
    public abstract class ActionRecognizerBase
    {
        protected PoseDataProcessor dataProcessor;
        protected CalibrationSystem calibrationSystem;
        protected RecognitionConfig config;
        protected ActionEventBus eventBus;
        
        protected RecognitionState state;
        protected float lastTriggerTime;
        protected bool wasTriggeredLastFrame;
        
        protected abstract ActionType ActionType { get; }
        
        public RecognitionState State => state;
        public bool IsInCooldown => state == RecognitionState.Cooldown;
        
        protected ActionRecognizerBase(
            PoseDataProcessor dataProcessor,
            CalibrationSystem calibrationSystem,
            RecognitionConfig config,
            ActionEventBus eventBus)
        {
            this.dataProcessor = dataProcessor;
            this.calibrationSystem = calibrationSystem;
            this.config = config;
            this.eventBus = eventBus;
            this.state = RecognitionState.Idle;
            this.lastTriggerTime = 0f;
            this.wasTriggeredLastFrame = false;
        }
        
        /// <summary>
        /// 更新识别器（应在每帧调用）
        /// </summary>
        public void Update()
        {
            // 检查校准状态
            if (!calibrationSystem.IsCalibrated)
            {
                state = RecognitionState.Idle;
                return;
            }
            
            // 检查冷却时间
            if (state == RecognitionState.Cooldown)
            {
                if (Time.time - lastTriggerTime >= config.cooldownTime)
                {
                    state = RecognitionState.Idle;
                }
                else
                {
                    return;
                }
            }
            
            // 检查触发条件
            bool currentCondition = CheckTriggerCondition();
            
            // 边缘触发：仅在状态从 false 变为 true 时触发
            if (currentCondition && !wasTriggeredLastFrame)
            {
                Trigger();
            }
            
            wasTriggeredLastFrame = currentCondition;
            
            // 更新状态
            if (currentCondition)
            {
                state = RecognitionState.Detecting;
            }
            else
            {
                state = RecognitionState.Idle;
            }
        }
        
        /// <summary>
        /// 检查触发条件（子类实现）
        /// </summary>
        protected abstract bool CheckTriggerCondition();
        
        /// <summary>
        /// 触发动作事件
        /// </summary>
        protected virtual void Trigger()
        {
            if (state == RecognitionState.Cooldown)
            {
                return; // 冷却中，不触发
            }
            
            var actionEvent = new ActionEvent(ActionType, Time.time, 1.0f);
            eventBus.Publish(actionEvent);
            
            state = RecognitionState.Triggered;
            lastTriggerTime = Time.time;
            
            // 进入冷却
            state = RecognitionState.Cooldown;
        }
        
        /// <summary>
        /// 重置识别器状态
        /// </summary>
        public virtual void Reset()
        {
            state = RecognitionState.Idle;
            lastTriggerTime = 0f;
            wasTriggeredLastFrame = false;
        }
    }
}


