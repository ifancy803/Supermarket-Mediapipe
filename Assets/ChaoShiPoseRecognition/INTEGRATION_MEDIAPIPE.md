# MediaPipe Unity Plugin 集成指南

本文档说明如何将 ChaoShi Pose Recognition System 与 MediaPipe Unity Plugin 集成。

## 方法一：使用 MediaPipePoseProvider（推荐）

### 步骤 1: 设置 MediaPipePoseProvider

1. 在场景中创建一个 GameObject（例如命名为 "PoseProvider"）
2. 添加 `MediaPipePoseProvider` 组件
3. 在 Inspector 中配置：
   - **Use World Coordinates**: 推荐勾选（使用世界坐标系）
   - **Visibility Threshold**: 0.5（可见性阈值）
   - **Pose Index**: 0（使用第一个检测到的姿态）

### 步骤 2: 连接 PoseLandmarkerRunner

有两种方式连接：

#### 方式 A: 手动设置结果（最简单）

创建一个辅助脚本，在 PoseLandmarkerRunner 的回调中调用：

```csharp
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe;
using ChaoShiPoseRecognition.Core;
using UnityEngine;

public class PoseResultForwarder : MonoBehaviour
{
    public MediaPipePoseProvider poseProvider;
    private Mediapipe.Unity.Sample.PoseLandmarkDetection.PoseLandmarkerRunner runner;
    
    void Start()
    {
        runner = FindObjectOfType<Mediapipe.Unity.Sample.PoseLandmarkDetection.PoseLandmarkerRunner>();
        if (poseProvider == null)
        {
            poseProvider = FindObjectOfType<MediaPipePoseProvider>();
        }
    }
    
    // 这个方法需要被 PoseLandmarkerRunner 的回调调用
    // 可以通过修改 PoseLandmarkerRunner 的 OnPoseLandmarkDetectionOutput 方法
    public void OnPoseResult(PoseLandmarkerResult result, Image image, long timestamp)
    {
        if (poseProvider != null)
        {
            poseProvider.SetPoseResult(result);
        }
    }
}
```

#### 方式 B: 修改 PoseLandmarkerRunner（需要访问源码）

在 `PoseLandmarkerRunner.cs` 的 `OnPoseLandmarkDetectionOutput` 方法中添加：

```csharp
private void OnPoseLandmarkDetectionOutput(PoseLandmarkerResult result, Image image, long timestamp)
{
    _poseLandmarkerResultAnnotationController.DrawLater(result);
    DisposeAllMasks(result);
    
    // 添加以下代码
    var poseProvider = FindObjectOfType<MediaPipePoseProvider>();
    if (poseProvider != null)
    {
        poseProvider.SetPoseResult(result);
    }
}
```

### 步骤 3: 设置 ActionRecognitionManager

1. 在场景中创建 GameObject，添加 `ActionRecognitionManager` 组件
2. 在 Inspector 中：
   - 将 `MediaPipePoseProvider` 组件拖到 **Media Pipe Source** 字段
   - 设置 RecognitionConfig
   - 配置校准参数

## 方法二：直接实现 IPoseDataProvider

如果你已经有自己的 MediaPipe 集成代码，可以直接实现 `IPoseDataProvider` 接口：

```csharp
using UnityEngine;
using ChaoShiPoseRecognition.Core;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Tasks.Components.Containers;

public class MyPoseProvider : MonoBehaviour, IPoseDataProvider
{
    private PoseLandmarkerResult currentResult;
    private readonly object resultLock = new object();
    
    public void SetResult(PoseLandmarkerResult result)
    {
        lock (resultLock)
        {
            result.CloneTo(ref currentResult);
        }
    }
    
    public bool HasValidPose
    {
        get
        {
            lock (resultLock)
            {
                return currentResult.poseWorldLandmarks != null && 
                       currentResult.poseWorldLandmarks.Count > 0;
            }
        }
    }
    
    public Vector3? GetLandmark(int index)
    {
        lock (resultLock)
        {
            if (currentResult.poseWorldLandmarks == null || 
                currentResult.poseWorldLandmarks.Count == 0)
            {
                return null;
            }
            
            var landmarks = currentResult.poseWorldLandmarks[0].landmarks;
            if (landmarks == null || index < 0 || index >= landmarks.Count)
            {
                return null;
            }
            
            var landmark = landmarks[index];
            if (!landmark.visibility.HasValue || landmark.visibility.Value < 0.5f)
            {
                return null;
            }
            
            return new Vector3(landmark.x, landmark.y, landmark.z);
        }
    }
    
    public bool IsLandmarkValid(int index)
    {
        var landmark = GetLandmark(index);
        return landmark.HasValue;
    }
}
```

## 坐标系说明

MediaPipe 使用两种坐标系：

1. **归一化坐标系** (NormalizedLandmarks)
   - x, y, z 范围 [0, 1]
   - 相对于图像尺寸
   - 需要转换为世界坐标

2. **世界坐标系** (WorldLandmarks) - **推荐使用**
   - x, y, z 单位为米
   - 直接对应 3D 空间位置
   - MediaPipe 坐标系：x 向右，y 向上，z 向前（相机方向）

## 关键点索引

MediaPipe Pose 使用以下关键点索引（BlazePose 模型）：
- 11: 左肩
- 12: 右肩
- 15: 左手腕
- 16: 右手腕
- 23: 左臀
- 24: 右臀

完整索引表请参考 MediaPipe 文档。

## 注意事项

1. **线程安全**: `SetPoseResult` 和 `GetLandmark` 方法使用锁保证线程安全
2. **可见性检查**: 系统会自动检查关键点的可见性（visibility）
3. **多姿态支持**: 默认使用第一个检测到的姿态（索引 0）
4. **性能**: 建议使用世界坐标系，避免坐标转换开销

## 故障排除

### 问题：HasValidPose 始终返回 false

- 检查 MediaPipe 是否正确初始化
- 确认姿态检测正在运行
- 检查 `SetPoseResult` 是否被正确调用
- 查看 MediaPipe 的日志输出

### 问题：GetLandmark 返回 null

- 检查关键点索引是否有效（0-32）
- 确认可见性阈值设置是否合理
- 检查当前是否有有效的姿态数据

### 问题：坐标不正确

- 确认使用的是世界坐标系还是归一化坐标系
- 检查坐标系转换是否正确
- 验证 MediaPipe 的坐标系定义

