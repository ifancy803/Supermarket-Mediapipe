# ChaoShi Pose Recognition System

基于 MediaPipe Pose Tracking 的动作识别系统，用于《超市购物狂》游戏的体感控制。

## 功能特性

- ✅ 完整的动作识别系统（移动、抓取、手势）
- ✅ 可配置的参数系统（ScriptableObject）
- ✅ 数据平滑和防抖处理
- ✅ 事件系统（支持外部项目集成）
- ✅ 模块化架构，易于扩展

## 快速开始

### 1. 集成到项目

将 `ChaoShiPoseRecognition` 文件夹复制到你的 Unity 项目的 `Assets` 目录下。

### 2. 设置 MediaPipe 数据提供者

系统已提供 `MediaPipePoseProvider` 组件，可直接使用：

1. 在场景中创建 GameObject，添加 `MediaPipePoseProvider` 组件
2. 配置参数（推荐使用世界坐标系）
3. 使用 `PoseResultForwarder` 脚本连接 MediaPipe 的 `PoseLandmarkerRunner`

详细步骤请参考 `INTEGRATION_MEDIAPIPE.md`。

如果你需要自定义实现，可以实现 `IPoseDataProvider` 接口。

### 3. 设置 ActionRecognitionManager

1. 在场景中创建一个 GameObject
2. 添加 `ActionRecognitionManager` 组件
3. 创建 `RecognitionConfig` 资源（右键菜单：Create > ChaoShi > Recognition Config）
4. 将配置资源赋值给 Manager
5. 将你的 MediaPipe 数据源（实现 IPoseDataProvider 的组件）赋值给 Manager

### 4. 订阅事件

有两种方式订阅动作事件：

#### 方式1：实现 IActionListener 接口

```csharp
using ChaoShiPoseRecognition.Events;
using ChaoShiPoseRecognition.Recognition;

public class GameActionHandler : MonoBehaviour, IActionListener
{
    private ActionRecognitionManager recognitionManager;
    
    void Start()
    {
        recognitionManager = FindObjectOfType<ActionRecognitionManager>();
        recognitionManager.RegisterListener(this);
    }
    
    public void OnActionTriggered(ActionEvent actionEvent)
    {
        switch (actionEvent.ActionType)
        {
            case ActionType.MoveLeft:
                // 处理向左移动
                break;
            case ActionType.GrabLeft:
                // 处理左手抓取
                break;
            // ... 其他动作
        }
    }
}
```

#### 方式2：使用委托

```csharp
using ChaoShiPoseRecognition.Events;

public class GameManager : MonoBehaviour
{
    private ActionRecognitionManager recognitionManager;
    
    void Start()
    {
        recognitionManager = FindObjectOfType<ActionRecognitionManager>();
        recognitionManager.OnActionTriggered += HandleAction;
    }
    
    void HandleAction(ActionEvent actionEvent)
    {
        // 处理动作事件
    }
    
    void OnDestroy()
    {
        if (recognitionManager != null)
        {
            recognitionManager.OnActionTriggered -= HandleAction;
        }
    }
}
```

## 动作类型

### 移动类动作
- `MoveLeft`: 向左移动
- `MoveRight`: 向右移动
- `MoveForward`: 向前移动（身体前倾）

### 交互类动作
- `GrabLeft`: 左手抓取
- `GrabRight`: 右手抓取
- `GrabGeneral`: 通用抓取（左右手任一）

### 功能类动作
- `GestureHold`: 双手抱拳（刷新手势）

## 配置参数

在 `RecognitionConfig` 中可以配置以下参数：

- **移动类动作阈值**：左右移动的触发距离
- **交互类动作阈值**：举手高于肩膀的距离
- **功能类动作阈值**：双手抱拳的距离和持续时间
- **数据平滑参数**：平滑系数
- **防抖参数**：进入/退出状态的阈值倍数
- **冷却时间**：动作触发后的冷却时间

## 架构说明

系统采用分层模块化架构：

- **Core/**: 核心数据层（数据管理、处理、校准）
- **Recognition/**: 识别层（各种动作识别器）
- **Events/**: 事件系统（事件总线、监听接口）
- **Config/**: 配置系统（ScriptableObject）
- **Utils/**: 工具类（数学计算、滤波器）
- **Managers/**: 管理器（主控制器、协调器）

## 扩展新动作

要添加新的动作识别，只需：

1. 继承 `ActionRecognizerBase`
2. 实现 `CheckTriggerCondition()` 方法
3. 在 `ActionRecognitionManager` 中注册

示例：

```csharp
public class CustomActionRecognizer : ActionRecognizerBase
{
    protected override ActionType ActionType => ActionType.CustomAction;
    
    protected override bool CheckTriggerCondition()
    {
        // 实现你的识别逻辑
        return false;
    }
}
```

## 注意事项

1. 确保在游戏启动或进入体感关卡前执行校准
2. 所有阈值参数以身体尺度（Scale，即肩宽）为单位
3. 系统使用边缘触发机制，避免重复触发
4. 移动类和交互类动作在同一帧不会同时触发（互斥）


