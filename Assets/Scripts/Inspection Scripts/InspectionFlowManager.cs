using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;

public class InspectionFlowManager : MonoBehaviour
{
    [System.Serializable]
    public class InspectionStep
    {
        [Header("Teleport")]
        public Transform teleportPoint;

        [Header("Inspection")]
        public MonoBehaviour inspectionScript; // Script with StartInspection()
        public Button checkButton;

        [Header("UI")]
        public GameObject stepUIRoot; // Panel or UI root inside Canvas
    }

    [Header("XR")]
    public Transform xrOrigin;

    [Header("Inspection Steps (Order Matters)")]
    public InspectionStep[] steps;

    [Header("Final UI")]
    public GameObject finalUIRoot;

    private int currentIndex = 0;

    private void Start()
    {
        DisableAllUI();
        MoveToStep(0);
    }

    private void MoveToStep(int index)
    {
        if (index >= steps.Length)
        {
            // Inspection completed
            DisableAllUI();
            if (finalUIRoot != null)
                finalUIRoot.SetActive(true);
            return;
        }

        currentIndex = index;
        DisableAllUI();

        InspectionStep step = steps[currentIndex];

        // Teleport XR Origin
        TeleportXR(step.teleportPoint);

        // Enable step UI
        if (step.stepUIRoot != null)
            step.stepUIRoot.SetActive(true);

        // Setup button
        if (step.checkButton != null)
        {
            step.checkButton.onClick.RemoveAllListeners();
            step.checkButton.interactable = true;
            step.checkButton.onClick.AddListener(() => OnCheckPressed(step));
        }
        else
        {
            // No inspection → auto advance (tire, cargo)
            StartCoroutine(AutoAdvance());
        }
    }

    private void OnCheckPressed(InspectionStep step)
    {
        step.checkButton.interactable = false;

        if (step.inspectionScript != null)
        {
            SubscribeToCompletion(step.inspectionScript);
            CallStartInspection(step.inspectionScript);
        }
        else
        {
            MoveToStep(currentIndex + 1);
        }
    }

    private void SubscribeToCompletion(MonoBehaviour inspection)
    {
        var eventField = inspection.GetType().GetField(
            "OnInspectionCompleted",
            BindingFlags.Public | BindingFlags.Instance
        );

        if (eventField != null)
        {
            System.Action callback = () =>
            {
                eventField.SetValue(inspection, null);
                MoveToStep(currentIndex + 1);
            };

            eventField.SetValue(inspection, callback);
        }
        else
        {
            Debug.LogError(
                $"{inspection.name} is missing public Action OnInspectionCompleted"
            );
        }
    }

    private void CallStartInspection(MonoBehaviour inspection)
    {
        MethodInfo method = inspection.GetType().GetMethod("StartInspection");
        if (method != null)
        {
            method.Invoke(inspection, null);
        }
        else
        {
            Debug.LogError(
                $"{inspection.name} is missing StartInspection()"
            );
        }
    }

    private void TeleportXR(Transform target)
    {
        if (xrOrigin == null || target == null) return;

        xrOrigin.position = target.position;
        xrOrigin.rotation = target.rotation;
    }

    private IEnumerator AutoAdvance()
    {
        yield return new WaitForSeconds(0.5f);
        MoveToStep(currentIndex + 1);
    }

    private void DisableAllUI()
    {
        foreach (var step in steps)
        {
            if (step.stepUIRoot != null)
                step.stepUIRoot.SetActive(false);

            if (step.checkButton != null)
                step.checkButton.interactable = true;
        }

        if (finalUIRoot != null)
            finalUIRoot.SetActive(false);
    }
}
