// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using System.Collections.Generic; // Add this line
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

    // Custom variables for pose recognition
    private Queue<List<Vector3>> _smoothedLandmarksQueue;
    private int _smoothingWindowSize = 5; // Default smoothing window size
    private List<Vector3> _lastSmoothedPose;
    private string _currentWalkingDirection = "静止"; // "静止", "向前", "向后", "向左", "向右"
    private bool _handsClaspedState = false;
    private string _oneHandRaisedState = "无"; // "左手", "右手", "无"
    
    // Thresholds for gesture recognition
    private const float MOVEMENT_THRESHOLD = 0.005f; // Threshold for walking detection
    private const float HANDS_CLASPED_DISTANCE_THRESHOLD = 0.1f; // Threshold for hands clasped distance
    private const float HAND_RAISED_Y_THRESHOLD = 0.1f; // Threshold for hand raised detection (relative to shoulder)

    public override void Stop()
    {
      base.Stop();
      _textureFramePool?.Dispose();
      _textureFramePool = null;
      _smoothedLandmarksQueue?.Clear(); // Clear the queue on stop
    }

    protected override IEnumerator Run()
    {
      _smoothedLandmarksQueue = new Queue<List<Vector3>>(); // Initialize the queue
      _lastSmoothedPose = null; // Initialize last smoothed pose
      Debug.Log($"Delegate = {config.Delegate}");
      Debug.Log($"Image Read Mode = {config.ImageReadMode}");
      Debug.Log($"Model = {config.ModelName}");
      Debug.Log($"Running Mode = {config.RunningMode}");
      Debug.Log($"NumPoses = {config.NumPoses}");
      Debug.Log($"MinPoseDetectionConfidence = {config.MinPoseDetectionConfidence}");
      Debug.Log($"MinPosePresenceConfidence = {config.MinPosePresenceConfidence}");
      Debug.Log($"MinTrackingConfidence = {config.MinTrackingConfidence}");
      Debug.Log($"OutputSegmentationMasks = {config.OutputSegmentationMasks}");

      yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

      var options = config.GetPoseLandmarkerOptions(config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM ? OnPoseLandmarkDetectionOutput : null);
      taskApi = PoseLandmarker.CreateFromOptions(options, GpuManager.GpuResources);
      var imageSource = ImageSourceProvider.ImageSource;

      yield return imageSource.Play();

      if (!imageSource.isPrepared)
      {
        Logger.LogError(TAG, "Failed to start ImageSource, exiting...");
        yield break;
      }

      // Use RGBA32 as the input format.
      // TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so maybe the following code needs to be fixed.
      _textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);

      // NOTE: The screen will be resized later, keeping the aspect ratio.
      screen.Initialize(imageSource);

      SetupAnnotationController(_poseLandmarkerResultAnnotationController, imageSource);
      _poseLandmarkerResultAnnotationController.InitScreen(imageSource.textureWidth, imageSource.textureHeight);

      var transformationOptions = imageSource.GetTransformationOptions();
      var flipHorizontally = transformationOptions.flipHorizontally;
      var flipVertically = transformationOptions.flipVertically;

      // Always setting rotationDegrees to 0 to avoid the issue that the detection becomes unstable when the input image is rotated.
      // https://github.com/homuler/MediaPipeUnityPlugin/issues/1196
      var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: 0);

      AsyncGPUReadbackRequest req = default;
      var waitUntilReqDone = new WaitUntil(() => req.done);
      var waitForEndOfFrame = new WaitForEndOfFrame();
      var result = PoseLandmarkerResult.Alloc(options.numPoses, options.outputSegmentationMasks);

      // NOTE: we can share the GL context of the render thread with MediaPipe (for now, only on Android)
      var canUseGpuImage = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && GpuManager.GpuResources != null;
      using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

      while (true)
      {
        if (isPaused)
        {
          yield return new WaitWhile(() => isPaused);
        }

        if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
        {
          yield return new WaitForEndOfFrame();
          continue;
        }

        // Build the input Image
        Image image;
        switch (config.ImageReadMode)
        {
          case ImageReadMode.GPU:
            if (!canUseGpuImage)
            {
              throw new System.Exception("ImageReadMode.GPU is not supported");
            }
            textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            image = textureFrame.BuildGPUImage(glContext);
            // TODO: Currently we wait here for one frame to make sure the texture is fully copied to the TextureFrame before sending it to MediaPipe.
            // This usually works but is not guaranteed. Find a proper way to do this. See: https://github.com/homuler/MediaPipeUnityPlugin/pull/1311
            yield return waitForEndOfFrame;
            break;
          case ImageReadMode.CPU:
            yield return waitForEndOfFrame;
            textureFrame.ReadTextureOnCPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            image = textureFrame.BuildCPUImage();
            textureFrame.Release();
            break;
          case ImageReadMode.CPUAsync:
          default:
            req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            yield return waitUntilReqDone;

            if (req.hasError)
            {
              Debug.LogWarning($"Failed to read texture from the image source");
              continue;
            }
            image = textureFrame.BuildCPUImage();
            textureFrame.Release();
            break;
        }

        switch (taskApi.runningMode)
        {
          case Tasks.Vision.Core.RunningMode.IMAGE:
            if (taskApi.TryDetect(image, imageProcessingOptions, ref result))
            {
              _poseLandmarkerResultAnnotationController.DrawNow(result);
            }
            else
            {
              _poseLandmarkerResultAnnotationController.DrawNow(default);
            }
            DisposeAllMasks(result);
            break;
          case Tasks.Vision.Core.RunningMode.VIDEO:
            if (taskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions, ref result))
            {
              _poseLandmarkerResultAnnotationController.DrawNow(result);
            }
            else
            {
              _poseLandmarkerResultAnnotationController.DrawNow(default);
            }
            DisposeAllMasks(result);
            break;
          case Tasks.Vision.Core.RunningMode.LIVE_STREAM:
            taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
            break;
        }
      }
    }

    private void OnPoseLandmarkDetectionOutput(PoseLandmarkerResult result, Image image, long timestamp)
    {
      _poseLandmarkerResultAnnotationController.DrawLater(result);
      DisposeAllMasks(result);

      if (result.poseLandmarks != null && result.poseLandmarks.Count > 0)
      {
        // Convert NormalizedLandmarkList to List<Vector3>
        List<Vector3> currentLandmarks = new List<Vector3>();
        foreach (var landmark in result.poseLandmarks[0].landmarks) // Corrected to access the 'landmarks' property
        {
          currentLandmarks.Add(new Vector3(landmark.x, landmark.y, landmark.z)); // Changed X, Y, Z to x, y, z
        }

        // Add to queue and manage size
        _smoothedLandmarksQueue.Enqueue(currentLandmarks);
        while (_smoothedLandmarksQueue.Count > _smoothingWindowSize)
        {
          _smoothedLandmarksQueue.Dequeue();
        }

        // Calculate smoothed landmarks
        List<Vector3> currentSmoothedLandmarks = CalculateSmoothedLandmarks();

        // Perform gesture recognition
        CheckWalkingDirection(currentSmoothedLandmarks);
        CheckHandsClasped(currentSmoothedLandmarks);
        CheckOneHandRaised(currentSmoothedLandmarks);

        // Log results
        Debug.Log($"Smoothed Nose Position: {currentSmoothedLandmarks[0]}"); // NOSE is 0
        Debug.Log($"Walking Direction: {_currentWalkingDirection}");
        Debug.Log($"Hands Clasped: {_handsClaspedState}");
        Debug.Log($"One Hand Raised: {_oneHandRaisedState}");

        // Update last smoothed pose for next frame's comparison
        _lastSmoothedPose = new List<Vector3>(currentSmoothedLandmarks);
      }
    }

    // Helper to get landmark position
    private Vector3 GetLandmarkPosition(List<Vector3> landmarks, int index) // Change type to int
    {
      if (landmarks != null && index >= 0 && index < landmarks.Count) // Add index >= 0 check
      {
        return landmarks[index];
      }
      return Vector3.zero; // Or throw an exception, depending on desired error handling
    }

    // Calculate smoothed landmarks from the queue
    private List<Vector3> CalculateSmoothedLandmarks()
    {
      if (_smoothedLandmarksQueue.Count == 0)
      {
        return new List<Vector3>();
      }

      List<Vector3> smoothed = new List<Vector3>(new Vector3[_smoothedLandmarksQueue.Peek().Count]);

      foreach (var frameLandmarks in _smoothedLandmarksQueue)
      {
        for (int i = 0; i < frameLandmarks.Count; i++)
        {
          smoothed[i] += frameLandmarks[i];
        }
      }

      for (int i = 0; i < smoothed.Count; i++)
      {
        smoothed[i] /= _smoothedLandmarksQueue.Count;
      }

      return smoothed;
    }

    // Implementation of walking direction check
    private void CheckWalkingDirection(List<Vector3> currentSmoothedLandmarks)
    {
      if (_lastSmoothedPose == null || currentSmoothedLandmarks.Count == 0)
      {
        _currentWalkingDirection = "静止";
        return;
      }

      // Using NOSE as a reference point for movement
      Vector3 currentNose = GetLandmarkPosition(currentSmoothedLandmarks, 0); // NOSE is 0
      Vector3 lastNose = GetLandmarkPosition(_lastSmoothedPose, 0); // NOSE is 0

      float deltaX = currentNose.x - lastNose.x;
      float deltaZ = currentNose.z - lastNose.z; // Unity's Z is forward/backward

      if (Mathf.Abs(deltaX) > MOVEMENT_THRESHOLD || Mathf.Abs(deltaZ) > MOVEMENT_THRESHOLD)
      {
        if (Mathf.Abs(deltaX) > Mathf.Abs(deltaZ)) // Dominant horizontal movement
        {
          _currentWalkingDirection = deltaX > 0 ? "向右" : "向左";
        }
        else // Dominant vertical (forward/backward) movement
        {
          _currentWalkingDirection = deltaZ > 0 ? "向前" : "向后";
        }
      }
      else
      {
        _currentWalkingDirection = "静止";
      }
    }

    // Implementation of hands clasped check
    private void CheckHandsClasped(List<Vector3> currentSmoothedLandmarks)
    {
      if (currentSmoothedLandmarks.Count == 0)
      {
        _handsClaspedState = false;
        return;
      }

      Vector3 leftWrist = GetLandmarkPosition(currentSmoothedLandmarks, 15); // LEFT_WRIST is 15
      Vector3 rightWrist = GetLandmarkPosition(currentSmoothedLandmarks, 16); // RIGHT_WRIST is 16
      Vector3 nose = GetLandmarkPosition(currentSmoothedLandmarks, 0); // NOSE is 0
      Vector3 leftHip = GetLandmarkPosition(currentSmoothedLandmarks, 23); // LEFT_HIP is 23
      Vector3 rightHip = GetLandmarkPosition(currentSmoothedLandmarks, 24); // RIGHT_HIP is 24
      Vector3 centerHip = (leftHip + rightHip) / 2;

      float distanceBetweenWrists = Vector3.Distance(leftWrist, rightWrist);

      // Check if wrists are close enough
      bool wristsClose = distanceBetweenWrists < HANDS_CLASPED_DISTANCE_THRESHOLD;

      // Check if hands are in front of the body
      // Approximate body depth/z-position using nose and hip
      float bodyFrontZ = (nose.z + centerHip.z) / 2 + 0.05f; // Add a small offset to be "in front"
      bool handsInFront = leftWrist.z > bodyFrontZ && rightWrist.z > bodyFrontZ;

      // Check if hands are roughly in the torso area (Y-axis)
      float minHandY = Mathf.Min(leftWrist.y, rightWrist.y);
      float maxHandY = Mathf.Max(leftWrist.y, rightWrist.y);
      float shoulderY = (GetLandmarkPosition(currentSmoothedLandmarks, 11).y + GetLandmarkPosition(currentSmoothedLandmarks, 12).y) / 2; // LEFT_SHOULDER is 11, RIGHT_SHOULDER is 12
      float hipY = centerHip.y;

      bool handsAtTorsoHeight = minHandY < shoulderY && maxHandY > hipY;

      _handsClaspedState = wristsClose && handsInFront && handsAtTorsoHeight;
    }

    // Implementation of one hand raised check
    private void CheckOneHandRaised(List<Vector3> currentSmoothedLandmarks)
    {
      if (currentSmoothedLandmarks.Count == 0)
      {
        _oneHandRaisedState = "无";
        return;
      }

      Vector3 leftWrist = GetLandmarkPosition(currentSmoothedLandmarks, 15); // LEFT_WRIST is 15
      Vector3 rightWrist = GetLandmarkPosition(currentSmoothedLandmarks, 16); // RIGHT_WRIST is 16
      Vector3 leftShoulder = GetLandmarkPosition(currentSmoothedLandmarks, 11); // LEFT_SHOULDER is 11
      Vector3 rightShoulder = GetLandmarkPosition(currentSmoothedLandmarks, 12); // RIGHT_SHOULDER is 12
      Vector3 leftElbow = GetLandmarkPosition(currentSmoothedLandmarks, 13); // LEFT_ELBOW is 13
      Vector3 rightElbow = GetLandmarkPosition(currentSmoothedLandmarks, 14); // RIGHT_ELBOW is 14

      bool isLeftHandRaised = (leftWrist.y - leftShoulder.y) > HAND_RAISED_Y_THRESHOLD;
      bool isRightHandRaised = (rightWrist.y - rightShoulder.y) > HAND_RAISED_Y_THRESHOLD;

      // Add a check to ensure the elbow is also above the shoulder, to avoid false positives from bent arms
      isLeftHandRaised = isLeftHandRaised && (leftElbow.y - leftShoulder.y) > (HAND_RAISED_Y_THRESHOLD / 2);
      isRightHandRaised = isRightHandRaised && (rightElbow.y - rightShoulder.y) > (HAND_RAISED_Y_THRESHOLD / 2);


      if (isLeftHandRaised && !isRightHandRaised)
      {
        _oneHandRaisedState = "左手";
      }
      else if (!isLeftHandRaised && isRightHandRaised)
      {
        _oneHandRaisedState = "右手";
      }
      else
      {
        _oneHandRaisedState = "无";
      }
    }

    private void DisposeAllMasks(PoseLandmarkerResult result)
    {
      if (result.segmentationMasks != null)
      {
        foreach (var mask in result.segmentationMasks)
        {
          mask.Dispose();
        }
      }
    }
  }
}
