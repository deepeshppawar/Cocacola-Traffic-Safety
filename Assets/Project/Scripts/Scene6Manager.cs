// Scene6Manager.cs
// Scene 6 flow:
// Start -> Document Collect canvas -> Proceed -> (activate docs) -> Document Collection Complete canvas
// -> Proceed -> Exit canvas -> Exit quits game.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class Scene6Manager : MonoBehaviour
{
    [Header("Document Collect")]
    [SerializeField] private GameObject documentCollectCanvas;
    [SerializeField] private Button documentCollectProceedButton;

    [Header("Document Collection Complete")]
    [SerializeField] private GameObject documentCollectionCompleteCanvas;
    [SerializeField] private Button documentCollectionCompleteProceedButton;
    [SerializeField] private List<GameObject> documentRefsToActivate = new List<GameObject>(); // set these active on complete

    [Header("Exit Flow")]
    [SerializeField] private GameObject exitCanvas;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        if (documentCollectProceedButton)
            documentCollectProceedButton.onClick.AddListener(OnDocumentCollectProceed);

        if (documentCollectionCompleteProceedButton)
            documentCollectionCompleteProceedButton.onClick.AddListener(OnDocumentCollectionCompleteProceed);

        if (exitButton)
            exitButton.onClick.AddListener(OnExitPressed);
    }

    private void OnDestroy()
    {
        if (documentCollectProceedButton)
            documentCollectProceedButton.onClick.RemoveListener(OnDocumentCollectProceed);

        if (documentCollectionCompleteProceedButton)
            documentCollectionCompleteProceedButton.onClick.RemoveListener(OnDocumentCollectionCompleteProceed);

        if (exitButton)
            exitButton.onClick.RemoveListener(OnExitPressed);
    }

    private void Start()
    {
        // Clean slate
        HideAll();

        // Ensure docs start disabled (you can pre-place them in scene, they'll flip on at complete)
        foreach (var doc in documentRefsToActivate)
        {
            if (doc) doc.SetActive(false);
        }

        // Show first step
        SetActive(documentCollectCanvas, true);
    }

    // Step 1 -> Step 2
    private void OnDocumentCollectProceed()
    {
        // Activate all referenced docs
        foreach (var doc in documentRefsToActivate)
        {
            if (doc) doc.SetActive(true);
        }

        // Show "Collection Complete" canvas
        HideAll();
        SetActive(documentCollectionCompleteCanvas, true);
    }

    // Step 2 -> Step 3
    private void OnDocumentCollectionCompleteProceed()
    {
        HideAll();
        SetActive(exitCanvas, true);
    }

    private void OnExitPressed()
    {
        QuitGame();
    }

    // ---------- Helpers ----------
    private void HideAll()
    {
        SetActive(documentCollectCanvas, false);
        SetActive(documentCollectionCompleteCanvas, false);
        SetActive(exitCanvas, false);
    }

    private static void SetActive(GameObject go, bool active)
    {
        if (go && go.activeSelf != active)
            go.SetActive(active);
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        // In Editor, stop Play Mode
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
