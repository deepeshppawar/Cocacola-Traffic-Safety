using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class XRTruckReverse : MonoBehaviour
{
    [Header("Objects to Move")]
    public Transform truckTransform;
    public Transform xrOriginTransform;
    public Transform canvas;

    [Header("Movement Settings")]
    public float reverseSpeed = 2f;
    public float deadZone = 0.2f;

    [Header("Z Stop Settings")]
    public float stopZ = 30f;

    [Header("Enable / Disable On Stop")]
    public GameObject[] enableOnStop;
    public GameObject[] disableOnStop;

    [Header("Marshal Settings")]
    public Transform marshalTransform;

    [Header("Marshal Look Settings")]
    public bool marshalLookAtTruck = true;

    [Header("Marshal X Move Settings")]
    public float marshalMoveTriggerZ = 22f;
    public float marshalMoveXAmount = 2f;
    public float marshalMoveSpeed = 2f;

    private InputDevice rightHandDevice;
    private bool hasStopped = false;
    private bool marshalHasMoved = false;

    private Vector3 marshalTargetPosition;

    void Start()
    {
        InitializeRightHand();
    }

    void InitializeRightHand()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller,
            devices
        );

        if (devices.Count > 0)
            rightHandDevice = devices[0];
    }

    void Update()
    {
        // ===== Marshal always looks at truck =====
        if (marshalLookAtTruck && marshalTransform != null && truckTransform != null)
        {
            Vector3 dir = truckTransform.position - marshalTransform.position;
            dir.y = 0f; // keep marshal upright
            marshalTransform.rotation = Quaternion.LookRotation(dir);
        }

        // ===== Trigger marshal X movement when truck reaches Z =====
        if (!marshalHasMoved && truckTransform.position.z >= marshalMoveTriggerZ)
        {
            marshalHasMoved = true;
            marshalTargetPosition =
                marshalTransform.position + new Vector3(marshalMoveXAmount, 0f, 0f);
        }

        // ===== Move marshal smoothly =====
        if (marshalHasMoved && marshalTransform != null)
        {
            marshalTransform.position = Vector3.MoveTowards(
                marshalTransform.position,
                marshalTargetPosition,
                marshalMoveSpeed * Time.deltaTime
            );
        }

        // ===== Stop logic =====
        if (hasStopped)
            return;

        if (truckTransform.position.z >= stopZ)
        {
            StopTruck();
            return;
        }

        if (!rightHandDevice.isValid)
        {
            InitializeRightHand();
            return;
        }

        if (rightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystick))
        {
            float verticalInput = joystick.y;

            if (verticalInput < -deadZone)
            {
                Vector3 movement =
                    truckTransform.forward * reverseSpeed * Time.deltaTime;

                truckTransform.position += movement;
                xrOriginTransform.position += movement;
                canvas.position += movement;
            }
        }
    }

    void StopTruck()
    {
        hasStopped = true;

        foreach (GameObject obj in enableOnStop)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        foreach (GameObject obj in disableOnStop)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        Debug.Log("Truck stopped at Z = " + truckTransform.position.z);
    }
}
