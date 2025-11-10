using UnityEngine;
using System.Collections;

public class EnableGameObjectWithDelay : MonoBehaviour
{
    public GameObject targetObject; // Assign in Inspector
    public GameObject targetObject_2;
    public float delay = 2f; // Delay time in seconds

    void Start()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false); // Ensure it's disabled initially
            targetObject_2.SetActive(false);
        }
    }

    public void EnableWithDelay()
    {
        StartCoroutine(EnableAfterDelay());
    }

    IEnumerator EnableAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        if (targetObject != null)
        {
            targetObject_2.SetActive(false);
            targetObject.SetActive(true);
            Debug.Log("GameObject enabled after " + delay + " seconds.");
        }
    }
}
