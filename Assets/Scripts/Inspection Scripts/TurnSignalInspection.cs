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
        materials.Clear();
        originalEmissions.Clear();

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

    public void StartInspection()
    {
        if (isChecking) return;
        StartCoroutine(SequentialBlinkRoutine());
    }

    private IEnumerator SequentialBlinkRoutine()
    {
        isChecking = true;

        for (int i = 0; i < materials.Count; i++)
        {
            yield return BlinkSingleIndicator(i);
        }

        isChecking = false;
        OnInspectionCompleted?.Invoke();
    }

    private IEnumerator BlinkSingleIndicator(int index)
    {
        Material mat = materials[index];

        for (int i = 0; i < blinkCount; i++)
        {
            // Glow up
            yield return LerpEmission(mat, 0f, maxEmissionIntensity, glowUpTime);

            // Hold ON
            yield return new WaitForSeconds(glowHoldTime);

            // Glow down
            yield return LerpEmission(mat, maxEmissionIntensity, 0f, glowDownTime);

            // OFF pause
            yield return new WaitForSeconds(offPauseTime);
        }

        // Restore original emission
        mat.SetColor("_EmissionColor", originalEmissions[index]);
    }

    private IEnumerator LerpEmission(Material mat, float start, float end, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float intensity = Mathf.Lerp(start, end, elapsed / duration);
            SetEmission(mat, intensity);

            elapsed += Time.deltaTime;
            yield return null;
        }

        SetEmission(mat, end);
    }

    private void SetEmission(Material mat, float intensity)
    {
        Color finalColor = emissionColor * intensity;
        mat.SetColor("_EmissionColor", finalColor);
    }
}
