using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkerButtonManager : MonoBehaviour
{
    public List<Button> buttons; // List of buttons to manage
    private Button lastActiveButton; // Stores the last active button

    void Start()
    {
        // Assign click listeners to all buttons
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => OnButtonPressed(button));
        }
    }

    // Disables all buttons except the one pressed
    private void OnButtonPressed(Button activeButton)
    {
        lastActiveButton = activeButton;
        
        foreach (Button button in buttons)
        {
            button.gameObject.SetActive(button == activeButton);
        }
    }

    // Enables all buttons except the last active one
    public void EnableAllButLastActive()
    {
        foreach (Button button in buttons)
        {
            button.gameObject.SetActive(button != lastActiveButton);
        }
    }
}