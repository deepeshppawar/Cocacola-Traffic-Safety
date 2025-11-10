using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip rightSound;
    public AudioClip wrongSound;
    
    public List<Button> rightButtons;
    public List<Button> wrongButtons;

    void Start()
    {
        // Assign listeners to right buttons
        foreach (Button btn in rightButtons)
        {
            btn.onClick.AddListener(() => PlaySound(true));
        }

        // Assign listeners to wrong buttons
        foreach (Button btn in wrongButtons)
        {
            btn.onClick.AddListener(() => PlaySound(false));
        }
    }

    void PlaySound(bool isRight)
    {
        if (audioSource != null)
        {
            audioSource.clip = isRight ? rightSound : wrongSound;
            audioSource.Play();
        }
    }
}
