using UnityEngine;
using UnityEngine.InputSystem;

public class VRRaycastDestroy : MonoBehaviour
{
    public InputActionProperty triggerAction; // Assign via Inspector
    public LayerMask targetLayer; // Only hit objects with this layer
    public float rayDistance = 10f;

    private void Update()
    {
        if (triggerAction.action.WasPressedThisFrame()) // Detect trigger press
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, targetLayer))
            {
                Destroy(hit.collider.gameObject); // Destroy object hit by the ray
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * rayDistance); // Draw debug ray
    }
}
