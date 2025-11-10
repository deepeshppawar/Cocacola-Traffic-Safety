using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Input;
using System.Collections;

public class FrustumCheckSpecificCamera : MonoBehaviour
{
    public Camera targetCamera;
    public Renderer objectRenderer;
    public InputActionProperty triggerAction;
    public Material objectMaterial;
    public float shutter;
    public AudioSource shuttersound;
    public GameObject next1;
    public GameObject next2;
    public GameObject next3;
    public GameObject disable;
    public GameObject phone;
    public GameObject instructionText;

    public AudioManager audioManager; // Reference to AudioManager
    public int allButtonsPressedSoundIndex = 1; // Sound to play when all buttons are pressed

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;  // Default to main camera if not assigned
        }
        //objectRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
        if (GeometryUtility.TestPlanesAABB(planes, objectRenderer.bounds) && triggerAction.action.WasPressedThisFrame())
        {
            Debug.Log(gameObject.name + " is visible to " + targetCamera.name);
            Next();
        }

        if (triggerAction.action.WasPressedThisFrame())
        {
            ChangeColor();
            shuttersound.Play();
        }
    }

    public void ChangeColor()
    {
        StartCoroutine(ChangeColorCoroutine());
    }

    private IEnumerator ChangeColorCoroutine()
    {
        objectMaterial.color = Color.black;
        yield return new WaitForSeconds(shutter);
        objectMaterial.color = Color.white;
    }

    public void Next()
    {
        StartCoroutine(NextScreen());
    }

    private IEnumerator NextScreen()
    {
        yield return new WaitForSeconds(1f);
        next1.SetActive(true);
        next2.SetActive(true);
        next3.SetActive(true);
        disable.SetActive(false);
        phone.SetActive(false);
        instructionText.SetActive(false);

        if (audioManager != null)
                {
                    audioManager.PlayAudio(allButtonsPressedSoundIndex);
                }
    }
}


