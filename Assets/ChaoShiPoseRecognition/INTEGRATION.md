# 集成指南

本文档说明如何将 ChaoShi Pose Recognition System 集成到你的 Unity 项目中。

## 前置要求

1. Unity 2020.3 或更高版本
2. MediaPipe Unity Plugin（已安装并配置）
3. 你的项目需要能够获取 MediaPipe Pose 数据

## 集成步骤

### 步骤 1: 复制模块到项目

将整个 `ChaoShiPoseRecognition` 文件夹复制到你的 Unity 项目的 `Assets` 目录下。

### 步骤 2: 实现 IPoseDataProvider

创建一个类实现 `IPoseDataProvider` 接口，用于从 MediaPipe 获取姿态数据。

参考 `Examples/MediaPipePoseProviderExample.cs` 中的示例代码。

关键点：
- 实现 `GetLandmark(int index)` 方法，返回 MediaPipe 关键点位置
- 实现 `IsLandmarkValid(int index)` 方法，检查关键点是否有效
- 实现 `HasValidPose` 属性，表示是否有有效的姿态数据

### 步骤 3: 创建配置资源

1. 在 Unity 编辑器中，右键点击 Project 窗口
2. 选择 `Create > ChaoShi > Recognition Config`
3. 配置参数（或使用默认值）
4. 保存配置资源

### 步骤 4: 设置场景

1. 在场景中创建一个 GameObject（例如命名为 "PoseRecognitionManager"）
2. 添加 `ActionRecognitionManager` 组件
3. 在 Inspector 中：
   - 将步骤 3 创建的配置资源拖到 `Config` 字段
   - 将步骤 2 创建的 MediaPipe 数据提供者组件拖到 `Media Pipe Source` 字段
   - 配置校准参数（可选）

### 步骤 5: 实现事件处理器

有两种方式处理动作事件：

#### 方式 A: 实现 IActionListener 接口

```csharp
public class MyGameHandler : MonoBehaviour, IActionListener
{
    void Start()
    {
        var manager = FindObjectOfType<ActionRecognitionManager>();
        manager.RegisterListener(this);
    }
    
    public void OnActionTriggered(ActionEvent actionEvent)
    {
        // 处理动作事件
    }
}
```

#### 方式 B: 使用委托

```csharp
public class MyGameHandler : MonoBehaviour
{
    void Start()
    {
        var manager = FindObjectOfType<ActionRecognitionManager>();
        manager.OnActionTriggered += HandleAction;
    }
    
    void HandleAction(ActionEvent actionEvent)
    {
        // 处理动作事件
    }
    
    void OnDestroy()
    {
        var manager = FindObjectOfType<ActionRecognitionManager>();
        if (manager != null)
        {
            manager.OnActionTriggered -= HandleAction;
        }
    }
}
```

### 步骤 6: 执行校准

校准是必需的，有两种方式：

1. **自动校准**（推荐）：
   - 在 `ActionRecognitionManager` 的 Inspector 中勾选 `Auto Calibrate On Start`
   - 设置 `Calibration Delay`（建议 2-3 秒，给用户时间摆好姿势）

2. **手动校准**：
   ```csharp
   var manager = FindObjectOfType<ActionRecognitionManager>();
   manager.PerformCalibration();
   ```

## 使用示例

参考 `Examples/` 目录下的示例代码：
- `GameActionHandlerExample.cs`: 展示如何实现事件处理器
- `MediaPipePoseProviderExample.cs`: 展示如何实现数据提供者

## 注意事项

1. **校准时机**：确保在用户准备好姿势后再执行校准
2. **数据有效性**：系统会自动检查数据有效性，无效数据不会触发动作
3. **动作互斥**：移动类和交互类动作在同一帧不会同时触发
4. **阈值单位**：所有阈值参数以身体尺度（Scale，即肩宽）为单位

## 调试

1. 启用 `ActionRecognitionManager` 的调试日志
2. 检查 `IsCalibrated` 状态
3. 在事件处理器中添加日志输出
4. 使用 Unity Profiler 检查性能

## 扩展

要添加新的动作识别：

1. 在 `ActionType` 枚举中添加新类型
2. 创建新的识别器类继承 `ActionRecognizerBase`
3. 在 `ActionRecognitionManager.RegisterRecognizers()` 中注册

参考现有识别器的实现方式。


