using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnSignalInspection : MonoBehaviour
{
    [Header("Indicator Renderers")]
    public Renderer[] indicatorRenderers;
    public int materialIndex = 0;

    [Header("Emission Settings")]
    public Color emissionColor = Color.yellow;
    public float maxEmissionIntensity = 3.5f;

    [Header("Blink Timing")]
    public int blinkCount = 5;
    public float glowUpTime = 0.15f;
    public float glowHoldTime = 0.15f;
    public float glowDownTime = 0.15f;
    public float offPauseTime = 0.2f;

    private bool isChecking = false;

    private List<Material> materials = new List<Material>();
    private List<Color> originalEmissions = new List<Color>();

    public System.Action OnInspectionCompleted;

    private void Awake()
    {
        foreach (Renderer renderer in indicatorRenderers)
        {
            Material mat = renderer.materials[materialIndex];
            materials.Add(mat);

            if (mat.HasProperty("_EmissionColor"))
                originalEmissions.Add(mat.GetColor("_EmissionColor"));
            else
                originalEmissions.Add(Color.black);

            mat.EnableKeyword("_EMISSION");
        }
    }

    /// <summary>
    /// Call this to start turn signal inspection
    /// </summary>
    public void StartInspection()
    {
        if (isChecking) return;
        StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        isChecking = true;

        for (int i = 0; i < blinkCount; i++)
        {
            // Glow up
            yield return LerpEmission(0f, maxEmissionIntensity, glowUpTime);

            // Hold ON
            yield return new WaitForSeconds(glowHoldTime);

            // Glow down
            yield return LerpEmission(maxEmissionIntensity, 0f, glowDownTime);

            // OFF pause
            yield return new WaitForSeconds(offPauseTime);
        }

        // Restore original emission
        for (int i = 0; i < materials.Count; i++)
        {
            materials[i].SetColor("_EmissionColor", originalEmissions[i]);
        }

        isChecking = false;
        OnInspectionCompleted?.Invoke();
    }

    private IEnumerator LerpEmission(float start, float end, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float intensity = Mathf.Lerp(start, end, elapsed / duration);
            SetEmission(intensity);

            elapsed += Time.deltaTime;
            yield return null;
        }

        SetEmission(end);
    }

    private void SetEmission(float intensity)
    {
        Color finalColor = emissionColor * intensity;

        foreach (Material mat in materials)
        {
            mat.SetColor("_EmissionColor", finalColor);
        }
    }
}
