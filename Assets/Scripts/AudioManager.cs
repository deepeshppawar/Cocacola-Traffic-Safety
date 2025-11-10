using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;  // Assign the AudioSource in Inspector
    public AudioClip[] audioClips;   // Assign audio files in Inspector

    private int currentClipIndex = -1; // Track the current playing clip

    public void PlayAudio(int clipIndex)
    {
        if (clipIndex < 0 || clipIndex >= audioClips.Length)
        {
            Debug.LogWarning("Invalid clip index!");
            return;
        }

        // If the same clip is playing, stop it
        if (audioSource.isPlaying && currentClipIndex == clipIndex)
        {
            StopAudio();
            return;
        }

        // Play the selected clip
        audioSource.clip = audioClips[clipIndex];
        audioSource.Play();
        currentClipIndex = clipIndex;
    }

    public void StopAudio()
    {
        audioSource.Stop();
        currentClipIndex = -1; // Reset tracking index
    }
}
