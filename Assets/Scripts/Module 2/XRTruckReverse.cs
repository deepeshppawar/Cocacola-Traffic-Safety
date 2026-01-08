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

    private InputDevice rightHandDevice;
    private bool hasStopped = false;

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
        // Do nothing after stopping
        if (hasStopped)
            return;

        // Check Z position FIRST
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

            // Pull joystick backward
            if (verticalInput < -deadZone)
            {
                Vector3 movement = truckTransform.forward * reverseSpeed * Time.deltaTime;

                truckTransform.position += movement;
                xrOriginTransform.position += movement;
                canvas.position += movement;
            }
        }
    }

    void StopTruck()
    {
        hasStopped = true;

        // Enable objects
        foreach (GameObject obj in enableOnStop)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // Disable objects
        foreach (GameObject obj in disableOnStop)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        Debug.Log("Truck stopped at Z = " + truckTransform.position.z);
    }
}
