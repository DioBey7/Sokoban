using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle hapticsToggle;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button openButton;

    private void Awake()
    {
        if (openButton != null) openButton.onClick.AddListener(OpenSettings);
        if (closeButton != null) closeButton.onClick.AddListener(CloseSettings);

        if (musicToggle != null) musicToggle.onValueChanged.AddListener(OnMusicToggled);
        if (sfxToggle != null) sfxToggle.onValueChanged.AddListener(OnSFXToggled);
        if (hapticsToggle != null) hapticsToggle.onValueChanged.AddListener(OnHapticsToggled);

        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (musicToggle != null) musicToggle.SetIsOnWithoutNotify(SettingsManager.IsMusicOn);
        if (sfxToggle != null) sfxToggle.SetIsOnWithoutNotify(SettingsManager.IsSFXOn);
        if (hapticsToggle != null) hapticsToggle.SetIsOnWithoutNotify(SettingsManager.IsHapticsOn);

        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void OnMusicToggled(bool isOn)
    {
        SettingsManager.IsMusicOn = isOn;
        if (SoundManager.Instance != null) SoundManager.Instance.ApplySettings();
    }

    private void OnSFXToggled(bool isOn)
    {
        SettingsManager.IsSFXOn = isOn;
        if (SoundManager.Instance != null) SoundManager.Instance.ApplySettings();
    }

    private void OnHapticsToggled(bool isOn)
    {
        SettingsManager.IsHapticsOn = isOn;
    }
}