using UnityEngine;
using UnityEditor;
using ChaoShiPoseRecognition.Config;

namespace ChaoShiPoseRecognition.Editor
{
    /// <summary>
    /// RecognitionConfig 的可视化编辑器
    /// </summary>
    [CustomEditor(typeof(RecognitionConfig))]
    public class RecognitionConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            RecognitionConfig config = (RecognitionConfig)target;
            
            // 绘制默认 Inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("配置信息", EditorStyles.boldLabel);
            
            // 显示计算后的阈值（以实际单位显示）
            EditorGUILayout.HelpBox(
                "所有阈值参数以身体尺度（Scale）为单位。\n" +
                "实际触发距离 = 阈值 × 身体尺度（肩宽）",
                MessageType.Info
            );
            
            // 应用修改
            if (GUI.changed)
            {
                EditorUtility.SetDirty(config);
            }
        }
    }
}


