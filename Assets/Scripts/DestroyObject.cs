using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Input;
using System.Collections;

public class DestroyObject : MonoBehaviour
{
    public InputActionProperty triggerAction;
    public GameObject Tool;

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
            DestroyObject(Tool);
        }
    }
}
