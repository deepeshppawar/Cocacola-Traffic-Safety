using UnityEngine;
using UnityEngine.Video;

public class VideoEndHandler : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Assign in Inspector
    public GameObject uiGameObject1; // UI GameObject to enable
    public GameObject uiGameObject2;
    public GameObject screen;
    public GameObject skipButton;

    private bool hasVideoEnded = false; // prevents multiple calls

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // Subscribe to the event
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    public void OnVideoEnd(VideoPlayer vp)
    {
        if (hasVideoEnded) return; // stop if already triggered

        hasVideoEnded = true;

        Debug.Log("Video has ended!");

        uiGameObject1.SetActive(true);
        uiGameObject2.SetActive(true);
        skipButton.SetActive(false);
        screen.SetActive(false);

        // optional safety: unsubscribe from event
        videoPlayer.loopPointReached -= OnVideoEnd;
    }
}
