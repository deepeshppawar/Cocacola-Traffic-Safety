using UnityEngine;

public class VRCanvasLookAt : MonoBehaviour
{
    public Transform cameraToLookAt; // Assign the XR Camera (Main Camera)
    public bool smoothRotation = true;
    public float smoothSpeed = 5f;

    void Start()
    {
        if (cameraToLookAt == null)
        {
            cameraToLookAt = Camera.main?.transform;
        }
    }

    void Update()
    {
        if (cameraToLookAt != null)
        {
            Vector3 direction = cameraToLookAt.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            if (smoothRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
    }
}
