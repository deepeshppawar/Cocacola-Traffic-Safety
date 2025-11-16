using UnityEngine;
using UnityEngine.Video;

public class VideoEndHandler : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Assign in Inspector
    public GameObject uiGameObject1; // UI GameObject to enable
    public GameObject uiGameObject2;
    public GameObject screen;
    public GameObject skipButton;

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // Subscribe to the event
        videoPlayer.loopPointReached += OnVideoEnd;

        // Ensure the UI elements are disabled initially
        //if (uiGameObject != null) uiGameObject.SetActive(false);
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Video has ended!");

        // Enable the whole UI GameObject
            uiGameObject1.SetActive(true);
            uiGameObject2.SetActive(true);
            skipButton.SetActive(false);
            screen.SetActive(false);

    }
}
