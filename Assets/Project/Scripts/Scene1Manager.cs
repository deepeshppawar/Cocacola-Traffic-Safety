// Scene1Manager.cs
// Single-script controller for Scene 1 VR flow (Interactions 1–7).
// Attach to an empty GameObject and assign all references in the Inspector.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class Scene1Manager : MonoBehaviour
{
    [Header("Player / Teleport")]
    [SerializeField] private Transform playerRig;                 // XR Rig root (move/teleport this)
    [SerializeField] private Transform teleportTarget;            // Empty placeholder Transform to teleport to (after module intro)

    [Header("Global (Optional)")]
    [SerializeField] private CanvasGroup blocker;                 // Optional fade/input blocker (can be null)
    [SerializeField] private float fadeDuration = 0.35f;

    // -----------------------------
    // Start -> Welcome -> Module Intro
    // -----------------------------
    [Header("Start / Welcome  (Scene 1: Facility Gate Arrival)")]
    [SerializeField] private GameObject welcomeCanvas;
    [SerializeField] private Button welcomeProceedButton;

    [Header("Module 1 Intro")]
    [SerializeField] private GameObject moduleIntroCanvas;
    [SerializeField] private Button moduleIntroProceedButton;

    // -----------------------------
    // Interaction 1: Greeting / Procedure Explanation (two info canvases)
    // Scene Mapping: User listens & acknowledges entry briefing
    // -----------------------------
    [Header("Interaction 1 - Greeting / Briefing")]
    [SerializeField] private GameObject i1GreetingCanvasA;        // first info canvas
    [SerializeField] private Button i1GreetingProceedAButton;
    [SerializeField] private GameObject i1GreetingCanvasB;        // second, same style info canvas
    [SerializeField] private Button i1GreetingProceedBButton;

    // -----------------------------
    // Interaction 2: Document request -> place docs -> submit complete -> awareness
    // Scene Mapping: Document Submission
    // -----------------------------
    [Header("Interaction 2 - Documents")]
    [SerializeField] private GameObject i2RequestCanvas;
    [SerializeField] private Button i2SubmitButton;
    [SerializeField] private List<GameObject> i2DocsToActivateOnTable = new List<GameObject>(); // initially disabled in scene
    [SerializeField] private GameObject i2SubmitCompleteCanvas;
    [SerializeField] private Button i2SubmitCompleteProceedButton; // optional; leads to awareness
    [SerializeField] private GameObject i2AwarenessCanvas;
    [SerializeField] private Button i2AwarenessProceedButton;

    // -----------------------------
    // Interaction 3: Alcohol analyzer test
    // Scene Mapping: Alcohol Analyzer Test
    // -----------------------------
    [Header("Interaction 3 - Alcohol Analyzer")]
    [SerializeField] private GameObject i3IntroCanvas;            // "Take alcohol analyser test"
    [SerializeField] private Button i3IntroProceedButton;

    [SerializeField] private GameObject i3DuringTestCanvas;       // "Take test" canvas
    [SerializeField] private Button i3DuringTestProceedButton;

    [SerializeField] private GameObject i3SuccessCanvas;          // "Complete successful" canvas
    [SerializeField] private Button i3SuccessProceedButton;

    [SerializeField] private GameObject i3ToggleOnObject;         // set active TRUE at start of test, then FALSE at success
    [SerializeField] private GameObject i3ToggleOffObject;        // set active FALSE at start of test, then TRUE at success

    // -----------------------------
    // Interaction 4: Click listed gameobjects via adjacent UI buttons (all must be clicked)
    // Scene Mapping: PPE Collection (helmet, jacket, shoes)
    // -----------------------------
    [Serializable]
    private sealed class TargetClickEntry
    {
        public GameObject targetObject;      // will be SetActive(false) upon its button click
        public Button clickButton;           // assign the adjacent UI button
        [HideInInspector] public bool clicked;
    }

    [Header("Interaction 4 - Click Targets (PPE)")]
    [SerializeField] private GameObject i4InfoCanvas;             // tells user to click all
    [SerializeField] private List<TargetClickEntry> i4Targets = new List<TargetClickEntry>();
    [SerializeField] private GameObject i4CompleteCanvas;         // shows "now we will collect wheel chokes"
    [SerializeField] private Button i4CompleteProceedButton;

    // -----------------------------
    // Interaction 5: Wheel Choke Pickup
    // Explicit: when Interaction 5 starts -> SetActive(true), on Collect -> SetActive(false)
    // -----------------------------
    [Header("Interaction 5 - Wheel Choke Pickup")]
    [SerializeField] private GameObject i5InfoCanvas;             // simple instruction
    [SerializeField] private GameObject i5TargetObject;           // wheel choke GO (TRUE at start, FALSE on collect)
    [SerializeField] private Button i5CollectButton;              // the collect button
    [SerializeField] private GameObject i5CompleteCanvas;         // completion
    [SerializeField] private Button i5CompleteProceedButton;

    // -----------------------------
    // Interaction 6 (Single-script version: intro -> completion)
    // -----------------------------
    [Header("Interaction 6 - Intro & Completion")]
    [SerializeField] private GameObject i6IntroCanvas;
    [SerializeField] private Button i6IntroProceedButton;
    [SerializeField] private GameObject i6CompletionCanvas;
    [SerializeField] private Button i6CompletionProceedButton;

    // -----------------------------
    // Interaction 7 (Single-script version: intro -> completion -> Load Scene)
    // -----------------------------
    [Header("Interaction 7 - Intro, Completion & Load Next Scene")]
    [SerializeField] private GameObject i7IntroCanvas;
    [SerializeField] private Button i7IntroProceedButton;
    [SerializeField] private GameObject i7CompletionCanvas;
    [SerializeField] private Button i7CompletionProceedButton;
    [SerializeField] private string nextSceneName = "Scene2";      // set exact scene name

    // Runtime
    private bool _isFading;

    private void Awake()
    {
        // Wire all button listeners
        if (welcomeProceedButton)               welcomeProceedButton.onClick.AddListener(OnWelcomeProceed);
        if (moduleIntroProceedButton)          moduleIntroProceedButton.onClick.AddListener(OnModuleIntroProceed);

        if (i1GreetingProceedAButton)          i1GreetingProceedAButton.onClick.AddListener(OnI1GreetingAProceed);
        if (i1GreetingProceedBButton)          i1GreetingProceedBButton.onClick.AddListener(OnI1GreetingBProceed);

        if (i2SubmitButton)                    i2SubmitButton.onClick.AddListener(OnI2SubmitDocuments);
        if (i2SubmitCompleteProceedButton)     i2SubmitCompleteProceedButton.onClick.AddListener(ShowI2Awareness);
        if (i2AwarenessProceedButton)          i2AwarenessProceedButton.onClick.AddListener(OnI2AwarenessProceed);

        if (i3IntroProceedButton)              i3IntroProceedButton.onClick.AddListener(OnI3IntroProceed);
        if (i3DuringTestProceedButton)         i3DuringTestProceedButton.onClick.AddListener(OnI3DuringTestProceed);
        if (i3SuccessProceedButton)            i3SuccessProceedButton.onClick.AddListener(OnI3SuccessProceed);

        if (i4CompleteProceedButton)           i4CompleteProceedButton.onClick.AddListener(StartInteraction5);
        if (i5CollectButton)                   i5CollectButton.onClick.AddListener(OnI5Collect);
        if (i5CompleteProceedButton)           i5CompleteProceedButton.onClick.AddListener(StartInteraction6);

        // i4 dynamic buttons
        foreach (var entry in i4Targets)
        {
            if (entry?.clickButton == null) continue;
            entry.clicked = false;
            entry.clickButton.onClick.AddListener(() => OnI4ClickEntry(entry));
        }

        // Interaction 6 and 7 buttons
        if (i6IntroProceedButton)              i6IntroProceedButton.onClick.AddListener(OnI6IntroProceed);
        if (i6CompletionProceedButton)         i6CompletionProceedButton.onClick.AddListener(StartInteraction7);

        if (i7IntroProceedButton)              i7IntroProceedButton.onClick.AddListener(OnI7IntroProceed);
        if (i7CompletionProceedButton)         i7CompletionProceedButton.onClick.AddListener(LoadNextScene);
    }

    private void OnDestroy()
    {
        // Clean up to avoid memory leaks
        if (welcomeProceedButton)               welcomeProceedButton.onClick.RemoveListener(OnWelcomeProceed);
        if (moduleIntroProceedButton)          moduleIntroProceedButton.onClick.RemoveListener(OnModuleIntroProceed);

        if (i1GreetingProceedAButton)          i1GreetingProceedAButton.onClick.RemoveListener(OnI1GreetingAProceed);
        if (i1GreetingProceedBButton)          i1GreetingProceedBButton.onClick.RemoveListener(OnI1GreetingBProceed);

        if (i2SubmitButton)                    i2SubmitButton.onClick.RemoveListener(OnI2SubmitDocuments);
        if (i2SubmitCompleteProceedButton)     i2SubmitCompleteProceedButton.onClick.RemoveListener(ShowI2Awareness);
        if (i2AwarenessProceedButton)          i2AwarenessProceedButton.onClick.RemoveListener(OnI2AwarenessProceed);

        if (i3IntroProceedButton)              i3IntroProceedButton.onClick.RemoveListener(OnI3IntroProceed);
        if (i3DuringTestProceedButton)         i3DuringTestProceedButton.onClick.RemoveListener(OnI3DuringTestProceed);
        if (i3SuccessProceedButton)            i3SuccessProceedButton.onClick.RemoveListener(OnI3SuccessProceed);

        if (i4CompleteProceedButton)           i4CompleteProceedButton.onClick.RemoveListener(StartInteraction5);
        if (i5CollectButton)                   i5CollectButton.onClick.RemoveListener(OnI5Collect);
        if (i5CompleteProceedButton)           i5CompleteProceedButton.onClick.RemoveListener(StartInteraction6);

        foreach (var entry in i4Targets)
        {
            if (entry?.clickButton == null) continue;
            entry.clickButton.onClick.RemoveAllListeners();
        }

        if (i6IntroProceedButton)              i6IntroProceedButton.onClick.RemoveListener(OnI6IntroProceed);
        if (i6CompletionProceedButton)         i6CompletionProceedButton.onClick.RemoveListener(StartInteraction7);

        if (i7IntroProceedButton)              i7IntroProceedButton.onClick.RemoveListener(OnI7IntroProceed);
        if (i7CompletionProceedButton)         i7CompletionProceedButton.onClick.RemoveListener(LoadNextScene);
    }

    private void Start()
    {
        // Initial visibility
        HideAll();

        // Show Welcome at start
        SetActive(welcomeCanvas, true);
    }

    // =========================================================
    // Flow: Welcome -> Module Intro -> Teleport -> Interaction 1
    // =========================================================
    private void OnWelcomeProceed()
    {
        HideAll();
        SetActive(moduleIntroCanvas, true);
    }

    private void OnModuleIntroProceed()
    {
        // Teleport player to target
        TeleportPlayerToTarget();

        // Start Interaction 1
        StartInteraction1();
    }

    private void TeleportPlayerToTarget()
    {
        if (playerRig == null || teleportTarget == null)
        {
            Debug.LogWarning("[Scene1Manager] Teleport skipped, missing references.");
            return;
        }
        playerRig.position = teleportTarget.position;
        playerRig.rotation = teleportTarget.rotation;
    }

    // =========================================================
    // Interaction 1: Greeting (two info canvases in sequence)
    // =========================================================
    private void StartInteraction1()
    {
        HideAll();
        SetActive(i1GreetingCanvasA, true);
    }

    private void OnI1GreetingAProceed()
    {
        HideAll();
        SetActive(i1GreetingCanvasB, true);
    }

    private void OnI1GreetingBProceed()
    {
        StartInteraction2();
    }

    // =========================================================
    // Interaction 2: Document request -> place docs -> submit complete -> awareness
    // =========================================================
    private void StartInteraction2()
    {
        HideAll();
        SetActive(i2RequestCanvas, true);
    }

    private void OnI2SubmitDocuments()
    {
        // Activate docs on table
        foreach (var doc in i2DocsToActivateOnTable)
        {
            if (doc) doc.SetActive(true);
        }

        // Show submit complete
        HideAll();
        SetActive(i2SubmitCompleteCanvas, true);
        // If you want to skip the extra click, call ShowI2Awareness() here instead or Invoke after delay.
    }

    private void ShowI2Awareness()
    {
        HideAll();
        SetActive(i2AwarenessCanvas, true);
    }

    private void OnI2AwarenessProceed()
    {
        StartInteraction3();
    }

    // =========================================================
    // Interaction 3: Alcohol analyzer test
    // =========================================================
    private void StartInteraction3()
    {
        HideAll();

        // Ensure initial toggle state before starting test
        if (i3ToggleOnObject)  i3ToggleOnObject.SetActive(false);
        if (i3ToggleOffObject) i3ToggleOffObject.SetActive(true);

        SetActive(i3IntroCanvas, true);
    }

    private void OnI3IntroProceed()
    {
        // Start test: one GO ON, one GO OFF, and show "Take test" canvas
        if (i3ToggleOnObject)  i3ToggleOnObject.SetActive(true);
        if (i3ToggleOffObject) i3ToggleOffObject.SetActive(false);

        HideAll();
        SetActive(i3DuringTestCanvas, true);
    }

    private void OnI3DuringTestProceed()
    {
        // Success: swap back the states, show success canvas
        if (i3ToggleOnObject)  i3ToggleOnObject.SetActive(false);
        if (i3ToggleOffObject) i3ToggleOffObject.SetActive(true);

        HideAll();
        SetActive(i3SuccessCanvas, true);
    }

    private void OnI3SuccessProceed()
    {
        StartInteraction4();
    }

    // =========================================================
    // Interaction 4: Click listed objects via adjacent buttons (all must be clicked)
    // =========================================================
    private void StartInteraction4()
    {
        HideAll();
        foreach (var entry in i4Targets)
        {
            if (entry == null) continue;
            entry.clicked = false;
            if (entry.targetObject) entry.targetObject.SetActive(true); // ensure visible at start
        }

        SetActive(i4InfoCanvas, true);
    }

    private void OnI4ClickEntry(TargetClickEntry entry)
    {
        if (entry == null) return;
        if (entry.clicked) return;

        entry.clicked = true;
        if (entry.targetObject) entry.targetObject.SetActive(false);

        // Check if all done
        foreach (var e in i4Targets)
        {
            if (e != null && !e.clicked)
            {
                return; // not finished yet
            }
        }

        // All clicked
        HideAll();
        SetActive(i4CompleteCanvas, true); // informs "now we will collect wheel chokes"
    }

    // =========================================================
    // Interaction 5: Wheel Choke Pickup (explicit behavior requested)
    // - Start: i5TargetObject.SetActive(true)
    // - Collect button: i5TargetObject.SetActive(false)
    // =========================================================
    private void StartInteraction5()
    {
        HideAll();

        // EXPLICIT per request: ensure choke object is visible/active at start
        if (i5TargetObject) i5TargetObject.SetActive(true);

        SetActive(i5InfoCanvas, true);
    }

    private void OnI5Collect()
    {
        // EXPLICIT per request: turn wheel choke off on collect
        if (i5TargetObject) i5TargetObject.SetActive(false);

        HideAll();
        SetActive(i5CompleteCanvas, true);
    }

    // =========================================================
    // Interaction 6: Single-script (Intro -> Completion -> Proceed)
    // =========================================================
    private void StartInteraction6()
    {
        HideAll();
        SetActive(i6IntroCanvas, true);
    }

    private void OnI6IntroProceed()
    {
        HideAll();
        SetActive(i6CompletionCanvas, true);
    }

    // Called by i6CompletionProceedButton
    private void StartInteraction7()
    {
        HideAll();
        SetActive(i7IntroCanvas, true);
    }

    // =========================================================
    // Interaction 7: Single-script (Intro -> Completion -> Load Scene)
    // =========================================================
    private void OnI7IntroProceed()
    {
        HideAll();
        SetActive(i7CompletionCanvas, true);
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("[Scene1Manager] nextSceneName is empty.");
            return;
        }
        SceneManager.LoadScene(nextSceneName);
    }

    // =========================================================
    // Helpers
    // =========================================================
    private void HideAll()
    {
        // Turn off every canvas we manage
        SetActive(welcomeCanvas, false);
        SetActive(moduleIntroCanvas, false);

        SetActive(i1GreetingCanvasA, false);
        SetActive(i1GreetingCanvasB, false);

        SetActive(i2RequestCanvas, false);
        SetActive(i2SubmitCompleteCanvas, false);
        SetActive(i2AwarenessCanvas, false);

        SetActive(i3IntroCanvas, false);
        SetActive(i3DuringTestCanvas, false);
        SetActive(i3SuccessCanvas, false);

        SetActive(i4InfoCanvas, false);
        SetActive(i4CompleteCanvas, false);

        SetActive(i5InfoCanvas, false);
        SetActive(i5CompleteCanvas, false);

        SetActive(i6IntroCanvas, false);
        SetActive(i6CompletionCanvas, false);

        SetActive(i7IntroCanvas, false);
        SetActive(i7CompletionCanvas, false);
    }

    private void SetActive(GameObject go, bool active)
    {
        if (go && go.activeSelf != active) go.SetActive(active);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (fadeDuration < 0f) fadeDuration = 0f;
    }
#endif
}
