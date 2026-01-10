using UnityEngine;
using System.Collections;

public class HornInspection : MonoBehaviour
{
    [Header("Horn Audio")]
    public AudioSource hornAudio;

    [Header("Timing")]
    public float extraDelayAfterSound = 0.1f; // Optional buffer

    private bool isChecking = false;

    public System.Action OnInspectionCompleted;

    /// <summary>
    /// Call this to start horn inspection
    /// </summary>
    public void StartInspection()
    {
        if (isChecking || hornAudio == null) return;
        StartCoroutine(HornRoutine());
    }

    private IEnumerator HornRoutine()
    {
        isChecking = true;

        hornAudio.Play();

        // Wait for sound to finish
        yield return new WaitForSeconds(hornAudio.clip.length + extraDelayAfterSound);

        isChecking = false;
        OnInspectionCompleted?.Invoke();
    }
}
