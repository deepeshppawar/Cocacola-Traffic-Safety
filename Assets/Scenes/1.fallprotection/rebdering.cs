//using GLTFast.Schema;
using TMPro;
//using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class rebdering : MonoBehaviour
{
    public float radius = 3f; // Fixed orbit radius
    public float stepAngle = 10f; // Angle to move per click (degrees)

    private float currentAngle = 0f; // Tracks current position in degrees

    public Transform xrCameraTransform;  // Assign the VR Camera

    public float distanceFromUser = 2f;  // Distance in front of the user
    public float heightOffset = 0.5f;  // Adjust height if needed
    public GameObject objectToSpawn;

    void Start()
    {
       // disatnce();
    //  MoveInSteps();
  
    }
    private void Update()
    {
        // rotate2();
      if( ! IsPanelInView())
        SpawnObject();
    }

    public void rotate()
    {


        if (xrCameraTransform != null)
        {
            // Keep UI in front of the user
            Vector3 targetPosition = xrCameraTransform.position + xrCameraTransform.forward * distanceFromUser;
            //   Debug.Log(targetPosition);
            // targetPosition.y += heightOffset; // Adjust height
            transform.position = targetPosition;

            // Rotate UI to face the user
            transform.LookAt(new Vector3(xrCameraTransform.position.x, transform.position.y, xrCameraTransform.position.z));
            transform.Rotate(0, -135, 0); // Flip to face correctly

        }
    }

    public void rotate2()
    {
        // Calculate target position
        Vector3 targetPosition = xrCameraTransform.position + (xrCameraTransform.forward * distanceFromUser);
        //     targetPosition.y += heightOffset; // Adjust height

        // Move UI smoothly or instantly
        // if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
        }
        /* else
         {
             transform.position = targetPosition;
         }*/

        // Make UI face the user
        transform.rotation = Quaternion.LookRotation(transform.position - xrCameraTransform.position);
        // transform.position=new Vector3(transform.position.x,transform.position.y,);
    }
    void MoveInSteps()
    {
        if (xrCameraTransform != null)
        {
            currentAngle += stepAngle; // Increase angle by 10 degrees

            // Convert angle to radians (since Mathf.Sin/Cos use radians)
            float radians = currentAngle * Mathf.Deg2Rad;

            // Calculate new position
            float x = xrCameraTransform.position.x + Mathf.Cos(radians) * radius;
            float z = xrCameraTransform.position.z + Mathf.Sin(radians) * radius;

            // Apply new position while keeping the same Y height
            transform.position = new Vector3(x, transform.position.y, z);
        }
    }
    void disatnce()
    {

        // Calculate distance
        float distance = Vector3.Distance(transform.position, xrCameraTransform.position);
        xrCameraTransform.rotation=Quaternion.Euler(new Vector3(0,stepAngle,0));

        Vector3 z = xrCameraTransform.forward+new Vector3(0,0,7f);
        Vector3 x = xrCameraTransform.right;
        Vector3 targetpos = new Vector3(x.x, transform.position.y,z.z);
        transform.position= targetpos;
        Debug.Log("Distance between objectsz: " + z);
        Debug.Log("Distance between objectsz: " + x);
        Debug.Log("Distance between objects: " + targetpos);

    }
    public float checkAngle = 70f; // Max angle before repositioning
    void SpawnObject()
    {
      
            // Calculate spawn position in front of the player
           // Vector3 spawnPosition = xrCameraTransform.position + xrCameraTransform.forward * radius;
          //  spawnPosition.y += heightOffset; // Adjust height if needed
            //  GameObject ob=  Instantiate(objectToSpawn, spawnPosition, Quaternion.LookRotation(xrCameraTransform.forward));
                            
          //  GameObject ob=    Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
         // ob.transform.LookAt(xrCameraTransform);
  Vector3 spawnPosition = xrCameraTransform.position + new Vector3(xrCameraTransform.forward.x, 0, xrCameraTransform.forward.z).normalized * radius;
  
      objectToSpawn.transform.position = spawnPosition;
objectToSpawn.transform.LookAt(new Vector3(xrCameraTransform.position.x,objectToSpawn.transform.position.y,xrCameraTransform.position.z));

        objectToSpawn.transform.forward *= -1f;
    }
    bool IsPanelInView()
    {
        Vector3 toPanel = (transform.position -xrCameraTransform.position).normalized;
        float angle = Vector3.Angle(xrCameraTransform.forward, toPanel);

        return angle < checkAngle; // True if within the field of view
    }
}
      
    

 
    

