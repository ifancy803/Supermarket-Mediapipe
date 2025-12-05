using UnityEngine;

namespace ChaoShiPoseRecognition.Config
{
    /// <summary>
    /// 识别参数配置（ScriptableObject）
    /// 可在 Unity Inspector 中可视化配置
    /// </summary>
    [CreateAssetMenu(fileName = "RecognitionConfig", menuName = "ChaoShi/Recognition Config")]
    public class RecognitionConfig : ScriptableObject
    {
        [Header("移动类动作阈值（以 Scale 为单位）")]
        [Tooltip("左右移动的 X 轴触发距离")]
        [Range(0.1f, 1.0f)]
        public float navXThreshold = 0.3f;
        
        [Header("交互类动作阈值（以 Scale 为单位）")]
        [Tooltip("举手高于肩膀的 Y 轴距离")]
        [Range(0.1f, 0.5f)]
        public float grabYThreshold = 0.2f;
        
        [Header("功能类动作阈值（以 Scale 为单位）")]
        [Tooltip("双手抱拳的最大距离")]
        [Range(0.1f, 0.5f)]
        public float fistDistThreshold = 0.25f;
        
        [Tooltip("双手抱拳持续触发时间（秒）")]
        [Range(0.5f, 5.0f)]
        public float fistHoldTime = 1.5f;
        
        [Header("数据平滑参数")]
        [Tooltip("平滑系数（0-1，越大越平滑）")]
        [Range(0.0f, 1.0f)]
        public float smoothingFactor = 0.7f;
        
        [Header("防抖参数")]
        [Tooltip("进入状态的阈值倍数（相对于基础阈值）")]
        [Range(1.0f, 2.0f)]
        public float hysteresisEnterMultiplier = 1.1f;
        
        [Tooltip("退出状态的阈值倍数（相对于基础阈值）")]
        [Range(0.5f, 1.0f)]
        public float hysteresisExitMultiplier = 0.9f;
        
        [Header("冷却时间（秒）")]
        [Tooltip("动作触发后的冷却时间")]
        [Range(0.0f, 5.0f)]
        public float cooldownTime = 0.5f;
        
        [Header("前倾动作阈值（以 Scale 为单位）")]
        [Tooltip("身体前倾的 Y 轴距离")]
        [Range(0.1f, 0.5f)]
        public float forwardYThreshold = 0.15f;
    }
}


