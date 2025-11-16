using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Input;
using System.Collections;

public class OpenXRInput : MonoBehaviour
{
    public GameObject walkieTalkie;
    public InputActionProperty triggerAction;
    public AudioSource walkieTalkie_Audio;
    private bool hasPlayed = false; // Flag to track if audio has been played
    public GameObject nextStep1;
    public GameObject nextStep2;
    public GameObject nextStep3;
    public GameObject nextStep4;

    public GameObject disable;

    public AudioManager audioManager; // Reference to AudioManager
    public int allButtonsPressedSoundIndex = 1; // Sound to play when all buttons are pressed
    

    void Update()
    {
        if (triggerAction.action.WasPressedThisFrame() && !hasPlayed)
        {
            Debug.Log("Trigger button pressed!");
            walkieTalkie_Audio.Play();
            hasPlayed = true; // Mark audio as played
            StartCoroutine(DisableAfterAudio()); // Wait for audio to finish before disabling
        }
    }

    private IEnumerator DisableAfterAudio()
    {
        yield return new WaitForSeconds(2f);
        walkieTalkie.SetActive(false);
        nextStep1.SetActive(true);
        nextStep2.SetActive(true);
        nextStep3.SetActive(true);
        nextStep4.SetActive(true);
        disable.SetActive(false);

         if (audioManager != null)
                {
                    audioManager.PlayAudio(allButtonsPressedSoundIndex);
                }
    }
}
