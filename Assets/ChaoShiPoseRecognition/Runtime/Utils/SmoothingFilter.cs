using UnityEngine;

namespace ChaoShiPoseRecognition.Utils
{
    /// <summary>
    /// 平滑滤波器接口
    /// </summary>
    public interface ISmoothingFilter
    {
        Vector3 Filter(Vector3 current, Vector3 previous);
        void Reset();
    }
    
    /// <summary>
    /// 指数平滑滤波器实现
    /// </summary>
    public class ExponentialSmoothingFilter : ISmoothingFilter
    {
        private float smoothingFactor;
        
        public ExponentialSmoothingFilter(float smoothingFactor = 0.7f)
        {
            this.smoothingFactor = Mathf.Clamp01(smoothingFactor);
        }
        
        public Vector3 Filter(Vector3 current, Vector3 previous)
        {
            return Vector3.Lerp(previous, current, 1f - smoothingFactor);
        }
        
        public void Reset()
        {
            // 无需重置状态
        }
    }
    
    /// <summary>
    /// 平滑滤波器管理器（为每个关键点维护独立的滤波器）
    /// </summary>
    public class SmoothingFilterManager
    {
        private ISmoothingFilter[] filters;
        private Vector3[] previousValues;
        private int filterCount;
        
        public SmoothingFilterManager(int count, float smoothingFactor = 0.7f)
        {
            filterCount = count;
            filters = new ISmoothingFilter[count];
            previousValues = new Vector3[count];
            
            for (int i = 0; i < count; i++)
            {
                filters[i] = new ExponentialSmoothingFilter(smoothingFactor);
            }
        }
        
        public Vector3 Filter(int index, Vector3 current)
        {
            if (index < 0 || index >= filterCount)
            {
                return current;
            }
            
            Vector3 previous = previousValues[index];
            Vector3 filtered = filters[index].Filter(current, previous);
            previousValues[index] = filtered;
            
            return filtered;
        }
        
        public void Reset()
        {
            for (int i = 0; i < filterCount; i++)
            {
                filters[i].Reset();
                previousValues[i] = Vector3.zero;
            }
        }
    }
}


