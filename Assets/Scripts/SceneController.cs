using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour
{
    [Header("Default Delay")]
    public float delayInSeconds = 0f; // Default delay for all scenes

    // Call this from Button → OnClick(), pass Scene Name
    public void ChangeScene(string sceneName)
    {
        StartCoroutine(ChangeSceneAfterDelay(sceneName, delayInSeconds));
    }

    private IEnumerator ChangeSceneAfterDelay(string sceneName, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(sceneName);
    }
}
