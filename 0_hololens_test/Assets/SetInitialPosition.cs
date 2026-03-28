using UnityEngine;
using System.Collections;

public class SetInitialPosition : MonoBehaviour
{
    [Header("位置布局")]
    public float distance = 0.6f;      // 距离头部的距离
    public Vector3 manualOffset;      // 每个物体专属的位移量 (x 为左右, y 为上下)

    [Header("旋转布局")]
    public Vector3 manualRotation;    // 每个物体专属的额外旋转量 (角度)

    [Header("配置")]
    public bool lockVertical = true;  // 是否锁定垂直方向（防止物体歪斜）

    void Start()
    {
        // 使用协程延迟执行，等待 HoloLens 追踪稳定
        StartCoroutine(InitialPlacementRoutine());
    }

    IEnumerator InitialPlacementRoutine()
    {
        // 等待 1 秒左右，确保设备已经识别到空间并确定了头部位置
        yield return new WaitForSeconds(1.0f);

        if (Camera.main != null)
        {
            Transform cam = Camera.main.transform;

            // 1. 计算基础位置（相机正前方）
            Vector3 targetPos = cam.position + cam.forward * distance;

            // 2. 根据相机本地坐标轴进行偏移
            targetPos += cam.right * manualOffset.x;
            targetPos += cam.up * manualOffset.y;
            targetPos += cam.forward * manualOffset.z;

            transform.position = targetPos;

            // 3. 面向用户逻辑
            if (lockVertical)
            {
                // 仅绕 Y 轴旋转，物体不会上下“点头”
                Vector3 lookTarget = cam.position;
                lookTarget.y = transform.position.y;
                transform.LookAt(lookTarget);
            }
            else
            {
                transform.LookAt(cam.position);
            }

            // 4. 应用旋转修正
            // 先旋转 180 度修正模型背面朝向问题
            transform.Rotate(0, 180, 0);

            // 再应用你在 Inspector 面板中设置的自定义旋转
            // 使用 Space.Self 确保是基于物体自身的局部坐标轴进行旋转
            transform.Rotate(manualRotation, Space.Self);
        }

        // 执行完一次后禁用脚本
        this.enabled = false;
    }
}