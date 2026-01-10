using UnityEngine;
using System.Collections;

public class WiperBladesInspection : MonoBehaviour
{
    [System.Serializable]
    public class WiperBlade
    {
        public Transform blade;
        public Vector3 localRotationAxis = Vector3.forward;
    }

    [Header("Wiper Blades")]
    public WiperBlade[] wiperBlades;

    [Header("Sweep Settings")]
    public float sweepAngle = 60f;        // Degrees
    public float rotationSpeed = 200f;    // Degrees per second
    public int sweepCount = 2;             // Number of cycles

    private bool isChecking = false;
    private Quaternion[] initialLocalRotations;

    public System.Action OnInspectionCompleted;

    private void Awake()
    {
        initialLocalRotations = new Quaternion[wiperBlades.Length];

        for (int i = 0; i < wiperBlades.Length; i++)
        {
            initialLocalRotations[i] = wiperBlades[i].blade.localRotation;
        }
    }

    /// <summary>
    /// Call this to start wiper blade inspection
    /// </summary>
    public void StartInspection()
    {
        if (isChecking) return;
        StartCoroutine(WiperRoutine());
    }

    private IEnumerator WiperRoutine()
    {
        isChecking = true;

        for (int i = 0; i < sweepCount; i++)
        {
            // Sweep forward
            yield return RotateByAngle(sweepAngle);

            // Sweep back
            yield return RotateByAngle(-sweepAngle);
        }

        // Ensure exact reset
        for (int i = 0; i < wiperBlades.Length; i++)
        {
            wiperBlades[i].blade.localRotation = initialLocalRotations[i];
        }

        isChecking = false;
        OnInspectionCompleted?.Invoke();
    }

    private IEnumerator RotateByAngle(float angle)
    {
        float rotated = 0f;
        float direction = Mathf.Sign(angle);
        float target = Mathf.Abs(angle);

        while (Mathf.Abs(rotated) < target)
        {
            float step = rotationSpeed * Time.deltaTime;
            float delta = Mathf.Min(step, target - Mathf.Abs(rotated));

            for (int i = 0; i < wiperBlades.Length; i++)
            {
                wiperBlades[i].blade.Rotate(
                    wiperBlades[i].localRotationAxis,
                    delta * direction,
                    Space.Self
                );
            }

            rotated += delta * direction;
            yield return null;
        }
    }
}
