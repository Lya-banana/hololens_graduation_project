using UnityEngine;

public class SetInitialPosition : MonoBehaviour
{
    void Start()
    {
        // 将物体放在相机（头）正前方 0.5 米处
        if (Camera.main != null)
        {
            transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
            // 让物体面向用户
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0); // 修正背面朝向
        }
    }
}