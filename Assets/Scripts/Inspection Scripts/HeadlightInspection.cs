using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeadlightInspection : MonoBehaviour
{
    [Header("Headlight Renderers")]
    public Renderer[] headlightRenderers;
    public int materialIndex = 0;

    [Header("Glow Settings")]
    public Color emissionColor = Color.white;
    public float maxEmissionIntensity = 4f;
    public float glowUpTime = 1f;
    public float glowHoldTime = 1f;
    public float glowDownTime = 1f;

    private bool isChecking = false;

    // Store original emission colors
    private List<Material> materials = new List<Material>();
    private List<Color> originalEmissions = new List<Color>();

    // Event for teleport / manager
    public System.Action OnInspectionCompleted;

    private void Awake()
    {
        foreach (Renderer renderer in headlightRenderers)
        {
            Material mat = renderer.materials[materialIndex];
            materials.Add(mat);

            if (mat.HasProperty("_EmissionColor"))
            {
                originalEmissions.Add(mat.GetColor("_EmissionColor"));
            }
            else
            {
                originalEmissions.Add(Color.black);
            }

            mat.EnableKeyword("_EMISSION");
        }
    }

    /// <summary>
    /// Call this to start headlight inspection
    /// </summary>
    public void StartInspection()
    {
        if (isChecking) return;
        StartCoroutine(GlowRoutine());
    }

    private IEnumerator GlowRoutine()
    {
        isChecking = true;

        // Glow up
        yield return LerpEmission(0f, maxEmissionIntensity, glowUpTime);

        // Hold glow
        yield return new WaitForSeconds(glowHoldTime);

        // Glow down
        yield return LerpEmission(maxEmissionIntensity, 0f, glowDownTime);

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
