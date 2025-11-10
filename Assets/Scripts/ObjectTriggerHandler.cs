using UnityEngine;
using System.Collections.Generic;

public class ObjectTriggerHandler : MonoBehaviour
{
    // Dictionary to store original positions and rotations of objects with tag "good"
    private Dictionary<GameObject, (Vector3, Quaternion)> originalTransforms = new Dictionary<GameObject, (Vector3, Quaternion)>();
    public CounterManager counterManager;

    private void Start()
    {
        // Find all objects with the tag "good" and store their original positions and rotations
        GameObject[] goodObjects = GameObject.FindGameObjectsWithTag("Good");
        foreach (GameObject obj in goodObjects)
        {
            if (!originalTransforms.ContainsKey(obj))
            {
                originalTransforms[obj] = (obj.transform.position, obj.transform.rotation);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Good"))
        {
            // Reset the object's position and rotation to its original state
            if (originalTransforms.ContainsKey(other.gameObject))
            {
                other.transform.position = originalTransforms[other.gameObject].Item1;
                other.transform.rotation = originalTransforms[other.gameObject].Item2;
                
                // Disable Rigidbody to prevent falling
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                rb.isKinematic = false;
            }
        }
        else if (other.CompareTag("Bad"))
        {
            // Destroy the object
            Destroy(other.gameObject);
            OnObjectPlaced();
        }
    }

    public void OnObjectPlaced()
    {
        if (counterManager != null)
        {
            counterManager.IncreaseCount();
        }
    }
}
