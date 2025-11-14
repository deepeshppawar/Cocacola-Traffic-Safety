// GameManager.cs
// Unity 6000.1.12f1 (Unity 6) — OpenXR / XR Interaction Toolkit
// Sequential VR experience manager with teleport, canvases, and events/actions.
// UPDATE: Added MarshallSpawn and TruckReverseEvent to handle spawning a model and reversing the truck.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.XR.CoreUtils; // XROrigin
// using TMPro; // Not needed for this functionality
using UnityEngine.XR.Interaction.Toolkit; // at top of the file
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Add this line for XRBaseInteractable and XRSimpleInteractable
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.Animations; // Add this line for XRRayInteractor
using UnityEngine.SceneManagement; // Add this line for scene loading

public sealed class GameManager : MonoBehaviour
{
    // -----------------------------
    // Types
    // -----------------------------

    public enum StepType { Canvas, Event }

    [Serializable]
    public class CanvasStep
    {
        [Tooltip("Canvas root GameObject to show for this step.")]
        public GameObject canvasRoot;

        [Tooltip("Audio that should play while this canvas is active.")]
        public AudioClip audio;

        [Tooltip("Teleport anchor to move the player to when this canvas becomes active.")]
        public Transform teleportAnchor;

        [Header("Buttons (optional but recommended)")]
        public Button backButton;   // If assigned, will call GameManager.Back()
        public Button nextButton;   // If assigned, will call GameManager.Next()
    }

    [Serializable]
    public class EventStep
    {
        [Tooltip("Name of the action/event to execute. Must match a method you’ll implement later.")]
        public string actionName;

        [Tooltip("Optional canvas that is shown only while this event runs (e.g., tips, progress).")]
        public GameObject eventCanvasRoot;

        [Tooltip("Audio that should play while this event is active.")]
        public AudioClip audio;

        [Tooltip("Teleport anchor to move the player to when this event becomes active.")]
        public Transform teleportAnchor;

        [Header("Optional Buttons on the event canvas")]
        public Button backButton;   // If you want to allow going back during an event
        public Button nextButton;   // Usually not needed (auto-advances on completion)

        [Header("References needed by this event")]
        [Tooltip("Scalable list of references this event needs (GameObjects, Buttons, TMP fields, etc.). "
               + "Populate with anything the event/action must read/manipulate.")]
        public List<UnityEngine.Object> references = new List<UnityEngine.Object>();

        // (Optional) Helper getters you can use later in your event implementations:
        public T GetRef<T>(int index) where T : UnityEngine.Object
        {
            if (index < 0 || index >= references.Count) return null;
            return references[index] as T;
        }

        public bool TryGetRef<T>(int index, out T value) where T : UnityEngine.Object
        {
            value = null;
            if (index < 0 || index >= references.Count) return false;
            value = references[index] as T;
            return value != null;
        }
    }

    [Serializable]
    public class Step
    {
        public StepType type = StepType.Canvas;

        [Tooltip("A short label to make steps easy to read in the Inspector.")]
        public string label;

        [Tooltip("Canvas step data (used when Type = Canvas).")]
        public CanvasStep canvas;

        [Tooltip("Event step data (used when Type = Event).")]
        public EventStep ev;
    }

    // -----------------------------
    // Inspector
    // -----------------------------

    [Header("XR / Player Rig")]
    [Tooltip("Reference to your XR Origin (XR Interaction Toolkit).")]
    [SerializeField] private XROrigin xrOrigin;

    [Header("Audio")]
    [Tooltip("AudioSource used to play step audio. If not assigned, one will be created.")]
    [SerializeField] private AudioSource audioSource;

    [Header("Sequence")]
    [Tooltip("Add steps here in the exact order you want the experience to run.")]
    public List<Step> steps = new List<Step>();

    [Header("Start Options")]
    [Tooltip("Automatically start the sequence on Awake.")]
    public bool autoStart = true;

    [Header("Scene Loading")]
    [Tooltip("Name of the scene to load after completing all steps (leave empty to not load any scene).")]
    [SerializeField] private string nextSceneName = "";

    // -----------------------------
    // State
    // -----------------------------

    private int _index = -1; // current step index
    private Coroutine _runningEvent; // currently running event coroutine (if any)

    // -----------------------------
    // Unity
    // -----------------------------

    private void Awake()
    {
        if (!xrOrigin)
        {
            xrOrigin = FindFirstObjectByType<XROrigin>();
            if (!xrOrigin)
                Debug.LogError("[GameManager] XROrigin reference is missing.");
        }

        if (!audioSource)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f; // UI/voiceover style
        }

        // Ensure all canvases are hidden initially and wire buttons.
        HideAllCanvasesAndWireButtons();
    }

    private void Start()
    {
        if (autoStart && steps.Count > 0)
            GoTo(0);
    }

    private void OnDisable()
    {
        // Clean up listeners to avoid memory leaks if objects get reloaded
        UnwireAllButtons();
    }

    // -----------------------------
    // Public controls (you can hook these from UI or hotkeys)
    // -----------------------------

    public void Next()
    {
        if (_runningEvent != null)
            return; // Ignore manual Next while an event is running (it will auto-advance)

        int nextIdx = _index + 1;
        if (nextIdx < steps.Count)
            GoTo(nextIdx);
        else
        {
            Debug.Log("[GameManager] Reached end of sequence.");
            
            // Load next scene if specified
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log($"[GameManager] Loading next scene: {nextSceneName}");
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    public void Back()
    {
        if (_runningEvent != null)
        {
            // For MVP we’ll allow Back: stop event and go to previous.
            StopEventIfRunning();
        }

        int prevIdx = _index - 1;
        if (prevIdx >= 0)
            GoTo(prevIdx);
        else
            Debug.Log("[GameManager] Already at the beginning.");
    }

    // -----------------------------
    // Core flow
    // -----------------------------

    private void GoTo(int index)
    {
        if (index < 0 || index >= steps.Count)
        {
            Debug.LogError("[GameManager] GoTo out of range: " + index);
            return;
        }

        // Stop any currently running event
        StopEventIfRunning();

        // Hide everything and stop audio
        HideAllCanvases();
        StopAudio();

        _index = index;
        Step step = steps[_index];

        // Teleport (if provided)
        Transform anchor = GetTeleportAnchor(step);
        if (anchor) TeleportTo(anchor);

        // Audio (if provided)
        AudioClip clip = GetAudio(step);
        if (clip) PlayAudio(clip);

        // Activate relevant UI and start events
        if (step.type == StepType.Canvas)
        {
            if (step.canvas != null && step.canvas.canvasRoot)
                step.canvas.canvasRoot.SetActive(true);
        }
        else // Event
        {
            if (step.ev != null && step.ev.eventCanvasRoot)
                step.ev.eventCanvasRoot.SetActive(true);

            // Run event placeholder (or real action in future)
            _runningEvent = StartCoroutine(RunEvent(step.ev));
        }
    }

    private IEnumerator RunEvent(EventStep es)
    {
        if (es == null)
            yield break;

        Debug.Log($"[GameManager] EVENT START: {es.actionName}");

        // Handle specific events like MarshallSpawn and TruckReverseEvent:
        if (es.actionName == "MarshallSpawn")
        {
            // Call the MarshallSpawn method and pass references
            yield return StartCoroutine(MarshallSpawnEvent(es.references));
        }
        else if (es.actionName == "TruckReverse")
        {
            // Call the TruckReverse method and pass references
            yield return StartCoroutine(TruckReverseEvent(es.references));
        }
        else if (es.actionName == "BlinkHighlight")
        {
            // Example of a BlinkHighlight event with 0.5 second interval
            yield return StartCoroutine(BlinkHighlightEvent(es, 0.5f));
        }
        else if (es.actionName == "MarshallActivity")
        {
            // Call the MarshallActivity method and pass references
            yield return StartCoroutine(MarshallActivityEvent(es.references));
        }
        else if (es.actionName == "ClickButton")
        {
            // Call the ClickButton method and pass references
            yield return StartCoroutine(ClickButtonEvent(es.references));
        }
        else if (es.actionName == "DriverMCQ")
        {
            yield return StartCoroutine(DriverMCQEvent(es));
        }
        else
        {
            // Placeholder for other event actions
            yield return StartCoroutine(PlaceholderEventRoutine(es.actionName, es.references));
        }

        Debug.Log($"[GameManager] EVENT COMPLETE: {es.actionName}");
        _runningEvent = null;

        // Auto-advance when done
        int nextIdx = _index + 1;
        if (nextIdx < steps.Count)
            GoTo(nextIdx);
        else
        {
            Debug.Log("[GameManager] Sequence complete.");
            
            // Load next scene if specified
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log($"[GameManager] Loading next scene: {nextSceneName}");
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    private IEnumerator MarshallSpawnEvent(List<UnityEngine.Object> refsList)
    {
        // Ensure we have at least one reference and it is a valid GameObject
        if (refsList.Count > 0)
        {
            GameObject person = refsList[0] as GameObject;
            GameObject spot = refsList[1] as GameObject;
            if (person && spot)
            {
                // Enable the GameObject (i.e., "spawn" it by enabling)
                person.SetActive(true);
                spot.SetActive(true);
                Debug.Log("[GameManager] MarshallSpawn: GameObject enabled.");
            }
            else
            {
                Debug.LogWarning("[GameManager] MarshallSpawn: Invalid GameObject reference.");
            }
        }

        // Wait briefly before advancing to simulate completion
        yield return new WaitForSeconds(1f); // Placeholder duration
    }

    private IEnumerator ClickButtonEvent(List<UnityEngine.Object> refsList)
    {
        // Ensure we have at least one reference and it is a valid GameObject
        if (refsList.Count == 0 || !(refsList[0] is GameObject targetObject))
        {
            Debug.LogWarning("[GameManager] ClickButton: Missing or invalid GameObject reference.");
            yield break;
        }

        // Wait for user to click proceed button before concluding
        bool proceedPressed = false;
        UnityEngine.Events.UnityAction proceedAction = () => { proceedPressed = true; };

        // Get the current event step to access the nextButton
        EventStep currentEvent = steps[_index].ev;
        if (currentEvent != null && currentEvent.nextButton)
        {
            currentEvent.nextButton.onClick.AddListener(proceedAction);

            // Wait until proceed is pressed
            while (!proceedPressed)
                yield return null;

            // Clean up listener
            currentEvent.nextButton.onClick.RemoveListener(proceedAction);
        }
        else
        {
            Debug.LogWarning("[GameManager] ClickButton: No proceed button found, auto-advancing after 1 second.");
            yield return new WaitForSeconds(1f);
        }

        // Disable the GameObject
        if (targetObject != null)
        {
            targetObject.SetActive(false);
            Debug.Log("[GameManager] ClickButton: GameObject disabled and event completed.");
        }
    }

    private IEnumerator DriverMCQEvent(EventStep es)
    {
        if (es == null) yield break;

        // Ensure the event canvas is visible for the whole interaction
        if (es.eventCanvasRoot) es.eventCanvasRoot.SetActive(true);

        // Debug logging to check references
        Debug.Log($"[DriverMCQ] Total references count: {es.references.Count}");
        for (int i = 0; i < es.references.Count; i++)
        {
            Debug.Log($"[DriverMCQ] Reference[{i}]: {es.references[i]} (Type: {es.references[i]?.GetType().Name ?? "null"})");
        }

        // ---- Get references ----
        // Try to get Button components directly, or from GameObjects
        Button btnCorrect = null;
        Button btnWrong = null;

        // Check if reference[0] is a Button or GameObject with Button
        if (es.references.Count > 0)
        {
            if (es.references[0] is Button)
                btnCorrect = es.references[0] as Button;
            else if (es.references[0] is GameObject go0)
                btnCorrect = go0.GetComponent<Button>();
        }

        // Check if reference[1] is a Button or GameObject with Button
        if (es.references.Count > 1)
        {
            if (es.references[1] is Button)
                btnWrong = es.references[1] as Button;
            else if (es.references[1] is GameObject go1)
                btnWrong = go1.GetComponent<Button>();
        }

        var sfxCorrect = es.GetRef<AudioClip>(2);
        var sfxWrong   = es.GetRef<AudioClip>(3);

        if (!btnCorrect || !btnWrong)
        {
            Debug.LogWarning($"[DriverMCQ] btnCorrect is null: {btnCorrect == null}");
            Debug.LogWarning($"[DriverMCQ] btnWrong is null: {btnWrong == null}");
            yield break;
        }

        // Ensure both buttons are active and interactable
        btnCorrect.gameObject.SetActive(true);
        btnWrong.gameObject.SetActive(true);
        btnCorrect.interactable = true;
        btnWrong.interactable = true;

        // Get or add Image components for color changes
        var imgParentCorrect = btnCorrect.transform.parent.GetComponent<Image>();
        var imgCorrect = btnCorrect.GetComponent<Image>();
        var imgWrong   = btnWrong.GetComponent<Image>();

        if (!imgCorrect)
        {
            Debug.LogWarning("[DriverMCQ] Correct button missing Image component.");
            yield break;
        }
        if (!imgWrong)
        {
            Debug.LogWarning("[DriverMCQ] Wrong button missing Image component.");
            yield break;
        }

        // Target colors
        Color wrongColor, correctColor;
        ColorUtility.TryParseHtmlString("#F40009", out wrongColor);   // red for wrong
        ColorUtility.TryParseHtmlString("#00FF10", out correctColor); // green for correct

        // Flag to track if correct answer was selected
        bool answeredCorrect = false;

        // Button click handlers
        void OnCorrectClick()
        {
            if (answeredCorrect) return; // Prevent multiple clicks

            imgCorrect.color = correctColor;
            imgParentCorrect.color = correctColor;
            if (audioSource && sfxCorrect) 
                audioSource.PlayOneShot(sfxCorrect);
            
            answeredCorrect = true;
            
            // Disable both buttons to prevent further clicking
            btnCorrect.interactable = false;
            btnWrong.interactable = false;
        }

        void OnWrongClick()
        {
            if (answeredCorrect) return; // Don't process if already answered correctly
            
            imgWrong.color = wrongColor;
            if (audioSource && sfxWrong) 
                audioSource.PlayOneShot(sfxWrong);
            
            // Keep the button red but still allow clicking correct answer
            btnWrong.interactable = false; // Disable wrong button so it can't be clicked again
        }

        // Add listeners
        btnCorrect.onClick.AddListener(OnCorrectClick);
        btnWrong.onClick.AddListener(OnWrongClick);

        // Wait until correct answer is selected
        while (!answeredCorrect)
            yield return null;

        // Wait 2 seconds after correct answer before concluding
        yield return new WaitForSeconds(2f);

        // Clean up listeners
        btnCorrect.onClick.RemoveListener(OnCorrectClick);
        btnWrong.onClick.RemoveListener(OnWrongClick);

        // Canvas stays enabled as it will be handled by the GameManager
        Debug.Log("[DriverMCQ] Correct answer selected; event complete.");
    }

    
    private IEnumerator MarshallActivityEvent(List<UnityEngine.Object> refsList)
    {
        // Ensure we have at least one reference and it is a valid GameObject
        if (refsList.Count > 0)
        {
            GameObject person = refsList[0] as GameObject;
            GameObject pedestrian = refsList[1] as GameObject;
            if (person && pedestrian)
            {
                // Disable the GameObject (i.e., "despawn" it by disabling)
                person.SetActive(false);
                pedestrian.SetActive(true);
                Debug.Log("[GameManager] MarshallActivity: GameObject disabled.");
            }
            else
            {
                Debug.LogWarning("[GameManager] MarshallActivity: Invalid GameObject reference.");
            }
        }

        // Wait for user to click proceed button before concluding
        bool proceedPressed = false;
        UnityEngine.Events.UnityAction proceedAction = () => { proceedPressed = true; };

        // Get the current event step to access the nextButton
        EventStep currentEvent = steps[_index].ev;
        if (currentEvent != null && currentEvent.nextButton)
        {
            currentEvent.nextButton.onClick.AddListener(proceedAction);

            // Wait until proceed is pressed
            while (!proceedPressed)
                yield return null;

            // Clean up listener
            currentEvent.nextButton.onClick.RemoveListener(proceedAction);
        }
        else
        {
            Debug.LogWarning("[GameManager] MarshallActivity: No proceed button found, auto-advancing after 1 second.");
            yield return new WaitForSeconds(1f);
        }
    }


    private IEnumerator BlinkHighlightEvent(EventStep es, float intervalSeconds = 0.5f)
    {
        if (es == null)
            yield break;

        // --- Read references ---
        var goA = es.GetRef<GameObject>(0);
        var goB = es.GetRef<GameObject>(1);

        if (!goA || !goB)
        {
            Debug.LogWarning("[GameManager] BlinkHighlight: Missing GameObject references (need two).");
            yield break;
        }

        // Ensure the event canvas (if any) is visible during the event
        if (es.eventCanvasRoot) es.eventCanvasRoot.SetActive(true);

        // Save original states so we can restore them on exit
        bool aInitial = goA.activeSelf;
        bool bInitial = goB.activeSelf;

        // Button wiring: we use a local flag to exit the loop when proceed is pressed
        bool proceedPressed = false;
        UnityEngine.Events.UnityAction proceedAction = () => { proceedPressed = true; };

        if (es.nextButton)
            es.nextButton.onClick.AddListener(proceedAction);
        else
            Debug.LogWarning("[GameManager] BlinkHighlight: No Next/Proceed button assigned on EventStep.");

        // Optional: because the GameManager already wires event.nextButton -> Next(),
        // pressing it will also call GameManager.Next(). That's OK, because our Next()
        // ignores requests while an event is running. We still need our own listener
        // above to flip 'proceedPressed' so this coroutine can end cleanly.

        // Initialize one on / one off
        bool showA = true;
        goA.SetActive(showA);
        goB.SetActive(!showA);

        var wait = new WaitForSeconds(intervalSeconds);

        // --- Flicker until Proceed pressed ---
        while (!proceedPressed)
        {
            showA = !showA;
            goA.SetActive(showA);
            goB.SetActive(!showA);
            yield return wait;
        }

        // Cleanup: restore original states and remove our listener
        goA.SetActive(aInitial);
        goB.SetActive(bInitial);

        if (es.nextButton)
            es.nextButton.onClick.RemoveListener(proceedAction);

        // Done: coroutine returns -> GameManager will auto-advance to the next step
    }


    private IEnumerator PlaceholderEventRoutine(string actionName, List<UnityEngine.Object> refsList)
    {
        // Minimal placeholder: wait to mimic user performing the action.
        yield return new WaitForSeconds(2f);
    }

    private void StopEventIfRunning()
    {
        if (_runningEvent != null)
        {
            StopCoroutine(_runningEvent);
            _runningEvent = null;
        }
    }

    // -----------------------------
    // Helpers: teleport, audio, canvases
    // -----------------------------

    private void TeleportTo(Transform anchor)
    {
        if (!xrOrigin || !anchor) return;

        // Move XR Origin so the camera ends up at the anchor pose.
        xrOrigin.MoveCameraToWorldLocation(anchor.position);
        xrOrigin.MatchOriginUpCameraForward(anchor.up, anchor.forward);
    }

    private AudioClip GetAudio(Step step)
    {
        if (step == null) return null;
        if (step.type == StepType.Canvas && step.canvas != null)
            return step.canvas.audio;
        if (step.type == StepType.Event && step.ev != null)
            return step.ev.audio;
        return null;
    }

    private Transform GetTeleportAnchor(Step step)
    {
        if (step == null) return null;
        if (step.type == StepType.Canvas && step.canvas != null)
            return step.canvas.teleportAnchor;
        if (step.type == StepType.Event && step.ev != null)
            return step.ev.teleportAnchor;
        return null;
    }

    private void PlayAudio(AudioClip clip)
    {
        if (!audioSource || !clip) return;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void StopAudio()
    {
        if (audioSource && audioSource.isPlaying)
            audioSource.Stop();
        if (audioSource) audioSource.clip = null;
    }

    private void HideAllCanvasesAndWireButtons()
    {
        foreach (var s in steps)
        {
            if (s.type == StepType.Canvas && s.canvas != null)
            {
                if (s.canvas.canvasRoot) s.canvas.canvasRoot.SetActive(false);
                WireButton(s.canvas.backButton, Back);
                WireButton(s.canvas.nextButton, Next);
            }
            else if (s.type == StepType.Event && s.ev != null)
            {
                if (s.ev.eventCanvasRoot) s.ev.eventCanvasRoot.SetActive(false);
                WireButton(s.ev.backButton, Back);
                WireButton(s.ev.nextButton, Next); // optional; usually not needed
            }
        }
    }

    private void HideAllCanvases()
    {
        foreach (var s in steps)
        {
            if (s.type == StepType.Canvas && s.canvas != null && s.canvas.canvasRoot)
                s.canvas.canvasRoot.SetActive(false);
            else if (s.type == StepType.Event && s.ev != null && s.ev.eventCanvasRoot)
                s.ev.eventCanvasRoot.SetActive(false);
        }
    }

    private void WireButton(Button btn, Action action)
    {
        if (!btn) return;
        btn.onClick.RemoveListener(() => { }); // ensure clean slate
        btn.onClick.AddListener(() => action());
    }

    private void UnwireAllButtons()
    {
        foreach (var s in steps)
        {
            if (s.type == StepType.Canvas && s.canvas != null)
            {
                if (s.canvas.backButton) s.canvas.backButton.onClick.RemoveAllListeners();
                if (s.canvas.nextButton) s.canvas.nextButton.onClick.RemoveAllListeners();
            }
            else if (s.type == StepType.Event && s.ev != null)
            {
                if (s.ev.backButton) s.ev.backButton.onClick.RemoveAllListeners();
                if (s.ev.nextButton) s.ev.nextButton.onClick.RemoveAllListeners();
            }
        }
    }

    private IEnumerator TruckReverseEvent(List<UnityEngine.Object> refsList)
    {
        // Ensure we have the TruckReverseController GameObject reference
        if (refsList.Count == 0 || !(refsList[0] is GameObject truckControllerGO))
        {
            Debug.LogWarning("[GameManager] TruckReverse: Missing or invalid TruckReverseController GameObject reference.");
            yield break;
        }

        // Get the TruckReverseController component
        TruckReverseController truckController = truckControllerGO.GetComponent<TruckReverseController>();
        if (truckController == null)
        {
            Debug.LogWarning("[GameManager] TruckReverse: TruckReverseController component not found on GameObject.");
            yield break;
        }

        // Enable the GameObject to activate the TruckReverseController
        truckControllerGO.SetActive(true);
        Debug.Log("[GameManager] TruckReverse: TruckReverseController enabled and started.");

        // Monitor the controller until maxReverseDistance is reached
        // We'll use reflection to access the private reversedThisHold field
        System.Reflection.FieldInfo reversedField = typeof(TruckReverseController)
            .GetField("reversedThisHold", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        System.Reflection.FieldInfo maxDistanceField = typeof(TruckReverseController)
            .GetField("maxReverseDistance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (reversedField == null || maxDistanceField == null)
        {
            Debug.LogWarning("[GameManager] TruckReverse: Could not access TruckReverseController fields via reflection.");
            yield break;
        }

        // Wait until the truck has reached its maximum reverse distance
        while (truckControllerGO.activeInHierarchy)
        {
            float reversedDistance = (float)reversedField.GetValue(truckController);
            float maxDistance = (float)maxDistanceField.GetValue(truckController);

            // Check if max distance is reached
            if (reversedDistance >= maxDistance)
            {
                Debug.Log("[GameManager] TruckReverse: Maximum reverse distance reached. Event complete.");
                break;
            }

            yield return null; // Wait one frame
        }

        // Optional: Keep the controller active or disable it based on your needs
        // truckControllerGO.SetActive(false); // Uncomment if you want to disable after completion

        Debug.Log("[GameManager] TruckReverse: Event completed successfully.");
    }

}
