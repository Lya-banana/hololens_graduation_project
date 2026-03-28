using UnityEngine;
using System.Collections.Generic;
using MixedReality.Toolkit.SpatialManipulation;

public class GameResetManager : MonoBehaviour
{
    public SequenceManager sequenceManager;
    public GameObject lid;

    public void FullReset()
    {
        // 1. 找到场景中所有带 ID 的物体
        ObjectIdentity[] allObjects = FindObjectsOfType<ObjectIdentity>();

        foreach (var ident in allObjects)
        {
            GameObject obj = ident.gameObject;

            // 2. 彻底重置物理
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
                rb.isKinematic = false;
            }

            // 3. 恢复抓取功能
            ObjectManipulator manipulator = obj.GetComponent<ObjectManipulator>();
            if (manipulator != null) manipulator.enabled = true;

            // 4. 命令物体重新回到相机面前
            SetInitialPosition sip = obj.GetComponent<SetInitialPosition>();
            if (sip != null)
            {
                sip.ForceReposition();
            }

            Debug.Log($"已重置并重新定位物体: {obj.name}");
        }

        // 5. 逻辑重置
        if (sequenceManager != null) sequenceManager.ResetZone();
        if (lid != null) lid.SetActive(false);

        Debug.Log("<color=orange>【系统】游戏已完全重置</color>");
    }
}