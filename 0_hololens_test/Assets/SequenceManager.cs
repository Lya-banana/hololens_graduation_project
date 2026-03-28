using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using MixedReality.Toolkit.SpatialManipulation;

public class SequenceManager : MonoBehaviour
{
    public int requiredCount = 3;
    public GameObject lid; // 在 Inspector 中拖入你的 Lid 物体
    public List<int> currentSequence = new List<int>();
    public UnityEvent OnSequenceComplete;

    private List<GameObject> objectsInsideTrigger = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        // 如果已经有三个了，就不再允许新的检测（防止第四个触发）
        if (currentSequence.Count >= requiredCount) return;

        ObjectIdentity identity = other.GetComponent<ObjectIdentity>();
        if (identity != null && !objectsInsideTrigger.Contains(other.gameObject))
        {
            objectsInsideTrigger.Add(other.gameObject);
            ObjectManipulator manipulator = other.GetComponent<ObjectManipulator>();
            if (manipulator != null)
                manipulator.selectExited.AddListener((args) => LockObject(other.gameObject));
        }
    }

    private void LockObject(GameObject obj)
    {
        if (!objectsInsideTrigger.Contains(obj) || currentSequence.Count >= requiredCount) return;

        ObjectIdentity identity = obj.GetComponent<ObjectIdentity>();
        if (identity == null || currentSequence.Contains(identity.objectID)) return;

        currentSequence.Add(identity.objectID);
        StartCoroutine(PhysicsHandoverRoutine(obj));
    }

    private IEnumerator PhysicsHandoverRoutine(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.drag = 0.5f;
            rb.velocity = new Vector3(0, -1.5f, 0);
            rb.WakeUp();
        }

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        ObjectManipulator manipulator = obj.GetComponent<ObjectManipulator>();
        if (manipulator != null) manipulator.enabled = false;

        if (currentSequence.Count == requiredCount)
        {
            // 达成三个，关上盖子
            if (lid != null) lid.SetActive(true);
            OnSequenceComplete?.Invoke();
            Debug.Log("<color=green>序列完成，盖子已关闭</color>");
        }
    }

    public void ResetZone()
    {
        currentSequence.Clear();
        objectsInsideTrigger.Clear();
    }
}