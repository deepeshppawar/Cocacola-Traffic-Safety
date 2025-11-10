using UnityEngine;
using System.Collections;

public class RotationController : MonoBehaviour
{
    public GameObject instantRotationObject; // Object that resets rotation instantly
    public GameObject lerpRotationObject; // Object that rotates smoothly
    public float rotationSpeed = 2f; // Speed of Lerp rotation

    public void SetInstantRotation()
    {
        if (instantRotationObject != null)
        {
            instantRotationObject.transform.localRotation = Quaternion.identity; // Set local rotation to 0 instantly
        }
    }

    public void StartLerpRotation()
    {
        if (lerpRotationObject != null)
        {
            StartCoroutine(LerpRotationCoroutine());
        }
    }

    private IEnumerator LerpRotationCoroutine()
    {
        float initialYRotation = lerpRotationObject.transform.localEulerAngles.y;
        float targetYRotation = 270f;
        float lerpProgress = 0f;

        while (lerpProgress < 1f)
        {
            lerpProgress += Time.deltaTime * rotationSpeed;
            float newYRotation = Mathf.Lerp(initialYRotation, targetYRotation, lerpProgress);
            
            Vector3 currentEulerAngles = lerpRotationObject.transform.localEulerAngles;
            lerpRotationObject.transform.localEulerAngles = new Vector3(currentEulerAngles.x, newYRotation, currentEulerAngles.z);
            
            yield return null;
        }

        // Ensure final rotation is exactly 270 degrees
        lerpRotationObject.transform.localEulerAngles = new Vector3(lerpRotationObject.transform.localEulerAngles.x, targetYRotation, lerpRotationObject.transform.localEulerAngles.z);
    }
}
