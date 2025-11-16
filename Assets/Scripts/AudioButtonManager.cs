using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class AudioSet
{
    public Button playButton;   // Button to play audio
    public Button stopButton;   // Button to stop audio
    public AudioClip audioClip; // Audio file
}

public class AudioButtonManager : MonoBehaviour
{
    public List<AudioSet> audioSets = new List<AudioSet>(); // List of button-audio sets
    [SerializeField] private AudioSource audioSource; // Single AudioSource

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        foreach (AudioSet set in audioSets)
        {
            if (set.playButton != null)
                set.playButton.onClick.AddListener(() => PlayAudio(set.audioClip));
            
            if (set.stopButton != null)
                set.stopButton.onClick.AddListener(StopAudio);
        }
    }

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.clip = clip;
        audioSource.Play();
    }

    private void StopAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
