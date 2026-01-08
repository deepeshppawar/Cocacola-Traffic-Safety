using UnityEngine;
using System.Collections;

public class SteeringRotateAction : InspectionAction
{
    [Header("Target")]
    public Transform steeringWheel;

    [Header("Rotation Settings")]
    [Tooltip("Rotation angle in degrees (applied in both directions)")]
    public float angle = 30f;

    [Tooltip("Rotation speed")]
    public float speed = 2f;

    public enum RotationAxis
    {
        LocalX,
        LocalY,
        LocalZ
    }

    [Header("Axis")]
    public RotationAxis rotationAxis = RotationAxis.LocalY;

    Coroutine rotationRoutine;

    public override void Execute()
    {
        if (rotationRoutine != null)
            StopCoroutine(rotationRoutine);

        rotationRoutine = StartCoroutine(RotateSequence());
    }

    IEnumerator RotateSequence()
    {
        if (!steeringWheel) yield break;

        Quaternion startRotation = steeringWheel.localRotation;

        Quaternion clockwise =
            startRotation * Quaternion.Euler(GetAxisVector(angle));

        Quaternion anticlockwise =
            startRotation * Quaternion.Euler(GetAxisVector(-angle));

        // Clockwise
        yield return RotateTo(clockwise);

        // Back to center
        yield return RotateTo(startRotation);

        // Anti-clockwise
        yield return RotateTo(anticlockwise);

        // Back to center
        yield return RotateTo(startRotation);
    }

    Vector3 GetAxisVector(float value)
    {
        switch (rotationAxis)
        {
            case RotationAxis.LocalX:
                return new Vector3(value, 0f, 0f);
            case RotationAxis.LocalY:
                return new Vector3(0f, value, 0f);
            case RotationAxis.LocalZ:
                return new Vector3(0f, 0f, value);
            default:
                return Vector3.zero;
        }
    }

    IEnumerator RotateTo(Quaternion target)
    {
        while (Quaternion.Angle(steeringWheel.localRotation, target) > 0.05f)
        {
            steeringWheel.localRotation = Quaternion.Slerp(
                steeringWheel.localRotation,
                target,
                Time.deltaTime * speed
            );
            yield return null;
        }

        steeringWheel.localRotation = target;
    }
}
