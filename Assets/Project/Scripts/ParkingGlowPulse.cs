using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ParkingGlowPulse : MonoBehaviour
{
    [Header("Glow Settings")]
    public Material glowMaterial;           // The glowing material
    public float pulseSpeed = 2f;
    public float minEmission = 0.5f;
    public float maxEmission = 2f;

    private Renderer[] renderers;
    private Material[] instantiatedMaterials;
    private Color baseEmissionColor;

    void Start()
    {
        // Get all renderers in children
        renderers = GetComponentsInChildren<Renderer>();

        if (glowMaterial == null)
        {
            Debug.LogWarning($"[{name}] No glow material assigned!");
            return;
        }

        // Store base emission color
        baseEmissionColor = glowMaterial.GetColor("_EmissionColor");

        // Instantiate materials for each renderer
        instantiatedMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            // Create unique instance so emission changes don’t affect others
            Material matInstance = new Material(glowMaterial);
            matInstance.EnableKeyword("_EMISSION"); // important
            renderers[i].material = matInstance;
            instantiatedMaterials[i] = matInstance;
        }
    }

    void Update()
    {
        if (instantiatedMaterials == null) return;

        float emission = minEmission + Mathf.PingPong(Time.time * pulseSpeed, maxEmission - minEmission);

        foreach (var mat in instantiatedMaterials)
        {
            // Multiply by base color
            Color finalColor = baseEmissionColor * Mathf.LinearToGammaSpace(emission);
            mat.SetColor("_EmissionColor", finalColor);
        }
    }
}
