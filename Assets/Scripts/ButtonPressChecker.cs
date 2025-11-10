using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ButtonPressChecker : MonoBehaviour
{
    public Button[] buttons; // Assign buttons in the inspector
    private bool[] buttonStates;
    public UnityEvent onAllButtonsPressed; // Event triggered when all buttons are pressed

    void Start()
    {
        buttonStates = new bool[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Capture index for the lambda
            buttons[i].onClick.AddListener(() => ButtonPressed(index));
        }
    }

    void ButtonPressed(int index)
    {
        buttonStates[index] = true;
        if (AllButtonsPressed())
        {
            onAllButtonsPressed?.Invoke(); // Trigger event when all buttons are pressed
        }
    }

    bool AllButtonsPressed()
    {
        foreach (bool state in buttonStates)
        {
            if (!state) return false;
        }
        return true;
    }
}