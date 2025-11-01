// TruckReverseController.cs
// Unity 6000.1.12f1 – Meta Quest (OpenXR + XRI)
// Reverse Truck + XR Origin together when pulling stick back.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using XRCommonUsages = UnityEngine.XR.CommonUsages;  // Alias to resolve ambiguity

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;                       // for InputActionReference & Keyboard
using XRInputDevice = UnityEngine.XR.InputDevice;    // Alias to resolve ambiguity
#else
using XRInputDevice = UnityEngine.XR.InputDevice;    // Alias for consistency
#endif

public sealed class TruckReverseController : MonoBehaviour
{
    [Header("References (assign these)")]
    [SerializeField] private Transform truck;        // Truck root to move
    [SerializeField] private Transform xrOrigin;     // XR Origin root
    [SerializeField] private Transform canvas;       // Canvas to move with truck

    [Header("Motion")]
    [Tooltip("Speed in meters/sec (reverse).")]
    [SerializeField] private float reverseSpeed = 0.8f;

    [Tooltip("TOTAL meters allowed (hard cap). After this, reversing is locked until reset.")]
    [SerializeField] private float maxReverseDistance = 5f;

    [Tooltip("Stick dead-zone (0..1). Movement happens when Y < -deadZone.")]
    [Range(0f, 1f)]
    [SerializeField] private float deadZone = 0.35f;

#if ENABLE_INPUT_SYSTEM
    [Header("Input Actions (assign from XRI asset)")]
    [Tooltip("Vector2 action for LEFT hand stick (e.g., XRI LeftHand Locomotion/Move).")]
    [SerializeField] private InputActionReference leftStick;   // Vector2
    [Tooltip("Vector2 action for RIGHT hand stick (e.g., XRI RightHand Locomotion/Move).")]
    [SerializeField] private InputActionReference rightStick;  // Vector2
#endif

    [Header("Debug (Temporary)")]
    [SerializeField] private bool useKeyboardFallback = true; // Remove once XR input works
    [SerializeField] private bool showInputDebug = true;

    [Header("Blinking Effect")]
    [Tooltip("First GameObject to blink (will start enabled).")]
    [SerializeField] private GameObject blinkObject1;
    [Tooltip("Second GameObject to blink (will start disabled).")]
    [SerializeField] private GameObject blinkObject2;
    [Tooltip("Time in seconds between blinks.")]
    [SerializeField] private float blinkInterval = 0.5f;

    // XR devices cache (legacy fallback)
    private readonly List<XRInputDevice> leftDevices  = new();
    private readonly List<XRInputDevice> rightDevices = new();

    private CharacterController xrCC;
    private Coroutine blinkCoroutine;
    private bool isBlinking = false;

    // NOTE: This now represents the **TOTAL** distance reversed since last reset (hard cap),
    // not "per-hold" anymore.
    private float reversedThisHold = 0f;

    // Public property to allow external monitoring of reverse progress
    public float ReversedDistance => reversedThisHold;
    public float MaxReverseDistance => maxReverseDistance;
    public bool IsMaxDistanceReached => reversedThisHold >= maxReverseDistance;

    private Vector3 initialTruckPosition;
    private Vector3 initialXRPosition;
    private Vector3 initialCanvasPosition;

    private void Awake()
    {
        if (truck == null || xrOrigin == null)
        {
            Debug.LogError("[TruckReverseController] Please assign Truck and XR Origin in the Inspector.");
            enabled = false;
            return;
        }

        xrCC = xrOrigin.GetComponent<CharacterController>();

        // Store initial positions
        initialTruckPosition = truck.position;
        initialXRPosition = xrOrigin.position;
        if (canvas != null)
            initialCanvasPosition = canvas.position;

        RefreshDevices();
        InputDevices.deviceConnected    += OnDeviceChanged;
        InputDevices.deviceDisconnected += OnDeviceChanged;

        // Start blinking effect
        StartBlinking();
    }

#if ENABLE_INPUT_SYSTEM
    private void OnEnable()
    {
        leftStick?.action.Enable();
        rightStick?.action.Enable();
    }

    private void OnDisable()
    {
        leftStick?.action.Disable();
        rightStick?.action.Disable();
    }
#endif

    private void OnDestroy()
    {
        InputDevices.deviceConnected    -= OnDeviceChanged;
        InputDevices.deviceDisconnected -= OnDeviceChanged;
    }

    private void OnDeviceChanged(XRInputDevice _) => RefreshDevices();

    private void RefreshDevices()
    {
        leftDevices.Clear();
        rightDevices.Clear();

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left,
            leftDevices);

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right,
            rightDevices);

        if (showInputDebug)
        {
            List<XRInputDevice> allDevices = new List<XRInputDevice>();
            InputDevices.GetDevices(allDevices);
            Debug.Log($"[TruckReverseController] Total XR devices: {allDevices.Count}");
            foreach (var device in allDevices)
                Debug.Log($"[TruckReverseController] Device: '{device.name}' | Characteristics: {device.characteristics} | Valid: {device.isValid}");

            Debug.Log($"[TruckReverseController] Final device count - Left: {leftDevices.Count}, Right: {rightDevices.Count}");
        }
    }

    private void Update()
    {
        // HARD STOP: if we've already covered the max distance, do nothing
        if (reversedThisHold >= maxReverseDistance)
        {
            StopBlinking();
            return;
        }

        // 1) Read intent (stick back)
        float y = ReadStickY();

        // Keyboard fallback (Editor)
        bool keyboardInput = false;
#if ENABLE_INPUT_SYSTEM
        if (useKeyboardFallback && Keyboard.current != null)
            keyboardInput = Keyboard.current.sKey.isPressed;
#endif

        bool pullingBack = y < -deadZone || keyboardInput;

        if (showInputDebug)
            Debug.Log($"[TruckReverseController] Y: {y:F3}, Keyboard: {keyboardInput}, PullingBack: {pullingBack}, Total:{reversedThisHold:F2}/{maxReverseDistance:F2}");

        if (!pullingBack)
        {
            // IMPORTANT CHANGE: we NO LONGER reset here.
            // This makes maxReverseDistance a hard cap across multiple pulls.
            return;
        }

        // 2) Remaining budget (hard cap)
        float remaining = maxReverseDistance - reversedThisHold;
        if (remaining <= 0f) return;

        // 3) Calculate reverse movement (positive Z in world space for reverse)
        float deltaZ = reverseSpeed * Time.deltaTime;   // positive = reverse
        if (deltaZ > remaining) deltaZ = remaining;     // clamp to remaining budget

        // 4) Move truck in world space (only Z-axis)
        Vector3 newTruckPos = truck.position;
        newTruckPos.z += deltaZ;
        truck.position = newTruckPos;

        // 5) Move canvas along with truck (if assigned)
        if (canvas != null)
        {
            Vector3 newCanvasPos = canvas.position;
            newCanvasPos.z += deltaZ;
            canvas.position = newCanvasPos;
        }

        // 6) Move XR Origin to maintain relative position
        if (xrCC != null && xrCC.enabled)
        {
            xrCC.Move(new Vector3(0f, 0f, deltaZ));
        }
        else
        {
            Vector3 newXRPos = xrOrigin.position;
            newXRPos.z += deltaZ;
            xrOrigin.position = newXRPos;
        }

        // 7) Accumulate TOTAL distance moved (do NOT reset on release)
        reversedThisHold += deltaZ;

        // Check if we've reached max distance and stop blinking
        if (reversedThisHold >= maxReverseDistance)
        {
            StopBlinking();
        }
    }

    // ---------- INPUT ----------
    private float ReadStickY()
    {
#if ENABLE_INPUT_SYSTEM
        float y = 0f;
        if (leftStick  != null) y  = leftStick.action.ReadValue<Vector2>().y;
        if (rightStick != null)
        {
            float yr = rightStick.action.ReadValue<Vector2>().y;
            if (Mathf.Abs(yr) > Mathf.Abs(y)) y = yr;
        }
        if (Mathf.Abs(y) > 0.0001f) return y;
#endif
        // Fallback to legacy XR devices (if any)
        float yLeft  = GetPrimary2DAxisY(leftDevices);
        float yRight = GetPrimary2DAxisY(rightDevices);
        return Mathf.Abs(yLeft) > Mathf.Abs(yRight) ? yLeft : yRight;
    }

    private static float GetPrimary2DAxisY(List<XRInputDevice> list)
    {
        foreach (var d in list)
        {
            if (!d.isValid) continue;

            if (d.TryGetFeatureValue(XRCommonUsages.primary2DAxis, out Vector2 axis))
                return axis.y;

            if (d.TryGetFeatureValue(XRCommonUsages.secondary2DAxis, out Vector2 axis2))
                return axis2.y;
        }
        return 0f;
    }
    // ---------------------------

    // Public method to reset truck position AND allow reversing again
    public void ResetTruckPosition()
    {
        truck.position = initialTruckPosition;
        xrOrigin.position = initialXRPosition;
        if (canvas != null)
            canvas.position = initialCanvasPosition;
        reversedThisHold = 0f; // also unlocks the hard cap
        Debug.Log("[TruckReverseController] Truck position reset and reverse limit cleared.");

        // Restart blinking when reset
        StartBlinking();
    }

    // Optional utility: clear only the limit without moving objects
    public void ResetReverseLimitOnly()
    {
        reversedThisHold = 0f;
        Debug.Log("[TruckReverseController] Reverse limit cleared (positions unchanged).");
    }

    private void StartBlinking()
    {
        if (blinkObject1 == null || blinkObject2 == null)
        {
            Debug.LogWarning("[TruckReverseController] Blink objects not assigned. Blinking disabled.");
            return;
        }

        if (!isBlinking)
        {
            isBlinking = true;
            // Initialize: object1 enabled, object2 disabled
            blinkObject1.SetActive(true);
            blinkObject2.SetActive(false);
            blinkCoroutine = StartCoroutine(BlinkCoroutine());
        }
    }

    private void StopBlinking()
    {
        if (isBlinking)
        {
            isBlinking = false;
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
            // Optional: set final state of blink objects
            if (blinkObject1 != null) blinkObject1.SetActive(true);
            if (blinkObject2 != null) blinkObject2.SetActive(false);
        }
    }

    private IEnumerator BlinkCoroutine()
    {
        while (isBlinking && reversedThisHold < maxReverseDistance)
        {
            yield return new WaitForSeconds(blinkInterval);
            
            if (!isBlinking) break;
            
            // Toggle the objects
            bool obj1Active = blinkObject1.activeSelf;
            blinkObject1.SetActive(!obj1Active);
            blinkObject2.SetActive(obj1Active);
        }
    }
}
