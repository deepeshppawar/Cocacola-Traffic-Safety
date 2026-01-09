using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

public class XROriginTeleporter : MonoBehaviour
{
    [System.Serializable]
    public class TeleportPoint
    {
        [Header("Teleport Target")]
        public Transform target;

        [Header("Delay")]
        public float delay = 1f;

        [Header("Objects To Enable")]
        public List<GameObject> enableObjects = new List<GameObject>();

        [Header("Objects To Disable")]
        public List<GameObject> disableObjects = new List<GameObject>();
    }

    [Header("XR References")]
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Transform cameraOffset;

    [Header("Teleport Points")]
    [SerializeField] private List<TeleportPoint> teleportPoints = new List<TeleportPoint>();

    /// <summary>
    /// Call from Unity Events (Button, Trigger, etc.)
    /// </summary>
    public void TeleportToIndex(int index)
    {
        if (index < 0 || index >= teleportPoints.Count)
        {
            Debug.LogWarning("Teleport index out of range!");
            return;
        }

        StartCoroutine(TeleportRoutine(index));
    }

    private IEnumerator TeleportRoutine(int index)
    {
        TeleportPoint point = teleportPoints[index];

        // Delay
        yield return new WaitForSeconds(point.delay);

        if (point.target == null)
        {
            Debug.LogWarning("Teleport target is missing!");
            yield break;
        }

        // Teleport XR Origin
        Vector3 cameraOffsetPosition = cameraOffset.position - xrOrigin.transform.position;

        xrOrigin.transform.position = point.target.position - cameraOffsetPosition;

        xrOrigin.transform.rotation = Quaternion.Euler(
            0f,
            point.target.rotation.eulerAngles.y,
            0f
        );

        // Enable objects
        foreach (GameObject go in point.enableObjects)
        {
            if (go != null)
                go.SetActive(true);
        }

        // Disable objects
        foreach (GameObject go in point.disableObjects)
        {
            if (go != null)
                go.SetActive(false);
        }
    }
}
