using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIFadeIn : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Image image;
    public TextMeshProUGUI text;
    public GameObject uiObject;
    public float fadeInSpeed = 1.0f;
    
    private float imageTargetAlpha;
    private float textTargetAlpha;
    private Renderer objectRenderer;
    private Material objectMaterial;
    private float objectTargetAlpha;

    public SceneController sceneController;

    private void Awake()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0;
        if (image != null)
        {
            imageTargetAlpha = image.color.a;
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }
        if (text != null)
        {
            textTargetAlpha = text.color.a;
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        }
        if (uiObject != null)
        {
            objectRenderer = uiObject.GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                objectMaterial = objectRenderer.material;
                objectTargetAlpha = objectMaterial.color.a;
                Color color = objectMaterial.color;
                color.a = 0;
                objectMaterial.color = color;
                
                // Ensure the material is set to transparent mode
                objectMaterial.SetFloat("_Mode", 3); // 3 = Transparent
                objectMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                objectMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                objectMaterial.SetInt("_ZWrite", 0);
                objectMaterial.EnableKeyword("_ALPHABLEND_ON");
                objectMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            uiObject.SetActive(false);
        }
    }

    private void Start()
    {
        StartFadeIn(fadeInSpeed);
        Invoke("SceneChange", 10f);
    }

    public void SceneChange()
    {
        sceneController.ChangeScene("1. Introduction");
    }

    public void StartFadeIn(float speed)
    {
        StopAllCoroutines();
        StartCoroutine(DelayedFadeInCoroutine(speed));
    }

    private IEnumerator DelayedFadeInCoroutine(float speed)
    {
        yield return new WaitForSeconds(2f); // 3-second delay before starting fade-in
        
        if (uiObject != null)
            uiObject.SetActive(true);
        
        float elapsedTime = 0;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * speed;
            
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime);
            if (image != null)
                image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(0, imageTargetAlpha, elapsedTime));
            if (text != null)
                text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(0, textTargetAlpha, elapsedTime));
            if (objectMaterial != null)
            {
                Color color = objectMaterial.color;
                color.a = Mathf.Lerp(0, objectTargetAlpha, elapsedTime);
                objectMaterial.color = color;
            }
            yield return null;
        }
    }
}

// using System.Collections;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class UIFadeIn : MonoBehaviour
// {
//     public CanvasGroup canvasGroup;
//     public Image image;
//     public TextMeshProUGUI text;
//     public GameObject uiObject;
//     public float fadeInSpeed = 1.0f;
    
//     private float imageTargetAlpha;
//     private float textTargetAlpha;
//     private Renderer objectRenderer;
//     private Material objectMaterial;
//     private float objectTargetAlpha;

//     public SceneController sceneController;

//     private void Awake()
//     {
//         if (canvasGroup != null)
//             canvasGroup.alpha = 0;
//         if (image != null)
//         {
//             imageTargetAlpha = image.color.a;
//             image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
//         }
//         if (text != null)
//         {
//             textTargetAlpha = text.color.a;
//             text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
//         }
//         if (uiObject != null)
//         {
//             objectRenderer = uiObject.GetComponent<Renderer>();
//             if (objectRenderer != null)
//             {
//                 objectMaterial = objectRenderer.material;
//                 objectTargetAlpha = objectMaterial.color.a;
//                 Color color = objectMaterial.color;
//                 color.a = 0;
//                 objectMaterial.color = color;
                
//                 // Ensure the material is set to transparent mode
//                 objectMaterial.SetFloat("_Mode", 3); // 3 = Transparent
//                 objectMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
//                 objectMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
//                 objectMaterial.SetInt("_ZWrite", 0);
//                 objectMaterial.EnableKeyword("_ALPHABLEND_ON");
//                 objectMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
//             }
//             uiObject.SetActive(false);
//         }
//     }

//     private void Start()
//     {
//         StartFadeIn(fadeInSpeed);
//         Invoke("SceneChange", 10f);
//     }

//     public void SceneChange()
//     {
//         sceneController.ChangeScene(1);
//     }

//     public void StartFadeIn(float speed)
//     {
//         StopAllCoroutines();
//         if (uiObject != null)
//             uiObject.SetActive(true);
//         StartCoroutine(FadeInCoroutine(speed));
//     }

//     private IEnumerator FadeInCoroutine(float speed)
//     {
//         float elapsedTime = 0;
//         while (elapsedTime < 1f)
//         {
//             elapsedTime += Time.deltaTime * speed;
            
//             if (canvasGroup != null)
//                 canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime);
//             if (image != null)
//                 image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(0, imageTargetAlpha, elapsedTime));
//             if (text != null)
//                 text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(0, textTargetAlpha, elapsedTime));
//             if (objectMaterial != null)
//             {
//                 Color color = objectMaterial.color;
//                 color.a = Mathf.Lerp(0, objectTargetAlpha, elapsedTime);
//                 objectMaterial.color = color;
//             }
//             yield return null;
//         }
//     }
// }
