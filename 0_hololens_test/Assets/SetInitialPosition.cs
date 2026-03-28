using UnityEngine;
using System.Collections;

public class SetInitialPosition : MonoBehaviour
{
    [Header("布局设置（米）")]
    [Tooltip("物体距离相机的距离")]
    public float distance = 0.6f;
    [Tooltip("基于相机视角的左右(x), 上下(y), 前后(z)偏移")]
    public Vector3 manualOffset;
    [Tooltip("物体的自定义旋转角度")]
    public Vector3 manualRotation;
    [Tooltip("是否锁定垂直方向，防止物体随着头低头而倾斜")]
    public bool lockVertical = true;

    [Header("调试信息（无需设置）")]
    [SerializeField] private bool isInitialized = false;

    void Start()
    {
        // 游戏启动时执行，需要等待追踪稳定
        ForceReposition(false);
    }

    // 公开方法：供重置按钮调用
    // parameter isReset: true 代表快重置，false 代表慢启动
    public void ForceReposition(bool isReset = true)
    {
        isInitialized = false;
        StopAllCoroutines(); // 停止冲突的协程
        StartCoroutine(InitialPlacementRoutine(isReset));
    }

    IEnumerator InitialPlacementRoutine(bool isReset)
    {
        // --- 关键优化：根据状态决定等待时间 ---
        if (!isReset)
        {
            // 游戏初次启动，等待 1 秒让 OpenXR 追踪系统定位头部位置
            Debug.Log($"<color=white>[{gameObject.name}] 慢启动定位中...</color>");
            yield return new WaitForSeconds(1.0f);
        }
        else
        {
            // 按钮重置，追踪已稳定，等待一帧清理物理状态即可
            Debug.Log($"<color=orange>[{gameObject.name}] 快速重置定位中...</color>");
            yield return new WaitForFixedUpdate();
        }

        if (Camera.main != null)
        {
            Transform cam = Camera.main.transform;

            // 诊断：打印相机此时的位置，如果全是 0，说明等的时间还不够
            // Debug.Log($"[{gameObject.name}] 相机此时坐标: {cam.position}");

            // 1. 计算目标世界坐标
            // 基础位置（相机正前方）
            Vector3 targetPos = cam.position + cam.forward * distance;
            // 应用基于相机本地坐标轴的偏移（左右、上下、前后）
            targetPos += cam.right * manualOffset.x;
            targetPos += cam.up * manualOffset.y;
            targetPos += cam.forward * manualOffset.z;

            transform.position = targetPos;

            // 2. 处理旋转
            if (lockVertical)
            {
                // 仅绕 Y 轴旋转，物体不会点头
                Vector3 lookTarget = cam.position;
                lookTarget.y = transform.position.y;
                transform.LookAt(lookTarget);
            }
            else
            {
                transform.LookAt(cam.position);
            }

            // 3. 应用修正和自定义旋转
            transform.Rotate(0, 180, 0); // 修正背面朝向问题
            transform.Rotate(manualRotation, Space.Self);

            isInitialized = true;
            Debug.Log($"<color=green>[{gameObject.name}] 定位完成。坐标: {transform.position}</color>");
        }
        else
        {
            Debug.LogError($"<color=red>[{gameObject.name}] 找不到 Main Camera！定位失败。</color>");
        }

        // 执行完一次后禁用脚本以节省资源，重置时会被 ForceReposition 重新激活
        this.enabled = false;
    }
}