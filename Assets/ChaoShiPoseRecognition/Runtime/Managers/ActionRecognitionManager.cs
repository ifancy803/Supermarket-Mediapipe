using UnityEngine;
using ChaoShiPoseRecognition.Core;
using ChaoShiPoseRecognition.Events;
using ChaoShiPoseRecognition.Config;
using ChaoShiPoseRecognition.Recognition.Navigation;
using ChaoShiPoseRecognition.Recognition.Grabbing;
using ChaoShiPoseRecognition.Recognition.FlowControl;
using ChaoShiPoseRecognition.Managers;

namespace ChaoShiPoseRecognition.Managers
{
    /// <summary>
    /// 动作识别管理器（主控制器）
    /// 负责系统生命周期管理和模块协调
    /// </summary>
    public class ActionRecognitionManager : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private RecognitionConfig config;
        
        [Header("数据源")]
        [SerializeField] private MonoBehaviour mediaPipeSource; // 需要实现 IPoseDataProvider
        
        private IPoseDataProvider dataProvider;
        private PoseDataManager dataManager;
        private PoseDataProcessor dataProcessor;
        private CalibrationSystem calibrationSystem;
        private ActionEventBus eventBus;
        private RecognitionCoordinator coordinator;
        
        [Header("校准状态")]
        [SerializeField] private bool autoCalibrateOnStart = true;
        [SerializeField] private float calibrationDelay = 2f;
        
        private bool isInitialized = false;
        
        /// <summary>
        /// 事件委托（供外部项目订阅）
        /// </summary>
        public System.Action<ActionEvent> OnActionTriggered => eventBus?.OnActionEvent;
        
        /// <summary>
        /// 是否已校准
        /// </summary>
        public bool IsCalibrated => calibrationSystem != null && calibrationSystem.IsCalibrated;
        
        private void Awake()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            if (isInitialized)
            {
                return;
            }
            
            // 验证配置
            if (config == null)
            {
                Debug.LogError("RecognitionConfig is not assigned!");
                return;
            }
            
            // 获取数据提供者
            if (mediaPipeSource != null)
            {
                dataProvider = mediaPipeSource as IPoseDataProvider;
                if (dataProvider == null)
                {
                    Debug.LogError("MediaPipe source must implement IPoseDataProvider!");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("MediaPipe source is not assigned. Please assign it in Inspector.");
                return;
            }
            
            // 初始化核心组件
            dataManager = new PoseDataManager(dataProvider);
            dataProcessor = new PoseDataProcessor(dataManager, config.smoothingFactor);
            calibrationSystem = new CalibrationSystem(dataProcessor);
            eventBus = new ActionEventBus();
            coordinator = new RecognitionCoordinator();
            
            // 注册所有识别器
            RegisterRecognizers();
            
            isInitialized = true;
        }
        
        private void RegisterRecognizers()
        {
            // 移动类动作
            var moveLeft = new MoveLeftRecognizer(dataProcessor, calibrationSystem, config, eventBus);
            var moveRight = new MoveRightRecognizer(dataProcessor, calibrationSystem, config, eventBus);
            var moveForward = new MoveForwardRecognizer(dataProcessor, calibrationSystem, config, eventBus);
            
            coordinator.RegisterRecognizer(moveLeft, ActionCategory.Navigation);
            coordinator.RegisterRecognizer(moveRight, ActionCategory.Navigation);
            coordinator.RegisterRecognizer(moveForward, ActionCategory.Navigation);
            
            // 交互类动作
            var grabLeft = new GrabLeftRecognizer(dataProcessor, calibrationSystem, config, eventBus);
            var grabRight = new GrabRightRecognizer(dataProcessor, calibrationSystem, config, eventBus);
            var grabGeneral = new GrabGeneralRecognizer(dataProcessor, calibrationSystem, config, eventBus);
            
            coordinator.RegisterRecognizer(grabLeft, ActionCategory.Grabbing);
            coordinator.RegisterRecognizer(grabRight, ActionCategory.Grabbing);
            coordinator.RegisterRecognizer(grabGeneral, ActionCategory.Grabbing);
            
            // 功能类动作
            var gestureHold = new GestureHoldRecognizer(dataProcessor, calibrationSystem, config, eventBus);
            coordinator.RegisterRecognizer(gestureHold, ActionCategory.FlowControl);
        }
        
        private void Start()
        {
            if (autoCalibrateOnStart)
            {
                Invoke(nameof(PerformCalibration), calibrationDelay);
            }
        }
        
        private void Update()
        {
            if (!isInitialized)
            {
                return;
            }
            
            // 处理延迟事件
            eventBus.ProcessDeferredEvents();
            
            // 更新识别器
            coordinator.Update();
        }
        
        /// <summary>
        /// 执行校准
        /// </summary>
        public bool PerformCalibration()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("System not initialized. Cannot calibrate.");
                return false;
            }
            
            bool success = calibrationSystem.Calibrate();
            if (success)
            {
                Debug.Log("Calibration successful!");
            }
            else
            {
                Debug.LogWarning("Calibration failed. Make sure pose data is available.");
            }
            
            return success;
        }
        
        /// <summary>
        /// 注册外部事件监听器
        /// </summary>
        public void RegisterListener(IActionListener listener)
        {
            if (eventBus != null)
            {
                eventBus.RegisterListener(listener);
            }
        }
        
        /// <summary>
        /// 取消注册外部事件监听器
        /// </summary>
        public void UnregisterListener(IActionListener listener)
        {
            if (eventBus != null)
            {
                eventBus.UnregisterListener(listener);
            }
        }
        
        private void OnDestroy()
        {
            if (eventBus != null)
            {
                eventBus.Clear();
            }
        }
    }
}


