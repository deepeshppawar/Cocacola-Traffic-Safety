using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[DisallowMultipleComponent]
public sealed class Module2Manager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform playerRig;

    [Header("Parking Zones (Buttons placed on areas)")]
    [SerializeField] private GameObject parkingZone1; // Forklift near
    [SerializeField] private GameObject parkingZone2; // Safe
    [SerializeField] private GameObject parkingZone3; // Worker near

    [Header("Hazards (Always visible)")]
    [SerializeField] private GameObject forklift;
    [SerializeField] private GameObject worker;

    [Header("UI - Intro & Select Parking")]
    [SerializeField] private GameObject canvasIntro;
    [SerializeField] private Button buttonIntroProceed;

    [SerializeField] private GameObject canvasSelectParking;
    [SerializeField] private TextMeshProUGUI textSelectInfo;
    [SerializeField] private Button buttonSelectProceed;

    [Header("UI - Feedback")]
    [SerializeField] private GameObject canvasFeedback;
    [SerializeField] private GameObject panelCorrect;
    [SerializeField] private TextMeshProUGUI textCorrect;
    [SerializeField] private Button buttonCorrectProceed;

    [SerializeField] private GameObject panelIncorrect;
    [SerializeField] private TextMeshProUGUI textIncorrect;
    [SerializeField] private Button buttonIncorrectProceed;

    [Header("UI - Select Another Zone")]
    [SerializeField] private GameObject canvasSelectAnother;
    [SerializeField] private TextMeshProUGUI textSelectAnotherInfo;
    [SerializeField] private Button buttonSelectAnotherProceed;

    [Header("UI - Completion")]
    [SerializeField] private GameObject canvasCompletion;
    [SerializeField] private TextMeshProUGUI textCompletion;
    [SerializeField] private Button buttonCompletionProceed;

    [Header("Next Scene Settings")]
    [SerializeField] private string nextSceneName = "Scene3";

    // 🔊 AUDIO SETTINGS
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource; // Drag AudioSource here

    [Header("Canvas Audios")]
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip selectParkingClip;
    [SerializeField] private AudioClip selectAnotherClip;
    [SerializeField] private AudioClip completionClip;

    [Header("Zone Feedback Audios")]
    [SerializeField] private AudioClip zone1IncorrectClip;  // “Zone 1 is unsafe” voice line
    [SerializeField] private AudioClip zone2CorrectClip;    // “Correct! Zone 2 is safe” voice line
    [SerializeField] private AudioClip zone3IncorrectClip;  // “Zone 3 is unsafe” voice line

    private bool[] zoneUsed = new bool[4]; // 1–3 used

    // =================================================================
    private void Awake()
    {
        if (buttonIntroProceed) buttonIntroProceed.onClick.AddListener(OnIntroProceed);
        if (buttonSelectProceed) buttonSelectProceed.onClick.AddListener(OnSelectParkingProceed);
        if (buttonCorrectProceed) buttonCorrectProceed.onClick.AddListener(ShowCompletion);
        if (buttonIncorrectProceed) buttonIncorrectProceed.onClick.AddListener(ShowSelectAnotherZone);
        if (buttonSelectAnotherProceed) buttonSelectAnotherProceed.onClick.AddListener(BackToSelectParking);
        if (buttonCompletionProceed) buttonCompletionProceed.onClick.AddListener(LoadNextScene);
    }

    private void Start()
    {
        HideAll();
        SetZonesActive(false);
        SetActive(canvasIntro, true);
        PlayAudio(introClip);
    }

    // =================================================================
    // FLOW
    // =================================================================

    private void OnIntroProceed()
    {
        HideAll();
        SetActive(canvasSelectParking, true);
        PlayAudio(selectParkingClip);

        if (textSelectInfo)
            textSelectInfo.text = "Three parking zones are highlighted. Please select a safe parking zone.";

        SetZonesActive(false);
    }

    private void OnSelectParkingProceed()
    {
        HideAll();
        SetZonesActive(true);
        Debug.Log("[Module2Manager] Waiting for user to select a parking zone...");
    }

    // Called by parking zone buttons
    public void OnZoneSelected(int zoneIndex)
    {
        Debug.Log($"[Module2Manager] Zone {zoneIndex} selected.");
        HideAll();
        SetZonesActive(false);

        if (zoneIndex >= 1 && zoneIndex <= 3)
            zoneUsed[zoneIndex] = true;

        SetActive(canvasFeedback, true);

        switch (zoneIndex)
        {
            case 1:
                SetActive(panelIncorrect, true);
                if (textIncorrect)
                    textIncorrect.text = "Zone 1 is unsafe because a forklift is operating nearby.";
                PlayAudio(zone1IncorrectClip);
                break;

            case 2:
                SetActive(panelCorrect, true);
                if (textCorrect)
                    textCorrect.text = "Correct! Zone 2 is safe. Zone 1 and Zone 3 are unsafe because Zone 1 has a forklift operating nearby and Zone 3 has a worker hazard.";
                PlayAudio(zone2CorrectClip);
                break;

            case 3:
                SetActive(panelIncorrect, true);
                if (textIncorrect)
                    textIncorrect.text = "Zone 3 is unsafe because a worker is standing in the parking zone.";
                PlayAudio(zone3IncorrectClip);
                break;
        }
    }

    private void ShowSelectAnotherZone()
    {
        HideAll();
        SetActive(canvasSelectAnother, true);
        PlayAudio(selectAnotherClip);

        if (textSelectAnotherInfo)
            textSelectAnotherInfo.text = "Please select another safe parking zone.";
    }

    private void BackToSelectParking()
    {
        HideAll();
        Debug.Log("[Module2Manager] Player can now select another parking zone.");

        if (!zoneUsed[1] && parkingZone1) parkingZone1.SetActive(true);
        if (!zoneUsed[2] && parkingZone2) parkingZone2.SetActive(true);
        if (!zoneUsed[3] && parkingZone3) parkingZone3.SetActive(true);
    }

    private void ShowCompletion()
    {
        HideAll();
        SetActive(canvasCompletion, true);
        PlayAudio(completionClip);

        if (textCompletion)
            textCompletion.text = "You correctly identified the safe parking area. Good job!";
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("[Module2Manager] nextSceneName is empty.");
    }

    // =================================================================
    // HELPERS
    // =================================================================

    private void HideAll()
    {
        SetActive(canvasIntro, false);
        SetActive(canvasSelectParking, false);
        SetActive(canvasFeedback, false);
        SetActive(panelCorrect, false);
        SetActive(panelIncorrect, false);
        SetActive(canvasSelectAnother, false);
        SetActive(canvasCompletion, false);
    }

    private void SetZonesActive(bool active)
    {
        if (parkingZone1 && !zoneUsed[1]) parkingZone1.SetActive(active);
        if (parkingZone2 && !zoneUsed[2]) parkingZone2.SetActive(active);
        if (parkingZone3 && !zoneUsed[3]) parkingZone3.SetActive(active);
    }

    private void SetActive(GameObject go, bool active)
    {
        if (go && go.activeSelf != active)
            go.SetActive(active);
    }

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
}
