using UnityEngine;
using UnityEngine.XR; // Ensure XR namespace is included
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class XROriginTransformSetter : MonoBehaviour
{
    public Transform referenceObject; // Assign the reference object in the Inspector
    public GameObject xrOrigin;

    void Start()
    {
        // xrOrigin = GetComponent<XROrigin>();
        // if (xrOrigin == null)
        // {
        //     Debug.LogError("XROrigin component not found on this GameObject.");
        //     return;
        // }

        if (referenceObject != null)
        {
            // Set XR Origin's transform to match the reference object
            xrOrigin.transform.position = referenceObject.position;
            xrOrigin.transform.rotation = referenceObject.rotation;
            xrOrigin.transform.localScale = referenceObject.localScale;
        }
        else
        {
            Debug.LogError("Reference object not assigned.");
        }
    }
}
