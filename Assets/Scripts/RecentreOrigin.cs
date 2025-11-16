using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using System.Collections;

public class RecentreOrigin : MonoBehaviour
{
    public Transform head;
    public Transform origin;
    public Transform target;

    //public InputActionProperty triggerAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DelayedRecentre());
    }

    // Update is called once per frame
    void Update()
    {
        // if(triggerAction.action.WasPressedThisFrame())
        // {
        //     Recentre();
        //     Debug.Log("Recentre Pressed");
        // }
    }

    public void Recentre()
    {
        // XROrigin xrOrigin = GetComponent<XROrigin>();
        // xrOrigin.MoveCameraToWorldLocation(target.position);
        // xrOrigin.MatchOriginUpCameraForward(target.up, target.forward);

        Vector3 offset = head.position - origin.position;
        offset.y = 0;
        origin.position = target.position - offset;
        Debug.Log("Recentred");
    }

    IEnumerator DelayedRecentre()
    {
        yield return new WaitForSeconds(1f); // Small delay to allow XR tracking to initialize
        Recentre();
    }


}
