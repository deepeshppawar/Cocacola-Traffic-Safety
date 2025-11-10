using UnityEngine;

public class ObjectActivator : MonoBehaviour
{
    public GameObject targetObject; // The object to enable
    public GameObject targetObject2;
    public int targetCount = 5; // The count threshold
    private int currentCount = 0;

    void Start()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false); // Ensure the object starts disabled
        }
    }

    public void IncrementCount()
    {
        currentCount++;
        CheckActivation();
    }

    private void CheckActivation()
    {
        if (currentCount >= targetCount && targetObject != null)
        {
            targetObject.SetActive(true);
            targetObject2.SetActive(true);
        }
    }
}
