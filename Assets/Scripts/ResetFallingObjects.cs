using UnityEngine;
using System.Collections.Generic;

public class ResetFallingObjects : MonoBehaviour
{
    [SerializeField] private List<GameObject> trackedObjects; // Assign manually in the Inspector
    private static Dictionary<GameObject, (Vector3 position, Quaternion rotation)> originalTransforms = new Dictionary<GameObject, (Vector3, Quaternion)>();

    void Start()
    {
        // Store the original positions and rotations of assigned objects
        foreach (GameObject obj in trackedObjects)
        {
            if (obj != null && !originalTransforms.ContainsKey(obj))
            {
                originalTransforms.Add(obj, (obj.transform.position, obj.transform.rotation));
                Debug.Log($"Stored position of {obj.name}: {obj.transform.position}");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Good") || collision.gameObject.CompareTag("Bad"))
        {
            if (originalTransforms.TryGetValue(collision.gameObject, out var originalTransform))
            {
                Debug.Log($"Resetting {collision.gameObject.name} to {originalTransform.position}");

                collision.gameObject.transform.position = originalTransform.position;
                collision.gameObject.transform.rotation = originalTransform.rotation;

                Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                    rb.isKinematic = false;
                }
            }
        }
    }
}
