using UnityEngine;
using UnityEngine.UI;

public class ImageFader : MonoBehaviour
{
    public CanvasGroup canvasGroup; // Assign the CanvasGroup of the UI Image
    public float fadeSpeed = 1.0f;
    private bool fadingIn = true;

    void Start()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        if (fadingIn)
        {
            canvasGroup.alpha += fadeSpeed * Time.deltaTime;
            if (canvasGroup.alpha >= 1.0f)
            {
                canvasGroup.alpha = 1.0f;
                fadingIn = false;
            }
        }
        else
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
            if (canvasGroup.alpha <= 0.0f)
            {
                canvasGroup.alpha = 0.0f;
                fadingIn = true;
            }
        }
    }
}
