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
using DG.Tweening; // Add this line for DOTween
using UnityEngine.SceneManagement; // Add this line for scene loading

public sealed class GameManagerModule4 : MonoBehaviour
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
            Debug.Log("[GameManager] Reached end of sequence.");
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
        else if (es.actionName == "HandBrake")
        {
            // Call the HandBrake method and pass references
            yield return StartCoroutine(HandBrakeEvent(es));
        }
        else if (es.actionName == "KeyEvent")
        {
            // Call the KeyEvent method and pass references
            yield return StartCoroutine(KeyEvent(es));
        }
        else if (es.actionName == "SeatBelt")
        {
            // Call the SeatBelt method and pass references
            yield return StartCoroutine(SeatBeltEvent(es));
        }
        else if (es.actionName == "3PointContact")
        {
            // Call the 3PointContact method and pass references
            yield return StartCoroutine(ThreePointContactEvent(es));
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
        else if (es.actionName == "ChokeRemove")
        {
            yield return StartCoroutine(ChokeRemoveEvent(es));
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


    private IEnumerator HandBrakeEvent(EventStep es)
    {
        if (es == null) yield break;

        // Ensure the event canvas is visible for the whole event
        if (es.eventCanvasRoot) es.eventCanvasRoot.SetActive(true);

        // Get references from the event
        var blinkObject1 = es.GetRef<GameObject>(0);
        var blinkObject2 = es.GetRef<GameObject>(1);

        // Handle button reference - could be Button component or GameObject with Button
        Button handBrakeButton = null;
        if (es.references.Count > 2)
        {
            if (es.references[2] is Button)
                handBrakeButton = es.references[2] as Button;
            else if (es.references[2] is GameObject buttonGO)
                handBrakeButton = buttonGO.GetComponent<Button>();
        }

        var rotationObject = es.GetRef<GameObject>(3);

        // Validate references
        if (!blinkObject1 || !blinkObject2)
        {
            Debug.LogWarning("[GameManager] HandBrake: Missing blink GameObjects (need two).");
            yield break;
        }

        if (!handBrakeButton)
        {
            Debug.LogWarning("[GameManager] HandBrake: Missing handbrake Button component. Make sure reference[2] is a Button component or GameObject with Button component.");
            yield break;
        }

        if (!rotationObject)
        {
            Debug.LogWarning("[GameManager] HandBrake: Missing rotation GameObject reference.");
            yield break;
        }

        // Flag to track when button is pressed
        bool buttonPressed = false;
        UnityEngine.Events.UnityAction buttonAction = () => { buttonPressed = true; };

        // Add button listener
        handBrakeButton.onClick.AddListener(buttonAction);

        // Initialize blinking - start with object1 enabled, object2 disabled
        blinkObject1.SetActive(true);
        blinkObject2.SetActive(false);

        // Blinking parameters
        float blinkInterval = 0.5f;
        float timer = 0f;
        bool showFirstObject = true;

        Debug.Log("[GameManager] HandBrake: Starting blinking effect. Waiting for button press.");

        // Blinking loop - continues until button is pressed
        while (!buttonPressed)
        {
            timer += Time.deltaTime;

            if (timer >= blinkInterval)
            {
                timer = 0f;
                showFirstObject = !showFirstObject;

                // Set the objects based on current state
                blinkObject1.SetActive(showFirstObject);
                blinkObject2.SetActive(!showFirstObject);

                Debug.Log($"[GameManager] HandBrake: Toggled - Object1: {showFirstObject}, Object2: {!showFirstObject}");
            }

            yield return null;
        }

        // Button was pressed - stop blinking and set both objects active
        blinkObject1.SetActive(false);
        blinkObject2.SetActive(false);
        handBrakeButton.gameObject.SetActive(false); // Optionally hide the button after press

        Debug.Log("[GameManager] HandBrake: Button pressed. Both objects activated. Starting rotation.");

        // Start rotation using DOTween (35 degrees on Z-axis)
        bool rotationComplete = false;
        rotationObject.transform.DORotate(
            new Vector3(0, 35f, 0),
            1f, // Duration in seconds
            RotateMode.LocalAxisAdd
        ).OnComplete(() =>
        {
            rotationComplete = true;
            Debug.Log("[GameManager] HandBrake: Rotation completed.");
        });

        // Wait for rotation to complete
        while (!rotationComplete)
        {
            yield return null;
        }

        // Clean up button listener
        handBrakeButton.onClick.RemoveListener(buttonAction);

        Debug.Log("[GameManager] HandBrake: Event completed successfully.");
    }
    
    private IEnumerator KeyEvent(EventStep es)
    {
        if (es == null) yield break;

        // Ensure the event canvas is visible for the whole event
        if (es.eventCanvasRoot) es.eventCanvasRoot.SetActive(true);

        // Get references from the event
        var blinkObject1 = es.GetRef<GameObject>(0);
        var blinkObject2 = es.GetRef<GameObject>(1);
        
        // Handle button reference - could be Button component or GameObject with Button
        Button keyButton = null;
        if (es.references.Count > 2)
        {
            if (es.references[2] is Button)
                keyButton = es.references[2] as Button;
            else if (es.references[2] is GameObject buttonGO)
                keyButton = buttonGO.GetComponent<Button>();
        }

        var removalObject = es.GetRef<GameObject>(3);

        // Validate references
        if (!blinkObject1 || !blinkObject2)
        {
            Debug.LogWarning("[GameManager] KeyEvent: Missing blink GameObjects (need two).");
            yield break;
        }

        if (!keyButton)
        {
            Debug.LogWarning("[GameManager] KeyEvent: Missing key Button component. Make sure reference[2] is a Button component or GameObject with Button component.");
            yield break;
        }

        if (!removalObject)
        {
            Debug.LogWarning("[GameManager] KeyEvent: Missing removal GameObject reference.");
            yield break;
        }

        // Flag to track when button is pressed
        bool buttonPressed = false;
        UnityEngine.Events.UnityAction buttonAction = () => { buttonPressed = true; };

        // Add button listener
        keyButton.onClick.AddListener(buttonAction);

        // Initialize blinking - start with object1 enabled, object2 disabled
        blinkObject1.SetActive(true);
        blinkObject2.SetActive(false);

        // Blinking parameters
        float blinkInterval = 0.5f;
        float timer = 0f;
        bool showFirstObject = true;

        Debug.Log("[GameManager] KeyEvent: Starting blinking effect. Waiting for button press.");

        // Blinking loop - continues until button is pressed
        while (!buttonPressed)
        {
            timer += Time.deltaTime;
            
            if (timer >= blinkInterval)
            {
                timer = 0f;
                showFirstObject = !showFirstObject;
                
                // Set the objects based on current state
                blinkObject1.SetActive(showFirstObject);
                blinkObject2.SetActive(!showFirstObject);
                
                Debug.Log($"[GameManager] KeyEvent: Toggled - Object1: {showFirstObject}, Object2: {!showFirstObject}");
            }
            
            yield return null;
        }

        // Button was pressed - stop blinking and set both objects inactive
        blinkObject1.SetActive(false);
        blinkObject2.SetActive(false);
        keyButton.gameObject.SetActive(false); // Optionally hide the button after press
        
        Debug.Log("[GameManager] KeyEvent: Button pressed. Both objects deactivated. Starting removal.");

        // Start removal using DOTween (move the gameobject in the Z axis)
        bool removalComplete = false;
        removalObject.transform.DOMoveZ(removalObject.transform.position.z + 0.2f, 2f)
        .OnComplete(() =>
        {
            removalComplete = true;
            Debug.Log("[GameManager] KeyEvent: Removal completed.");
        });

        // Wait for removal to complete
        while (!removalComplete)
        {
            yield return null;
        }

        // Clean up button listener
        keyButton.onClick.RemoveListener(buttonAction);

        Debug.Log("[GameManager] KeyEvent: Event completed successfully.");
    }

    private IEnumerator SeatBeltEvent(EventStep es)
    {
        if (es == null) yield break;

        // Ensure the event canvas is visible for the whole event
        if (es.eventCanvasRoot) es.eventCanvasRoot.SetActive(true);

        // Get references from the event
        var blinkObject1 = es.GetRef<GameObject>(0);
        var blinkObject2 = es.GetRef<GameObject>(1);
        
        // Handle button reference - could be Button component or GameObject with Button
        Button seatBeltButton = null;
        if (es.references.Count > 2)
        {
            if (es.references[2] is Button)
                seatBeltButton = es.references[2] as Button;
            else if (es.references[2] is GameObject buttonGO)
                seatBeltButton = buttonGO.GetComponent<Button>();
        }

        var objectToDisable = es.GetRef<GameObject>(3);
        var objectToEnable = es.GetRef<GameObject>(4);

        // Validate references
        if (!blinkObject1 || !blinkObject2)
        {
            Debug.LogWarning("[GameManager] SeatBelt: Missing blink GameObjects (need two).");
            yield break;
        }

        if (!seatBeltButton)
        {
            Debug.LogWarning("[GameManager] SeatBelt: Missing seatbelt Button component. Make sure reference[2] is a Button component or GameObject with Button component.");
            yield break;
        }

        if (!objectToDisable)
        {
            Debug.LogWarning("[GameManager] SeatBelt: Missing object to disable GameObject reference.");
            yield break;
        }

        if (!objectToEnable)
        {
            Debug.LogWarning("[GameManager] SeatBelt: Missing object to enable GameObject reference.");
            yield break;
        }

        // Flag to track when button is pressed
        bool buttonPressed = false;
        UnityEngine.Events.UnityAction buttonAction = () => { buttonPressed = true; };

        // Add button listener
        seatBeltButton.onClick.AddListener(buttonAction);

        // Initialize blinking - start with object1 enabled, object2 disabled
        blinkObject1.SetActive(true);
        blinkObject2.SetActive(false);

        // Blinking parameters
        float blinkInterval = 0.5f;
        float timer = 0f;
        bool showFirstObject = true;

        Debug.Log("[GameManager] SeatBelt: Starting blinking effect. Waiting for button press.");

        // Blinking loop - continues until button is pressed
        while (!buttonPressed)
        {
            timer += Time.deltaTime;
            
            if (timer >= blinkInterval)
            {
                timer = 0f;
                showFirstObject = !showFirstObject;
                
                // Set the objects based on current state
                blinkObject1.SetActive(showFirstObject);
                blinkObject2.SetActive(!showFirstObject);
                
                Debug.Log($"[GameManager] SeatBelt: Toggled - Object1: {showFirstObject}, Object2: {!showFirstObject}");
            }
            
            yield return null;
        }

        // Button was pressed - stop blinking and disable/enable objects
        blinkObject1.SetActive(false);
        blinkObject2.SetActive(false);
        seatBeltButton.gameObject.SetActive(false); // Optionally hide the button after press
        
        Debug.Log("[GameManager] SeatBelt: Button pressed. Disabling and enabling objects.");

        // Disable one object and enable another
        objectToDisable.SetActive(false);
        objectToEnable.SetActive(true);

        Debug.Log("[GameManager] SeatBelt: Objects switched successfully.");

        // Clean up button listener
        seatBeltButton.onClick.RemoveListener(buttonAction);

        Debug.Log("[GameManager] SeatBelt: Event completed successfully.");
    }

    private IEnumerator ThreePointContactEvent(EventStep es)
    {
        if (es == null) yield break;

        // Ensure the event canvas is visible for the whole event
        if (es.eventCanvasRoot) es.eventCanvasRoot.SetActive(true);

        // Get door reference
        var doorObject = es.GetRef<GameObject>(0);

        // Get button references
        Button[] buttons = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            int buttonRefIndex = 1 + (i * 4); // 1, 5, 9
            if (es.references[buttonRefIndex] is Button)
                buttons[i] = es.references[buttonRefIndex] as Button;
            else if (es.references[buttonRefIndex] is GameObject buttonGO)
                buttons[i] = buttonGO.GetComponent<Button>();
        }

        // Get blink objects and success objects for each button
        GameObject[] blinkObj1 = new GameObject[3];
        GameObject[] blinkObj2 = new GameObject[3];
        GameObject[] successObj = new GameObject[3];

        for (int i = 0; i < 3; i++)
        {
            int baseIndex = 2 + (i * 4); // 2, 6, 10
            blinkObj1[i] = es.GetRef<GameObject>(baseIndex);     // 2, 6, 10
            blinkObj2[i] = es.GetRef<GameObject>(baseIndex + 1); // 3, 7, 11
            successObj[i] = es.GetRef<GameObject>(baseIndex + 2); // 4, 8, 12
        }

        // Validate references
        if (!doorObject)
        {
            Debug.LogWarning("[GameManager] 3PointContact: Missing door GameObject reference.");
            yield break;
        }

        for (int i = 0; i < 3; i++)
        {
            if (!buttons[i] || !blinkObj1[i] || !blinkObj2[i] || !successObj[i])
            {
                Debug.LogWarning($"[GameManager] 3PointContact: Missing references for button {i + 1}.");
                yield break;
            }
        }

        // Initially hide all elements except door (keep door visible)
        doorObject.SetActive(true);
        for (int i = 0; i < 3; i++)
        {
            buttons[i].gameObject.SetActive(false);
            blinkObj1[i].SetActive(false);
            blinkObj2[i].SetActive(false);
            successObj[i].SetActive(false);
        }

        Debug.Log("[GameManager] 3PointContact: Starting door rotation.");

        // Step 1: Rotate door by -45 degrees on Y axis
        bool doorRotationComplete = false;
        doorObject.transform.DORotate(
            new Vector3(0, 45f, 0),
            2f, // Duration in seconds
            RotateMode.LocalAxisAdd
        ).OnComplete(() => {
            doorRotationComplete = true;
            Debug.Log("[GameManager] 3PointContact: Door rotation completed.");
        });

        // Wait for door rotation to complete
        while (!doorRotationComplete)
        {
            yield return null;
        }

        Debug.Log("[GameManager] 3PointContact: Starting button interaction phase.");

        // Step 2: Setup buttons and blinking (CHANGE 1: Show elements after door opens)
        bool[] buttonClicked = new bool[3];
        bool[] showFirstObject = new bool[3];
        float[] timers = new float[3];
        float blinkInterval = 0.5f;

        // Initialize all buttons and blink objects (show them after door rotation)
        for (int i = 0; i < 3; i++)
        {
            buttons[i].gameObject.SetActive(true);
            blinkObj1[i].SetActive(true);
            blinkObj2[i].SetActive(false);
            successObj[i].SetActive(false);
            showFirstObject[i] = true;
            
            // Add button listeners
            int buttonIndex = i; // Capture for closure
            buttons[i].onClick.AddListener(() => {
                if (!buttonClicked[buttonIndex])
                {
                    buttonClicked[buttonIndex] = true;
                    
                    // Stop blinking and show success object
                    blinkObj1[buttonIndex].SetActive(false);
                    blinkObj2[buttonIndex].SetActive(false);
                    //Wanna turn the button component to turn off so I should not be able to click it again
                    buttons[buttonIndex].interactable = false;
                    successObj[buttonIndex].SetActive(true);
                    
                    // CHANGE 2: Removed button color change to green
                    
                    Debug.Log($"[GameManager] 3PointContact: Button {buttonIndex + 1} clicked successfully.");
                }
            });
        }

        // Step 3: Blinking loop until all buttons are clicked
        while (!buttonClicked[0] || !buttonClicked[1] || !buttonClicked[2])
        {
            for (int i = 0; i < 3; i++)
            {
                if (!buttonClicked[i])
                {
                    timers[i] += Time.deltaTime;
                    
                    if (timers[i] >= blinkInterval)
                    {
                        timers[i] = 0f;
                        showFirstObject[i] = !showFirstObject[i];
                        
                        blinkObj1[i].SetActive(showFirstObject[i]);
                        blinkObj2[i].SetActive(!showFirstObject[i]);
                    }
                }
            }
            
            yield return null;
        }

        Debug.Log("[GameManager] 3PointContact: All buttons clicked. Event completing.");

        // Clean up button listeners
        for (int i = 0; i < 3; i++)
        {
            buttons[i].onClick.RemoveAllListeners();
        }

        Debug.Log("[GameManager] 3PointContact: Event completed successfully.");
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

    private IEnumerator ChokeRemoveEvent(EventStep es)
    {
        if (es == null) yield break;

        // Ensure the event canvas is visible for the whole event
        if (es.eventCanvasRoot) es.eventCanvasRoot.SetActive(true);

        // Get button references - expect them as GameObjects with Button components
        Button button1 = null;
        Button button2 = null;

        if (es.references.Count > 0)
        {
            if (es.references[0] is Button)
                button1 = es.references[0] as Button;
            else if (es.references[0] is GameObject go1)
                button1 = go1.GetComponent<Button>();
        }

        if (es.references.Count > 1)
        {
            if (es.references[1] is Button)
                button2 = es.references[1] as Button;
            else if (es.references[1] is GameObject go2)
                button2 = go2.GetComponent<Button>();
        }

        // Validate references
        if (!button1 || !button2)
        {
            Debug.LogWarning("[GameManager] ChokeRemove: Missing button references. Need two Button components or GameObjects with Button components.");
            yield break;
        }

        // Enable both buttons initially
        button1.gameObject.SetActive(true);
        button1.interactable = true;
        button2.gameObject.SetActive(true);
        button2.interactable = true;

        // Flags to track button clicks
        bool button1Clicked = false;
        bool button2Clicked = false;

        // Button click handlers
        UnityEngine.Events.UnityAction button1Action = () => {
            if (!button1Clicked)
            {
                button1Clicked = true;
                button1.interactable = false;
                button1.gameObject.SetActive(false);
                Debug.Log("[GameManager] ChokeRemove: Button 1 clicked and disabled.");
            }
        };

        UnityEngine.Events.UnityAction button2Action = () => {
            if (!button2Clicked)
            {
                button2Clicked = true;
                button2.interactable = false;
                button2.gameObject.SetActive(false);
                Debug.Log("[GameManager] ChokeRemove: Button 2 clicked and disabled.");
            }
        };

        // Add button listeners
        button1.onClick.AddListener(button1Action);
        button2.onClick.AddListener(button2Action);

        Debug.Log("[GameManager] ChokeRemove: Buttons enabled. Waiting for user to click both buttons.");

        // Wait until both buttons are clicked
        while (!button1Clicked || !button2Clicked)
        {
            yield return null;
        }

        // Clean up button listeners
        button1.onClick.RemoveListener(button1Action);
        button2.onClick.RemoveListener(button2Action);

        Debug.Log("[GameManager] ChokeRemove: Both buttons clicked. Event completed successfully.");
    }

}
