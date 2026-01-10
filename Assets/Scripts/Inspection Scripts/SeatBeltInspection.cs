using UnityEngine;
using System.Collections;

public class SeatBeltInspection : MonoBehaviour
{
    [Header("Seat Belt Objects")]
    public GameObject[] objectsToEnable;
    public GameObject[] objectsToDisable;

    [Header("Timing")]
    public float inspectionDuration = 1.5f; // Time to keep state visible

    private bool isChecking = false;

    public System.Action OnInspectionCompleted;

    /// <summary>
    /// Call this to start seat belt inspection
    /// </summary>
    public void StartInspection()
    {
        if (isChecking) return;
        StartCoroutine(SeatBeltRoutine());
    }

    private IEnumerator SeatBeltRoutine()
    {
        isChecking = true;

        // Enable seat belt objects
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // Disable other objects
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Wait so the change is noticeable
        yield return new WaitForSeconds(inspectionDuration);

        isChecking = false;
        OnInspectionCompleted?.Invoke();
    }
}
