using UnityEngine;

public class faceuser : MonoBehaviour
{
    public Transform xrCamera; // Assign XR Camera (Main Camera inside XR Rig)

    void Update()
    {
        if (xrCamera != null)
        {
            // Make the object face the user's head position
            transform.LookAt(xrCamera);

            // Optional: Keep only the Y-axis rotation (good for UI elements)
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
    
    }
}
