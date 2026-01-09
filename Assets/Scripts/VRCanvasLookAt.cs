using UnityEngine;

public class VRCanvasLookAt : MonoBehaviour
{
    public Transform cameraToLookAt; // Assign the XR Camera (Main Camera)

    [Header("Rotation Settings")]
    public bool smoothRotation = true;
    public float smoothSpeed = 5f;

    [Header("Axis Constraints")]
    public bool useAxisConstraints = false;
    public bool rotateX = false;
    public bool rotateY = true;
    public bool rotateZ = false;

    void Start()
    {
        if (cameraToLookAt == null)
        {
            cameraToLookAt = Camera.main?.transform;
        }
    }

    void Update()
    {
        if (cameraToLookAt == null) return;

        Vector3 direction = cameraToLookAt.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        if (useAxisConstraints)
        {
            Vector3 targetEuler = targetRotation.eulerAngles;
            Vector3 currentEuler = transform.rotation.eulerAngles;

            targetEuler.x = rotateX ? targetEuler.x : currentEuler.x;
            targetEuler.y = rotateY ? targetEuler.y : currentEuler.y;
            targetEuler.z = rotateZ ? targetEuler.z : currentEuler.z;

            targetRotation = Quaternion.Euler(targetEuler);
        }

        if (smoothRotation)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                smoothSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }
}
