using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public Button[] buttons; // Assign buttons in the Inspector
    [Header("Enable")]
    public GameObject[] Enable;
    [Header("Disable")]
    public GameObject[] Disable;
    [Header("Audio")]
    public AudioManager audioManager; // Reference to AudioManager
    //public int buttonPressSoundIndex = 0; // Sound to play when a button is pressed
    public int allButtonsPressedSoundIndex = 1; // Sound to play when all buttons are pressed

    private int pressedCount = 0;

    void Start()
    {
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => OnButtonPressed(btn));
        }
    }

    void OnButtonPressed(Button button)
    {
        if (button.interactable)
        {
            button.interactable = false; // Disable the button after pressing
            pressedCount++;
            Debug.Log("Button Pressed");

            // Play button press sound
            // if (audioManager != null)
            // {
            //     audioManager.PlayAudio(buttonPressSoundIndex);
            // }

            if (pressedCount == buttons.Length)
            {
                ToggleObjects();
                Debug.Log("All Buttons Pressed");
                
                // Play all-buttons-pressed sound
                if (audioManager != null)
                {
                    audioManager.PlayAudio(allButtonsPressedSoundIndex);
                }
            }
        }
    }

    void ToggleObjects()
    {
        foreach (GameObject obj in Enable)
        {
            if (obj) obj.SetActive(true);
        }
        
        foreach (GameObject obj in Disable)
        {
            if (obj) obj.SetActive(false);
        }
    }
}
