using UnityEngine;

public class CounterManager : MonoBehaviour
{
    public int count = 0; // Starting count
    public int targetCount = 5; // Set this to the desired number
    public GameObject objectToEnable; // Assign in the Inspector

    public void IncreaseCount()
    {
        count++;
        Debug.Log("Count increased: " + count);

        // Check if count reached target
        if (count >= targetCount)
        {
            EnableGameObject();
        }
    }

    private void EnableGameObject()
    {
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(true);
            Debug.Log("Target count reached! GameObject enabled.");
        }
    }
}
