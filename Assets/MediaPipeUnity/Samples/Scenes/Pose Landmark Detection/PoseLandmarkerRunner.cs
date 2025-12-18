// Copyright (c) 2023 homuler
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

    // ================= 公共状态开关 (供其他脚本调用) =================
    [Header("移动状态")]
    public bool IsIdle { get; private set; } = true;
    public bool IsForward { get; private set; }
    public bool IsBackward { get; private set; }
    public bool IsLeft { get; private set; }
    public bool IsRight { get; private set; }

    [Header("手势状态")]
    public bool IsHandsClasped { get; private set; }
    public bool IsHandRaisedLeft { get; private set; }
    public bool IsHandRaisedRight { get; private set; }
    // ===============================================================

    private Queue<List<Vector3>> _smoothedLandmarksQueue = new Queue<List<Vector3>>();
    private int _smoothingWindowSize = 8; 

    private string _currentDirection = "静止";
    private string _lastLoggedDirection = "";
    private bool _internalClasped = false;
    private bool _lastLoggedClasped = false;
    private string _currentHandRaised = "无";
    private string _lastLoggedHandRaised = "";

    private float _initialShoulderWidth = 0f;
    private float _initialCenterX = 0f;
    private bool _isCalibrated = false;
    private int _calibrationFrames = 0;

    private int _claspCounter = 0;
    private int _raiseCounter = 0;
    private const int CONFIRM_THRESHOLD = 10; 

    public override void Stop() {
      base.Stop();
      _textureFramePool?.Dispose();
      _textureFramePool = null;
      _isCalibrated = false;
      ResetAllBools();
    }

    protected override IEnumerator Run() {
      yield return AssetLoader.PrepareAssetAsync(config.ModelPath);
      var options = config.GetPoseLandmarkerOptions(config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM ? OnPoseLandmarkDetectionOutput : null);
      taskApi = PoseLandmarker.CreateFromOptions(options, GpuManager.GpuResources);
      var imageSource = ImageSourceProvider.ImageSource;
      yield return imageSource.Play();
      _textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);
      screen.Initialize(imageSource);
      SetupAnnotationController(_poseLandmarkerResultAnnotationController, imageSource);
      _poseLandmarkerResultAnnotationController.InitScreen(imageSource.textureWidth, imageSource.textureHeight);
      
      var transformOpt = imageSource.GetTransformationOptions();
      while (true) {
        if (isPaused) yield return new WaitWhile(() => isPaused);
        if (!_textureFramePool.TryGetTextureFrame(out var textureFrame)) { yield return new WaitForEndOfFrame(); continue; }
        var req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), transformOpt.flipHorizontally, transformOpt.flipVertically);
        yield return new WaitUntil(() => req.done);
        if (req.hasError) continue;
        Image image = textureFrame.BuildCPUImage();
        textureFrame.Release();
        taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: 0));
      }
    }

    private void OnPoseLandmarkDetectionOutput(PoseLandmarkerResult result, Image image, long timestamp) {
      _poseLandmarkerResultAnnotationController.DrawLater(result);
      if (result.poseLandmarks == null || result.poseLandmarks.Count == 0) {
        ResetAllBools();
        return;
      }

      List<Vector3> cur = new List<Vector3>();
      foreach (var lm in result.poseLandmarks[0].landmarks) cur.Add(new Vector3(lm.x, lm.y, lm.z));
      _smoothedLandmarksQueue.Enqueue(cur);
      if (_smoothedLandmarksQueue.Count > _smoothingWindowSize) _smoothedLandmarksQueue.Dequeue();
      List<Vector3> smoothed = CalculateAverage();

      DetectMovement(smoothed);
      DetectGestures(smoothed);
      UpdatePublicBools(); // 更新布尔开关
      LogStateIfChanged();
      DisposeAllMasks(result);
    }

    private void DetectMovement(List<Vector3> lms) {
      float currentWidth = Vector2.Distance(new Vector2(lms[11].x, lms[11].y), new Vector2(lms[12].x, lms[12].y));
      float currentCenterX = (lms[11].x + lms[12].x) / 2f;

      if (!_isCalibrated) {
        _initialShoulderWidth += currentWidth;
        _initialCenterX += currentCenterX;
        _calibrationFrames++;
        if (_calibrationFrames >= 30) {
          _initialShoulderWidth /= 30f;
          _initialCenterX /= 30f;
          _isCalibrated = true;
        }
        return;
      }

      float widthRatio = currentWidth / _initialShoulderWidth;
      if (widthRatio > 1.20f) _currentDirection = "向前";
      else if (widthRatio < 0.80f) _currentDirection = "向后";
      else if (currentCenterX - _initialCenterX > 0.10f) _currentDirection = "向左"; 
      else if (currentCenterX - _initialCenterX < -0.10f) _currentDirection = "向右";
      else _currentDirection = "静止";
    }

    private void DetectGestures(List<Vector3> lms) {
      // 双手合拢
      float dist = Vector3.Distance(lms[15], lms[16]);
      bool isCurrentlyClasped = dist < 0.1f;
      if (isCurrentlyClasped != _internalClasped) {
        _claspCounter++;
        if (_claspCounter >= CONFIRM_THRESHOLD) {
          _internalClasped = isCurrentlyClasped;
          _claspCounter = 0;
        }
      } else _claspCounter = 0;

      // 举手
      string handStatus = "无";
      if (lms[15].y < lms[11].y - 0.15f) handStatus = "左手";
      else if (lms[16].y < lms[12].y - 0.15f) handStatus = "右手";

      if (handStatus != _currentHandRaised) {
        _raiseCounter++;
        if (_raiseCounter >= CONFIRM_THRESHOLD) {
          _currentHandRaised = handStatus;
          _raiseCounter = 0;
        }
      } else _raiseCounter = 0;
    }

    // 更新布尔状态
    private void UpdatePublicBools() {
      IsIdle = _currentDirection == "静止";
      IsForward = _currentDirection == "向前";
      IsBackward = _currentDirection == "向后";
      IsLeft = _currentDirection == "向左";
      IsRight = _currentDirection == "向右";

      IsHandsClasped = _internalClasped;
      IsHandRaisedLeft = _currentHandRaised == "左手";
      IsHandRaisedRight = _currentHandRaised == "右手";
    }

    private void ResetAllBools() {
      IsIdle = true;
      IsForward = IsBackward = IsLeft = IsRight = false;
      IsHandsClasped = IsHandRaisedLeft = IsHandRaisedRight = false;
    }

    private void LogStateIfChanged() {
      if (_currentDirection != _lastLoggedDirection) {
        Debug.Log($"<color=cyan>【移动】{_currentDirection}</color>");
        _lastLoggedDirection = _currentDirection;
      }
      if (_internalClasped != _lastLoggedClasped) {
        Debug.Log($"<color=yellow>【双手】{(_internalClasped ? "已合拢" : "已分开")}</color>");
        _lastLoggedClasped = _internalClasped;
      }
      if (_currentHandRaised != _lastLoggedHandRaised) {
        Debug.Log($"<color=orange>【举手】{_currentHandRaised}</color>");
        _lastLoggedHandRaised = _currentHandRaised;
      }
    }

    private List<Vector3> CalculateAverage() {
      List<Vector3> avg = new List<Vector3>(new Vector3[33]);
      foreach (var frame in _smoothedLandmarksQueue)
        for (int i = 0; i < 33; i++) avg[i] += frame[i];
      for (int i = 0; i < 33; i++) avg[i] /= _smoothedLandmarksQueue.Count;
      return avg;
    }

    private void DisposeAllMasks(PoseLandmarkerResult result) {
      if (result.segmentationMasks != null) foreach (var m in result.segmentationMasks) m.Dispose();
    }
  }
}