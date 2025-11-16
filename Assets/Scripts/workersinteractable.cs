using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class workersinteractable:MonoBehaviour
{
    [Header("Trigger Button for Grab")]
    public InputActionProperty triggerAction; // Reference to the trigger button input

    private NearFarInteractor directInteractor;
    //private XRRayInteractor rayInteractor;

   
     private void Awake()
    {
        // Get the Direct and Ray Interactors
        directInteractor = GetComponent<NearFarInteractor>();
       // rayInteractor = GetComponent<XRRayInteractor>();
    }

    private void OnEnable()
    {
        triggerAction.action.Enable();
    }

    private void OnDisable()
    {
        triggerAction.action.Disable();
    }

    private void Update()
    {
        // Read trigger button value
        bool isTriggerPressed = triggerAction.action.ReadValue<float>() > 0.1f;

        // Force both interactors to use trigger button for selection
        if (directInteractor != null)
        {
            directInteractor.allowSelect = isTriggerPressed;
        }

     /*   if (rayInteractor != null)
        {
            rayInteractor.allowSelect = isTriggerPressed;
        }*/
    }

}

 
