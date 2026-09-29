using UnityEngine;
using UnityEngine.UI;

public class StorePopupController : MonoBehaviour
{
    public static StorePopupController Instance { get; private set; }

    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Button watchAdButton;
    [SerializeField] private Button buyWithGoldButton;
    [SerializeField] private Button buyGoldIAPButton;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (watchAdButton != null) watchAdButton.onClick.AddListener(OnWatchAdClicked);
        if (buyWithGoldButton != null) buyWithGoldButton.onClick.AddListener(OnBuyWithGoldClicked);
        if (buyGoldIAPButton != null) buyGoldIAPButton.onClick.AddListener(OnBuyGoldIAPClicked);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePopup);

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    private void Start()
    {
        SetPanelToTop(popupPanel);
    }

    private void SetPanelToTop(GameObject panel)
    {
        if (panel == null) return;

        Canvas c = panel.GetComponent<Canvas>();
        if (c == null) c = panel.AddComponent<Canvas>();

        c.overrideSorting = true;
        c.sortingOrder = 105;

        if (panel.GetComponent<GraphicRaycaster>() == null)
        {
            panel.AddComponent<GraphicRaycaster>();
        }
    }

    public void ShowPopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    public void ClosePopup()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCoreSFX(SoundManager.Instance.uiClickSound);

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    private void OnWatchAdClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCoreSFX(SoundManager.Instance.uiClickSound);
        if (AdManager.Instance == null) return;

        AdManager.Instance.ShowRewardedAd(() =>
        {
            if (EconomyManager.Instance != null) EconomyManager.Instance.AddUndo(3);
            ClosePopup();
        });
    }

    private void OnBuyWithGoldClicked()
    {
        if (EconomyManager.Instance == null) return;

        if (EconomyManager.Instance.TrySpendGold(300))
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayCoreSFX(SoundManager.Instance.levelCompleteSound);
            EconomyManager.Instance.AddUndo(3);
            ClosePopup();
        }
        else
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayCoreSFX(SoundManager.Instance.errorSound);
        }
    }

    private void OnBuyGoldIAPClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCoreSFX(SoundManager.Instance.uiClickSound);
        if (IAPManager.Instance != null) IAPManager.Instance.Buy100Gold();
    }
}