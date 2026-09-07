using UnityEngine;
using MoreMountains.NiceVibrations;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; }

    private bool isHapticsEnabled = true;
    private bool isReady = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!PlayerPrefs.HasKey("HapticsEnabled"))
        {
            PlayerPrefs.SetInt("HapticsEnabled", 1);
            PlayerPrefs.Save();
        }

        LoadSettings();
    }

    private void Start()
    {
        isReady = true;
    }

    private void LoadSettings()
    {
        isHapticsEnabled = PlayerPrefs.GetInt("HapticsEnabled", 1) == 1;
        MMVibrationManager.SetHapticsActive(isHapticsEnabled);
    }

    public void ToggleHaptics(bool state)
    {
        if (!isReady) return;
        isHapticsEnabled = state;
        PlayerPrefs.SetInt("HapticsEnabled", state ? 1 : 0);
        PlayerPrefs.Save();
        MMVibrationManager.SetHapticsActive(state);
    }

    public void PlaySuccess()
    {
        if (!isHapticsEnabled) return;
        MMVibrationManager.Haptic(HapticTypes.Success);
    }

    public void PlayError()
    {
        if (!isHapticsEnabled) return;
        MMVibrationManager.Haptic(HapticTypes.Failure);
    }

    public void PlayWarning()
    {
        if (!isHapticsEnabled) return;
        MMVibrationManager.Haptic(HapticTypes.Warning);
    }

    public void PlayDoorToggle()
    {
        if (!isHapticsEnabled) return;
        MMVibrationManager.Haptic(HapticTypes.MediumImpact);
    }

    public void PlayPortalReject()
    {
        if (!isHapticsEnabled) return;
        MMVibrationManager.Haptic(HapticTypes.HeavyImpact);
    }
}