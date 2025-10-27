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
        else if (es.actionName == "TruckReverseEvent")
        {
            // Call the TruckReverseEvent method and pass references
            yield return StartCoroutine(TruckReverseEvent(es.references));
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
            Debug.Log("[GameManager] Sequence complete.");
    }

    private IEnumerator MarshallSpawnEvent(List<UnityEngine.Object> refsList)
    {
        // Ensure we have at least one reference and it is a valid GameObject
        if (refsList.Count > 0)
        {
            GameObject person = refsList[0] as GameObject;
            if (person)
            {
                // Enable the GameObject (i.e., "spawn" it by enabling)
                person.SetActive(true);
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

    private IEnumerator TruckReverseEvent(List<UnityEngine.Object> refsList)
    {
        // Ensure we have the XR Rig and Truck GameObject references
        GameObject truck = refsList[0] as GameObject;
        XROrigin xrRig = refsList[1] as XROrigin;

        if (truck == null || xrRig == null)
        {
            Debug.LogWarning("[GameManager] TruckReverseEvent: Missing references.");
            yield break;
        }

        // Define the target position for the reverse
        float reverseDistance = 30f; // how much to move (in the Z axis)
        float speed = 3f; // speed at which both will reverse
        float targetTruckZ = truck.transform.position.z - reverseDistance;
        float targetXRZ = xrRig.transform.position.z - reverseDistance;

        // Move the truck and XR Rig simultaneously
        while (truck.transform.position.z > targetTruckZ && xrRig.transform.position.z > targetXRZ)
        {
            float step = speed * Time.deltaTime; // calculate the movement step based on time

            truck.transform.position = new Vector3(truck.transform.position.x, truck.transform.position.y, truck.transform.position.z - step);
            xrRig.transform.position = new Vector3(xrRig.transform.position.x, xrRig.transform.position.y, xrRig.transform.position.z - step);

            yield return null; // Wait for the next frame
        }

        // Ensure they both stop at the exact target
        truck.transform.position = new Vector3(truck.transform.position.x, truck.transform.position.y, targetTruckZ);
        xrRig.transform.position = new Vector3(xrRig.transform.position.x, xrRig.transform.position.y, targetXRZ);

        Debug.Log("[GameManager] TruckReverseEvent: Reversal complete.");
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
}
