using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cart : MonoBehaviour
{
    public Transform parentToFollow; // 指定要跟随的父物体
    public float radius = 2.0f; // 旋转半径
    public float angleOffset = 0f; // 角度偏移量，可以微调位置
    
    void Update()
    {
        if (parentToFollow != null)
        {
            // 获取parentToFollow的位置
            Vector3 parentPosition = parentToFollow.position;
            
            // 获取parentToFollow的Y轴旋转角度
            float parentYRotation = parentToFollow.eulerAngles.y;
            
            // 计算当前物体应该所在的位置
            // 在前面（前方是Z轴正方向），所以不需要添加90度偏移
            float angleInRadians = (parentYRotation + angleOffset) * Mathf.Deg2Rad;
            float xPosition = parentPosition.x + radius * Mathf.Sin(angleInRadians);
            float zPosition = parentPosition.z + radius * Mathf.Cos(angleInRadians);
            
            // 保持当前物体的Y轴位置不变
            float currentYPosition = transform.position.y;
            
            // 设置新的位置
            transform.position = new Vector3(xPosition, currentYPosition, zPosition);
            
            // 让物体与parentToFollow保持相同的Y轴旋转
            Vector3 currentEuler = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(currentEuler.x, parentYRotation, currentEuler.z);
        }
    }
}