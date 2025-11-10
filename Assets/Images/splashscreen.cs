using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class splashscreen : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(timer(5f));
    }

 IEnumerator timer(float sec)
    {
        yield return new WaitForSeconds(sec);
        SceneManager.LoadScene(1);
    }
}
