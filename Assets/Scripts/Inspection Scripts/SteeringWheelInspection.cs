using UnityEngine;
using System.Collections;

public class SteeringWheelInspection : MonoBehaviour
{
    [Header("Steering Wheel")]
    public Transform steeringWheel;

    [Header("Rotation Settings")]
    public Vector3 localRotationAxis = Vector3.forward; // Axis in local space
    public float rotationAngle = 90f;                   // Degrees
    public float rotationSpeed = 180f;                  // Degrees per second

    private bool isChecking = false;
    private Quaternion initialLocalRotation;

    public System.Action OnInspectionCompleted;

    private void Awake()
    {
        if (steeringWheel == null)
            steeringWheel = transform;

        initialLocalRotation = steeringWheel.localRotation;
    }

    /// <summary>
    /// Call this to start steering wheel inspection
    /// </summary>
    public void StartInspection()
    {
        if (isChecking) return;
        StartCoroutine(InspectionRoutine());
    }

    private IEnumerator InspectionRoutine()
    {
        isChecking = true;

        Quaternion clockwiseTarget =
            initialLocalRotation * Quaternion.AngleAxis(rotationAngle, localRotationAxis);

        Quaternion anticlockwiseTarget =
            initialLocalRotation * Quaternion.AngleAxis(-rotationAngle, localRotationAxis);

        // Rotate clockwise
        yield return RotateTo(clockwiseTarget);

        // Back to center
        yield return RotateTo(initialLocalRotation);

        // Rotate anticlockwise
        yield return RotateTo(anticlockwiseTarget);

        // Back to center
        yield return RotateTo(initialLocalRotation);

        isChecking = false;
        OnInspectionCompleted?.Invoke();
    }

    private IEnumerator RotateTo(Quaternion targetRotation)
    {
        while (Quaternion.Angle(steeringWheel.localRotation, targetRotation) > 0.1f)
        {
            steeringWheel.localRotation = Quaternion.RotateTowards(
                steeringWheel.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            yield return null;
        }

        steeringWheel.localRotation = targetRotation;
    }
}
