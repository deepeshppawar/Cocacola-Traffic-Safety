using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Input;
using System.Collections;

using UnityEngine.Events;

public class TestScript : MonoBehaviour
{

    public InputActionProperty triggerAction;
    public UnityEvent onCustomEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (triggerAction.action.WasPressedThisFrame())
        {
            Debug.Log("Trigger button pressed!");
            onCustomEvent.Invoke();
        }
    }
    

    public void Select()
    {
        Debug.Log("Select");
    }

    public void Activate()
    {
        Debug.Log("Activate");
    }

}
