using UnityEngine;
using System.Collections.Generic;

public class InspectionStep : MonoBehaviour
{
    [Header("Teleport")]
    public Transform teleportTarget;
    public Transform xrOrigin;

    [Header("Enable / Disable")]
    public List<GameObject> enableObjects;
    public List<GameObject> disableObjects;

    [Header("Actions")]
    public List<InspectionAction> actions;

    public void StartStep()
    {
        Teleport();
        SetObjects();
    }

    public void ExecuteActions()
    {
        foreach (var action in actions)
        {
            action.Execute();
        }
    }

    void Teleport()
    {
        if (!xrOrigin || !teleportTarget) return;

        xrOrigin.position = teleportTarget.position;
        xrOrigin.rotation = teleportTarget.rotation;
    }

    void SetObjects()
    {
        foreach (var obj in enableObjects)
            obj.SetActive(true);

        foreach (var obj in disableObjects)
            obj.SetActive(false);
    }
}
