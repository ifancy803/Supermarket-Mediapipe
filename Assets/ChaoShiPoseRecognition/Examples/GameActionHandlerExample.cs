using UnityEngine;
using ChaoShiPoseRecognition.Events;
using ChaoShiPoseRecognition.Recognition;
using ChaoShiPoseRecognition.Managers;

namespace ChaoShiPoseRecognition.Examples
{
    /// <summary>
    /// 游戏动作处理器示例
    /// 展示如何在外部项目中实现 IActionListener 接口来处理动作事件
    /// </summary>
    public class GameActionHandlerExample : MonoBehaviour, IActionListener
    {
        [Header("引用")]
        private ActionRecognitionManager recognitionManager;
        
        [Header("调试")]
        [SerializeField] private bool enableDebugLog = true;
        
        private void Start()
        {
            // 查找 ActionRecognitionManager
            recognitionManager = FindFirstObjectByType<ActionRecognitionManager>();
            
            if (recognitionManager == null)
            {
                Debug.LogError("ActionRecognitionManager not found in scene!");
                return;
            }
            
            // 注册为监听器
            recognitionManager.RegisterListener(this);
            
            if (enableDebugLog)
            {
                Debug.Log("GameActionHandler registered to ActionRecognitionManager");
            }
        }
        
        private void OnDestroy()
        {
            // 取消注册
            if (recognitionManager != null)
            {
                recognitionManager.UnregisterListener(this);
            }
        }
        
        /// <summary>
        /// 实现 IActionListener 接口
        /// 当动作被识别时会调用此方法
        /// </summary>
        public void OnActionTriggered(ActionEvent actionEvent)
        {
            if (actionEvent == null)
            {
                return;
            }
            
            if (enableDebugLog)
            {
                Debug.Log($"Action triggered: {actionEvent.ActionType} at {actionEvent.Timestamp}");
            }
            
            // 根据动作类型执行相应的游戏逻辑
            switch (actionEvent.ActionType)
            {
                case ActionType.MoveLeft:
                    HandleMoveLeft();
                    break;
                    
                case ActionType.MoveRight:
                    HandleMoveRight();
                    break;
                    
                case ActionType.MoveForward:
                    HandleMoveForward();
                    break;
                    
                case ActionType.GrabLeft:
                    HandleGrabLeft();
                    break;
                    
                case ActionType.GrabRight:
                    HandleGrabRight();
                    break;
                    
                case ActionType.GrabGeneral:
                    HandleGrabGeneral();
                    break;
                    
                case ActionType.GestureHold:
                    HandleGestureHold();
                    break;
            }
        }
        
        // 以下方法需要根据你的游戏逻辑实现
        
        private void HandleMoveLeft()
        {
            // 实现向左移动的游戏逻辑
            Debug.Log("Player moved left");
        }
        
        private void HandleMoveRight()
        {
            // 实现向右移动的游戏逻辑
            Debug.Log("Player moved right");
        }
        
        private void HandleMoveForward()
        {
            // 实现向前移动的游戏逻辑
            Debug.Log("Player moved forward");
        }
        
        private void HandleGrabLeft()
        {
            // 实现左手抓取的游戏逻辑
            Debug.Log("Player grabbed with left hand");
        }
        
        private void HandleGrabRight()
        {
            // 实现右手抓取的游戏逻辑
            Debug.Log("Player grabbed with right hand");
        }
        
        private void HandleGrabGeneral()
        {
            // 实现通用抓取的游戏逻辑
            Debug.Log("Player grabbed (general)");
        }
        
        private void HandleGestureHold()
        {
            // 实现刷新手势的游戏逻辑
            Debug.Log("Player performed gesture hold (refresh)");
        }
    }
}


