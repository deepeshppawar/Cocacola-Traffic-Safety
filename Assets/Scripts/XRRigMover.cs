using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class XRRigMover : MonoBehaviour
{
    public List<Transform> referencePoints = new List<Transform>(); // List of positions
    public XROrigin xrOrigin; // Reference to the XR Origin
    public float moveDelay = 1.0f; // Delay before moving
    public GameObject step2;
    public GameObject step2Canvas;

    public void MoveToPosition(int index)
    {
        if (index < 0 || index >= referencePoints.Count)
        {
            Debug.LogWarning("Invalid index. Make sure it's within the range of reference points.");
            return;
        }
        StartCoroutine(MoveAfterDelay(index));
    }

    private IEnumerator MoveAfterDelay(int index)
    {
        yield return new WaitForSeconds(moveDelay);
        
        if (xrOrigin != null && referencePoints[index] != null)
        {
            xrOrigin.transform.position = referencePoints[index].position;
            xrOrigin.transform.rotation = referencePoints[index].rotation; // Move and rotate the XR Origin
            step2.SetActive(true);
            step2Canvas.SetActive(true);
        }
        else
        {
            Debug.LogWarning("XR Origin or reference point is missing.");
        }
    }
}