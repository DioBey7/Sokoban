using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SettingsMenuController : MonoBehaviour
{
    public static SettingsMenuController Instance { get; private set; }

    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private CanvasGroup settingsCanvasGroup;
    [SerializeField] private Transform settingsWindow;

    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle hapticsToggle;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button openButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (openButton != null) openButton.onClick.AddListener(OpenSettings);
        if (closeButton != null) closeButton.onClick.AddListener(CloseSettings);

        if (musicToggle != null) musicToggle.onValueChanged.AddListener(OnMusicToggled);
        if (sfxToggle != null) sfxToggle.onValueChanged.AddListener(OnSFXToggled);
        if (hapticsToggle != null) hapticsToggle.onValueChanged.AddListener(OnHapticsToggled);

        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void Start()
    {
        SetPanelToTop(settingsPanel);
    }

    private void SetPanelToTop(GameObject panel)
    {
        if (panel == null) return;
        Canvas c = panel.GetComponent<Canvas>();
        if (c == null) c = panel.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 105;
        if (panel.GetComponent<GraphicRaycaster>() == null) panel.AddComponent<GraphicRaycaster>();
    }

    public void OpenSettings()
    {
        if (musicToggle != null) musicToggle.SetIsOnWithoutNotify(SettingsManager.IsMusicOn);
        if (sfxToggle != null) sfxToggle.SetIsOnWithoutNotify(SettingsManager.IsSFXOn);
        if (hapticsToggle != null) hapticsToggle.SetIsOnWithoutNotify(SettingsManager.IsHapticsOn);

        if (SoundManager.Instance != null) SoundManager.Instance.PlayCoreSFX(SoundManager.Instance.uiClickSound);

        if (settingsPanel != null) settingsPanel.SetActive(true);

        if (settingsCanvasGroup != null)
        {
            settingsCanvasGroup.DOKill();
            settingsCanvasGroup.alpha = 0f;
            settingsCanvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
        }

        if (settingsWindow != null)
        {
            settingsWindow.DOKill();
            settingsWindow.localScale = Vector3.one * 0.8f;
            settingsWindow.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }

    public void CloseSettings()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCoreSFX(SoundManager.Instance.uiClickSound);

        if (settingsWindow != null)
        {
            settingsWindow.DOKill();
            settingsWindow.DOScale(Vector3.one * 0.8f, 0.2f).SetEase(Ease.InBack).SetUpdate(true);
        }

        if (settingsCanvasGroup != null)
        {
            settingsCanvasGroup.DOKill();
            settingsCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() => settingsPanel.SetActive(false));
        }
        else
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }
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