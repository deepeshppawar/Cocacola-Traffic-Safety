using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DragDropSnap : MonoBehaviour//module3 grab and place
{
    public Transform snapTarget; // The target position to snap to
    public float snapRadius = 1f; // The radius within which snapping will occur

    private XRSimpleInteractable grabInteractable;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isHeld = false;
    private Rigidbody rb;
    private Renderer snapTargetRenderer;
    private Material originalMaterial;
    public  Material highlightMaterial;
    public  GameObject snapTargetParent;
    public  GameObject tableParent;

    private void Awake()
    {
        grabInteractable = GetComponent<XRSimpleInteractable>();
     //   rb = GetComponent<Rigidbody>();

        originalPosition = transform.position;
        originalRotation = transform.rotation;
        if (grabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable component is missing from this GameObject. Please add it.", this);
            return;
        }

       /* if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing from this GameObject. Please add it.", this);
            return;
        }*/

        // Subscribe to grab events
     //   grabInteractable.activated.AddListener(OnGrabbed);
        
       // grabInteractable.deactivated.AddListener(OnReleased);

        if (snapTarget != null)
        {
            snapTargetRenderer = snapTarget.GetComponent<Renderer>();
            if (snapTargetRenderer != null)
            {
                originalMaterial = snapTargetRenderer.material;
            }
        }
    }

    private void OnDestroy()
    {
       /* if (grabInteractable != null)
        {
            // Unsubscribe from grab events
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }

        if (snapTargetRenderer != null && originalMaterial != null)
        {
            snapTargetRenderer.material = originalMaterial; // Restore original material on destroy
        }*/
    }
   

    private void OnGrabbed(ActivateEventArgs args)
    {
        isHeld = true;
        Debug.Log("grab");
        // Save the original position and rotation

        snapTargetParent.SetActive(true);
        // Highlight the snap target
        if (snapTargetRenderer != null)
        {
           // snapTargetRenderer.material = highlightMaterial;
        }
    }

    private void OnReleased(DeactivateEventArgs args)
    {
        Debug.Log("release");
        isHeld = false;
        snapTargetParent.SetActive(false);
        // Check if the object is within snapping range
      //  if (Vector3.Distance(transform.position, snapTarget.position) <= snapRadius)
        {
            SnapToTarget();
        }
      /*  else
        {
            Invoke(nameof(ReturnToOriginalPosition), 1.2f); // Delay before returning to the original position
        }*/

        // Restore the snap target's original material
        if (snapTargetRenderer != null && originalMaterial != null)
        {          
            //snapTargetRenderer.material = originalMaterial;
        }
    }

    public  void SnapToTarget()
    {
        snapTargetParent.SetActive(false);// added by me

        transform.position = snapTarget.position;
        transform.rotation = snapTarget.rotation;

       // GameManager.instance.snapM3Count++;

        // Disable grabbing and make the object kinematic
      //  grabInteractable.enabled = false;
        //rb.isKinematic = true;

        // Restore the snap target's original material
        if (snapTargetRenderer != null && originalMaterial != null)
        {
            //snapTargetRenderer.material = originalMaterial;
        }

        if (GameManager.instance.selectionindex == 1)
        {
           
           
            GameManager.instance.Screens[5].SetActive(true);// module 
         
            
        }
    }

    private void ReturnToOriginalPosition()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the snap radius in the editor
        if (snapTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(snapTarget.position, snapRadius);
        }
    }
}
    