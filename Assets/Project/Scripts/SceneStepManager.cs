// SceneStepManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class SceneStepManager : MonoBehaviour
{
    [Serializable]
    private sealed class SceneStep
    {
        [Tooltip("Main canvas for this step")]
        public GameObject canvas;

        public List<Button> proceedButtons = new();

        [Header("MCQ Options")]
        public bool isMCQ = false;
        public List<Button> mcqButtons;
        public Button correctMCQButton;
        [Tooltip("Result canvas for each button, same index as mcqButtons")]
        public List<GameObject> mcqResultCanvases;


        [Header("Teleport Options")]
        public bool teleportPlayer = false;
        public Transform teleportTarget;
        public bool returnToOrigin = false;
        public Transform canvasTarget;

        [Header("Audio Options")]
        public AudioClip stepAudio;
        [Range(0f, 1f)] public float audioVolume = 1f;

        [Header("Step Actions")]
        [Tooltip("Optional object(s) to enable when this step starts")]
        public List<GameObject> objectsToEnable = new();

        [Tooltip("Optional object(s) to disable when this step starts")]
        public List<GameObject> objectsToDisable = new(); // 🟡 NEW FEATURE

        [Tooltip("Optional button to proceed after interacting within this step")]
        public Button specialProceedButton;

        [Header("Ring Selection Step")]
        public bool isRingSelectionStep = false;
        public List<Button> ringButtons = new();

    }

    [Header("Player Rig")]
    [SerializeField] private Transform playerRig;
    [SerializeField] private Transform defaultPlayerPosition;

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Steps")]
    [SerializeField] private List<SceneStep> steps = new();

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName = "";

    private int _currentStepIndex = 0;

    private void Awake()
    {
        for (int i = 0; i < steps.Count; i++)
        {
            int capturedIndex = i;
            var step = steps[i];

            // Normal proceed buttons
            foreach (var btn in step.proceedButtons)
                if (btn != null) btn.onClick.AddListener(() => OnStepProceed(capturedIndex));

            // MCQ buttons
            if (step.isMCQ && step.mcqButtons != null)
            {
                foreach (var mcqBtn in step.mcqButtons)
                    if (mcqBtn != null) mcqBtn.onClick.AddListener(() => OnMCQButtonClicked(mcqBtn));
            }
            // Ring selection setup
            if (step.isRingSelectionStep && step.ringButtons != null)
            {
                foreach (var ringBtn in step.ringButtons)
                    if (ringBtn != null)
                        ringBtn.onClick.AddListener(() => OnRingClicked(capturedIndex, ringBtn));
            }

        }
    }

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        HideAll();

        if (steps.Count > 0)
            ShowStep(0);
    }

    private void HideAll()
    {
        foreach (var s in steps)
        {
            if (s.canvas != null)
                s.canvas.SetActive(false);

            if (s.mcqResultCanvases != null)
            {
                foreach (var r in s.mcqResultCanvases)
                    if (r != null) r.SetActive(false);
            }

            if (s.objectsToEnable != null)
            {
                foreach (var obj in s.objectsToEnable)
                    if (obj != null) obj.SetActive(false);
            }
        }
    }

    private void ShowStep(int index)
    {
        if (index < 0 || index >= steps.Count) return;

        HideAll();
        _currentStepIndex = index;
        var step = steps[index];

        // Teleport handling
        if (step.returnToOrigin && defaultPlayerPosition != null && playerRig != null)
        {
            playerRig.position = defaultPlayerPosition.position;
            playerRig.rotation = defaultPlayerPosition.rotation;
        }
        else if (step.teleportPlayer && step.teleportTarget != null && playerRig != null)
        {
            playerRig.position = step.teleportTarget.position;
            playerRig.rotation = step.teleportTarget.rotation;
        }

        // Move canvas
        if (step.canvas != null)
        {
            if (step.canvasTarget != null)
            {
                step.canvas.transform.position = step.canvasTarget.position;
                step.canvas.transform.rotation = step.canvasTarget.rotation;
            }
            step.canvas.SetActive(true);
        }

        // 🟢 Enable specified objects for this step
        if (step.objectsToEnable != null)
        {
            foreach (var go in step.objectsToEnable)
                if (go != null) go.SetActive(true);
        }

        // 🔴 Disable specified objects for this step (and keep them off)
        if (step.objectsToDisable != null)
        {
            foreach (var go in step.objectsToDisable)
                if (go != null) go.SetActive(false);
        }

        // Special proceed button
        if (step.specialProceedButton != null)
        {
            step.specialProceedButton.onClick.RemoveAllListeners();
            step.specialProceedButton.onClick.AddListener(() => OnStepProceed(_currentStepIndex));
        }

        // Play audio
        PlayStepAudio(step.stepAudio, step.audioVolume);
    }

    private void PlayStepAudio(AudioClip clip, float volume)
    {
        if (clip == null || audioSource == null) return;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    private void OnStepProceed(int fromIndex)
    {
        int nextIndex = fromIndex + 1;
        if (nextIndex < steps.Count)
            ShowStep(nextIndex);
        else if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    // === MCQ Logic ===
    private void OnMCQButtonClicked(Button clickedButton)
    {
        var step = steps[_currentStepIndex];
        if (!step.isMCQ || step.mcqButtons == null) return;

        bool isTransparent = clickedButton.CompareTag("TransparentButton") || clickedButton.name.Contains("Transparent");

        if (isTransparent)
        {
            if (clickedButton == step.correctMCQButton)
            {
                StartCoroutine(ShowResultCanvasAfterDelay(step, clickedButton, 0.5f));
            }
            else
            {
                clickedButton.gameObject.SetActive(false);
                if (step.mcqResultCanvases != null && step.mcqResultCanvases.Count > 0)
                {
                    var retryCanvas = step.mcqResultCanvases[0];
                    if (retryCanvas != null)
                    {
                        retryCanvas.transform.position = step.canvas.transform.position;
                        retryCanvas.transform.rotation = step.canvas.transform.rotation;
                        retryCanvas.SetActive(true);

                        foreach (var btn in retryCanvas.GetComponentsInChildren<Button>())
                        {
                            btn.onClick.RemoveAllListeners();
                            btn.onClick.AddListener(() =>
                            {
                                retryCanvas.SetActive(false);
                            });
                        }
                    }
                }
            }
            return;
        }

        foreach (var btn in step.mcqButtons)
        {
            if (btn == null) continue;
            btn.transition = Selectable.Transition.None;
        }

        foreach (var btn in step.mcqButtons)
        {
            if (btn == null) continue;
            Image img = btn.GetComponent<Image>() ?? btn.GetComponentInChildren<Image>();
            if (img == null) continue;
            if (btn.CompareTag("TransparentButton") || btn.name.Contains("Transparent")) continue;
            img.color = (btn == step.correctMCQButton) ? new Color32(0, 200, 0, 255) : new Color32(200, 0, 0, 255);
        }

        StartCoroutine(ShowResultCanvasAfterDelay(step, clickedButton, 0.5f));
    }

    private void OnRingClicked(int stepIndex, Button clickedButton)
{
    var step = steps[stepIndex];
    if (!step.isRingSelectionStep) return;

    // 🟡 Find the 3D ring mesh (child with MeshRenderer)
    MeshRenderer ringRenderer = clickedButton.GetComponentInChildren<MeshRenderer>(true);
    if (ringRenderer != null)
    {
        // Clone material instance so it doesn't affect others
        var mat = ringRenderer.material;
        mat.color = Color.green;
        ringRenderer.material = mat;
    }

    // Disable further clicks
    clickedButton.interactable = false;

    // 🟢 Check if all rings are clicked
    bool allClicked = true;
    foreach (var btn in step.ringButtons)
    {
        if (btn == null) continue;
        if (btn.interactable)
        {
            allClicked = false;
            break;
        }
    }

    // Proceed to next step once all are green
    if (allClicked)
    {
        OnStepProceed(stepIndex);
    }
}



    private System.Collections.IEnumerator ShowResultCanvasAfterDelay(SceneStep step, Button clickedButton, float delay)
    {
        yield return new WaitForSeconds(delay);

        bool isTransparent = clickedButton.CompareTag("TransparentButton") || clickedButton.name.Contains("Transparent");
        if (!isTransparent && step.canvas != null)
            step.canvas.SetActive(false);

        int buttonIndex = step.mcqButtons.IndexOf(clickedButton);
        if (buttonIndex >= 0 && step.mcqResultCanvases != null && buttonIndex < step.mcqResultCanvases.Count)
        {
            var resultCanvas = step.mcqResultCanvases[buttonIndex];
            if (resultCanvas != null)
            {
                resultCanvas.transform.position = step.canvas.transform.position;
                resultCanvas.transform.rotation = step.canvas.transform.rotation;
                resultCanvas.SetActive(true);

                foreach (var btn in resultCanvas.GetComponentsInChildren<Button>())
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnStepProceed(_currentStepIndex));
                }
            }
        }
    }
}

