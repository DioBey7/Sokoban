using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FailPopupController : MonoBehaviour
{
    public static FailPopupController Instance { get; private set; }

    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private CanvasGroup failPanelCanvasGroup;

    public bool IsVisible => popupPanel != null && popupPanel.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        HidePopup(); 
    }

    public void ShowPopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);

            if (failPanelCanvasGroup != null)
            {
                failPanelCanvasGroup.alpha = 0f;
                failPanelCanvasGroup.DOFade(1f, 0.3f);
            }

        }
    }

    public void HidePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);

            if (failPanelCanvasGroup != null)
            {
                failPanelCanvasGroup.DOFade(0f, 0.3f);
            }
        }
    }

    private void OnRetryClicked()
    {
        HidePopup();
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartLevel();
        }
    }
}