using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private TextMeshProUGUI bonusText;

    [SerializeField] private GameObject zeroUndoWarningPanel;
    [SerializeField] private CanvasGroup zeroUndoCanvasGroup;

    [SerializeField] private GameObject failPanel;
    [SerializeField] private CanvasGroup failCanvasGroup;

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

        HideLevelCompletePanel();
        HideFailPanel();
        HideZeroUndoWarningPanelInstant();
    }

    private void SetPanelToTop(GameObject panel)
    {
        if (panel == null) return;

        Canvas c = panel.GetComponent<Canvas>();
        if (c == null) c = panel.AddComponent<Canvas>();

        c.overrideSorting = true;
        c.sortingOrder = 100;

        if (panel.GetComponent<GraphicRaycaster>() == null)
        {
            panel.AddComponent<GraphicRaycaster>();
        }
    }

    public void UpdateLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = "LEVEL " + level.ToString();
        }
    }

    public void ShowLevelCompletePanel(int bonusAmount)
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);

            if (bonusText != null)
            {
                bonusText.text = $"+{bonusAmount} Gem";
            }

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.DOFade(1f, 0.3f);
            }
        }
    }

    public void HideLevelCompletePanel()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    public void ShowFailPanel()
    {
        if (failPanel != null)
        {
            failPanel.SetActive(true);
            if (failCanvasGroup != null)
            {
                failCanvasGroup.alpha = 0f;
                failCanvasGroup.DOFade(1f, 0.3f);
            }
        }
    }

    public void HideFailPanel()
    {
        if (failPanel != null)
        {
            failPanel.SetActive(false);
        }
    }

    public void OnNextLevelClicked()
    {
        HideLevelCompletePanel();
        if (LevelManager.Instance != null) LevelManager.Instance.NextLevel();
    }

    public void OnRestartLevelClicked()
    {
        HideFailPanel();
        if (LevelManager.Instance != null) LevelManager.Instance.RestartLevel();
    }

    public void ShowZeroUndoWarningPanel()
    {
        if (zeroUndoWarningPanel != null)
        {
            zeroUndoWarningPanel.SetActive(true);
            if (zeroUndoCanvasGroup != null)
            {
                zeroUndoCanvasGroup.alpha = 0f;
                zeroUndoCanvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
            }
        }
    }

    public void HideZeroUndoWarningPanel()
    {
        if (zeroUndoWarningPanel != null)
        {
            if (zeroUndoCanvasGroup != null)
            {
                zeroUndoCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() => zeroUndoWarningPanel.SetActive(false));
            }
            else
            {
                zeroUndoWarningPanel.SetActive(false);
            }
        }
    }

    private void HideZeroUndoWarningPanelInstant()
    {
        if (zeroUndoWarningPanel != null)
        {
            zeroUndoWarningPanel.SetActive(false);
            if (zeroUndoCanvasGroup != null)
            {
                zeroUndoCanvasGroup.alpha = 0f;
            }
        }
    }

    public void OnGoToStoreClicked()
    {
        HideZeroUndoWarningPanel();
    }
}