// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using System.Collections.Generic;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mediapipe.Unity.Sample.PoseLandmarkDetection
{
  public class PoseLandmarkerRunner : VisionTaskApiRunner<PoseLandmarker>
  {
    [SerializeField] private PoseLandmarkerResultAnnotationController _poseLandmarkerResultAnnotationController;

    private Experimental.TextureFramePool _textureFramePool;
    public readonly PoseLandmarkDetectionConfig config = new PoseLandmarkDetectionConfig();

    // ===== 姿态平滑 =====
    private Queue<List<Vector3>> _smoothedLandmarksQueue;
    private int _smoothingWindowSize = 5;
    private List<Vector3> _lastSmoothedPose;

    // ===== 当前识别状态 =====
    private string _currentWalkingDirection = "静止";
    private bool _handsClaspedState = false;
    private string _oneHandRaisedState = "无";

    // ===== Debug 状态缓存 =====
    private string _lastLoggedWalkingDirection = "";
    private bool _lastLoggedHandsClasped = false;
    private string _lastLoggedOneHandRaised = "";

    // ===== 阈值（老年人适配）=====
    private const float HANDS_CLASPED_DISTANCE_THRESHOLD = 0.1f;
    private const float HAND_RAISED_Y_THRESHOLD = 0.1f;

    // 行走降敏核心参数
    private Vector3 _accumulatedMovement = Vector3.zero;
    private const float MOVEMENT_ACCUM_THRESHOLD = 0.06f; // 老年人推荐

    public override void Stop()
    {
      base.Stop();
      _textureFramePool?.Dispose();
      _textureFramePool = null;
      _smoothedLandmarksQueue?.Clear();
    }

    protected override IEnumerator Run()
    {
      _smoothedLandmarksQueue = new Queue<List<Vector3>>();
      _lastSmoothedPose = null;

      yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

      var options = config.GetPoseLandmarkerOptions(
        config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM
          ? OnPoseLandmarkDetectionOutput
          : null);

      taskApi = PoseLandmarker.CreateFromOptions(options, GpuManager.GpuResources);
      var imageSource = ImageSourceProvider.ImageSource;

      yield return imageSource.Play();
      if (!imageSource.isPrepared) yield break;

      _textureFramePool = new Experimental.TextureFramePool(
        imageSource.textureWidth,
        imageSource.textureHeight,
        TextureFormat.RGBA32,
        10);

      screen.Initialize(imageSource);
      SetupAnnotationController(_poseLandmarkerResultAnnotationController, imageSource);
      _poseLandmarkerResultAnnotationController.InitScreen(
        imageSource.textureWidth,
        imageSource.textureHeight);

      var transformOpt = imageSource.GetTransformationOptions();
      var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: 0);

      AsyncGPUReadbackRequest req = default;
      var waitUntilReqDone = new WaitUntil(() => req.done);

      while (true)
      {
        if (isPaused) yield return new WaitWhile(() => isPaused);

        if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
        {
          yield return new WaitForEndOfFrame();
          continue;
        }

        req = textureFrame.ReadTextureAsync(
          imageSource.GetCurrentTexture(),
          transformOpt.flipHorizontally,
          transformOpt.flipVertically);

        yield return waitUntilReqDone;
        if (req.hasError) continue;

        Image image = textureFrame.BuildCPUImage();
        textureFrame.Release();

        taskApi.DetectAsync(
          image,
          GetCurrentTimestampMillisec(),
          imageProcessingOptions);
      }
    }

    private void OnPoseLandmarkDetectionOutput(
      PoseLandmarkerResult result,
      Image image,
      long timestamp)
    {
      _poseLandmarkerResultAnnotationController.DrawLater(result);
      DisposeAllMasks(result);

      if (result.poseLandmarks == null || result.poseLandmarks.Count == 0) return;

      List<Vector3> currentLandmarks = new List<Vector3>();
      foreach (var lm in result.poseLandmarks[0].landmarks)
        currentLandmarks.Add(new Vector3(lm.x, lm.y, lm.z));

      _smoothedLandmarksQueue.Enqueue(currentLandmarks);
      while (_smoothedLandmarksQueue.Count > _smoothingWindowSize)
        _smoothedLandmarksQueue.Dequeue();

      List<Vector3> smoothed = CalculateSmoothedLandmarks();

      CheckWalkingDirection(smoothed);
      CheckHandsClasped(smoothed);
      CheckOneHandRaised(smoothed);

      LogStateIfChanged();
      _lastSmoothedPose = new List<Vector3>(smoothed);
    }

    // ================= 工具方法 =================

    private Vector3 GetLandmarkPosition(List<Vector3> landmarks, int index)
    {
      if (landmarks != null && index >= 0 && index < landmarks.Count)
        return landmarks[index];
      return Vector3.zero;
    }

    private List<Vector3> CalculateSmoothedLandmarks()
    {
      List<Vector3> smoothed = new List<Vector3>(
        new Vector3[_smoothedLandmarksQueue.Peek().Count]);

      foreach (var frame in _smoothedLandmarksQueue)
        for (int i = 0; i < frame.Count; i++)
          smoothed[i] += frame[i];

      for (int i = 0; i < smoothed.Count; i++)
        smoothed[i] /= _smoothedLandmarksQueue.Count;

      return smoothed;
    }

    // ===== 老年人适配：累计位移 + 静止死区 =====
    private void CheckWalkingDirection(List<Vector3> cur)
    {
      if (_lastSmoothedPose == null)
      {
        _currentWalkingDirection = "静止";
        _accumulatedMovement = Vector3.zero;
        return;
      }

      Vector3 currentNose = GetLandmarkPosition(cur, 0);
      Vector3 lastNose = GetLandmarkPosition(_lastSmoothedPose, 0);
      Vector3 delta = currentNose - lastNose;

      _accumulatedMovement += delta;

      float absX = Mathf.Abs(_accumulatedMovement.x);
      float absZ = Mathf.Abs(_accumulatedMovement.z);

      if (absX < MOVEMENT_ACCUM_THRESHOLD && absZ < MOVEMENT_ACCUM_THRESHOLD)
      {
        _currentWalkingDirection = "静止";
        return;
      }

      _currentWalkingDirection =
        absX > absZ
          ? (_accumulatedMovement.x > 0 ? "向右" : "向左")
          : (_accumulatedMovement.z > 0 ? "向前" : "向后");

      _accumulatedMovement = Vector3.zero;
    }

    private void CheckHandsClasped(List<Vector3> cur)
    {
      Vector3 lw = GetLandmarkPosition(cur, 15);
      Vector3 rw = GetLandmarkPosition(cur, 16);
      _handsClaspedState =
        Vector3.Distance(lw, rw) < HANDS_CLASPED_DISTANCE_THRESHOLD;
    }

    private void CheckOneHandRaised(List<Vector3> cur)
    {
      bool left =
        GetLandmarkPosition(cur, 15).y -
        GetLandmarkPosition(cur, 11).y > HAND_RAISED_Y_THRESHOLD;

      bool right =
        GetLandmarkPosition(cur, 16).y -
        GetLandmarkPosition(cur, 12).y > HAND_RAISED_Y_THRESHOLD;

      if (left && !right) _oneHandRaisedState = "左手";
      else if (!left && right) _oneHandRaisedState = "右手";
      else _oneHandRaisedState = "无";
    }

    // ===== 只在状态变化时输出 Debug =====
    private void LogStateIfChanged()
    {
      if (_currentWalkingDirection != _lastLoggedWalkingDirection)
      {
        Debug.Log($"【移动】{_lastLoggedWalkingDirection} → {_currentWalkingDirection}");
        _lastLoggedWalkingDirection = _currentWalkingDirection;
      }

      if (_handsClaspedState != _lastLoggedHandsClasped)
      {
        Debug.Log($"【双手合拢】{_handsClaspedState}");
        _lastLoggedHandsClasped = _handsClaspedState;
      }

      if (_oneHandRaisedState != _lastLoggedOneHandRaised)
      {
        Debug.Log($"【举手】{_lastLoggedOneHandRaised} → {_oneHandRaisedState}");
        _lastLoggedOneHandRaised = _oneHandRaisedState;
      }
    }

    private void DisposeAllMasks(PoseLandmarkerResult result)
    {
      if (result.segmentationMasks == null) return;
      foreach (var m in result.segmentationMasks) m.Dispose();
    }
  }
}
