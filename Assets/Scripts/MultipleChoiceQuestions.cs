using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MultipleChoiceQuestions : MonoBehaviour
{
    public GameObject targetObject; // Object to enable
    public List<Button> uiButtons;  // List of UI Buttons

    private HashSet<Button> pressedButtons = new HashSet<Button>();

    void Start()
    {
        foreach (Button btn in uiButtons)
        {
            btn.onClick.AddListener(() => ButtonPressed(btn));
        }
    }

    void ButtonPressed(Button button)
    {
        pressedButtons.Add(button);

        // Check if all buttons have been pressed
        if (pressedButtons.Count == uiButtons.Count)
        {
            targetObject.SetActive(true);
        }
    }

    public void ResetButtons()
    {
        pressedButtons.Clear();
        targetObject.SetActive(false);
    }
}
