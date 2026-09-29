using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Level Complete Panel")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private CanvasGroup completeCanvasGroup;
    [SerializeField] private Transform completePopupWindow;
    [SerializeField] private TextMeshProUGUI bonusText;

    [Header("Fail Panel")]
    [SerializeField] private GameObject failPanel;
    [SerializeField] private CanvasGroup failCanvasGroup;
    [SerializeField] private Transform failPopupWindow;

    [Header("Warning Panel")]
    [SerializeField] private GameObject zeroUndoWarningPanel;
    [SerializeField] private CanvasGroup zeroUndoCanvasGroup;
    [SerializeField] private Transform warningPopupWindow;

    [Header("Pause / Options Panel")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private CanvasGroup pauseCanvasGroup;
    [SerializeField] private Transform pausePopupWindow;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetPanelToTop(levelCompletePanel);
        SetPanelToTop(failPanel);
        SetPanelToTop(zeroUndoWarningPanel);
        SetPanelToTop(pausePanel);

        HideAllPanelsInstant();
    }

    private void SetPanelToTop(GameObject panel)
    {
        if (panel == null) return;
        Canvas c = panel.GetComponent<Canvas>();
        if (c == null) c = panel.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 100;
        if (panel.GetComponent<GraphicRaycaster>() == null) panel.AddComponent<GraphicRaycaster>();
    }

    public void HideAllPanelsInstant()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);
        if (zeroUndoWarningPanel != null) zeroUndoWarningPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void UpdateLevelText(int level)
    {
        if (levelText != null) levelText.text = "LEVEL " + level.ToString();
    }

    public void ShowLevelCompletePanel(int bonusAmount)
    {
        if (levelCompletePanel == null) return;
        levelCompletePanel.SetActive(true);

        if (bonusText != null) bonusText.text = $"+{GameUtils.FormatNumber(bonusAmount)} Gem";
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.levelCompleteSound);

        AnimatePopup(completeCanvasGroup, completePopupWindow);
    }

    public void HideLevelCompletePanel()
    {
        HideAllPanelsInstant();
    }

    public void ShowFailPanel()
    {
        if (failPanel == null) return;
        failPanel.SetActive(true);

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.errorSound);

        AnimatePopup(failCanvasGroup, failPopupWindow);
    }

    public void HideFailPanel()
    {
        HideAllPanelsInstant();
    }

    public void ShowZeroUndoWarningPanel()
    {
        if (zeroUndoWarningPanel == null) return;
        zeroUndoWarningPanel.SetActive(true);
        AnimatePopup(zeroUndoCanvasGroup, warningPopupWindow);
    }

    public void HideZeroUndoWarningPanel()
    {
        ClosePopup(zeroUndoWarningPanel, zeroUndoCanvasGroup, warningPopupWindow);
    }

    public void OnPauseButtonClicked()
    {
        if (pausePanel == null) return;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        AnimatePopup(pauseCanvasGroup, pausePopupWindow);
    }

    public void OnResumeButtonClicked()
    {
        Time.timeScale = 1f;
        ClosePopup(pausePanel, pauseCanvasGroup, pausePopupWindow);
    }

    public void OnMainMenuButtonClicked()
    {
        Time.timeScale = 1f;

        if (SoundManager.Instance != null) SoundManager.Instance.PlayCoreSFX(SoundManager.Instance.uiClickSound);

        HideAllPanelsInstant();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ClearLevel();
        }

        if (MainMenuController.Instance != null && MainMenuController.Instance.mainMenuPanel != null)
        {
            MainMenuController.Instance.mainMenuPanel.SetActive(true);
        }
    }

    public void OnNextLevelClicked()
    {
        HideAllPanelsInstant();
        if (LevelManager.Instance != null) LevelManager.Instance.NextLevel();
    }

    public void OnRestartLevelClicked()
    {
        HideAllPanelsInstant();
        if (LevelManager.Instance != null) LevelManager.Instance.RestartLevel();
    }

    private void AnimatePopup(CanvasGroup cg, Transform window)
    {
        if (cg != null)
        {
            cg.DOKill();
            cg.alpha = 0f;
            cg.DOFade(1f, 0.3f).SetUpdate(true);
        }

        if (window != null)
        {
            window.DOKill();
            window.localScale = Vector3.one * 0.8f;
            window.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }

    private void ClosePopup(GameObject panel, CanvasGroup cg, Transform window)
    {
        if (window != null)
        {
            window.DOKill();
            window.DOScale(Vector3.one * 0.8f, 0.2f).SetEase(Ease.InBack).SetUpdate(true);
        }

        if (cg != null)
        {
            cg.DOKill();
            cg.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() => panel.SetActive(false));
        }
        else
        {
            panel.SetActive(false);
        }
    }
}