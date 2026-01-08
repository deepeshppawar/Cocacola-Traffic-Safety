using UnityEngine;
using System.Collections;

public class HighlightAction : InspectionAction
{
    public Renderer targetRenderer;
    public Color emissionColor = Color.green;
    public float duration = 2f;

    Material material;
    Color originalEmission;

    void Awake()
    {
        material = targetRenderer.material;
        originalEmission = material.GetColor("_EmissionColor");
    }

    public override void Execute()
    {
        StartCoroutine(Highlight());
    }

    IEnumerator Highlight()
    {
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emissionColor);

        yield return new WaitForSeconds(duration);

        material.SetColor("_EmissionColor", originalEmission);
    }
}
