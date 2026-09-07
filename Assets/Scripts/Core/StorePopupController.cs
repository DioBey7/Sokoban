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

    public void ShowPopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    public void ClosePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    private void OnWatchAdClicked()
    {
        AdManager.Instance.ShowRewardedAd(() =>
        {
            EconomyManager.Instance.AddUndo(3);
            ClosePopup();
        });
    }

    private void OnBuyWithGoldClicked()
    {
        if (EconomyManager.Instance.TrySpendGold(300))
        {
            EconomyManager.Instance.AddUndo(3);
            ClosePopup();
        }
    }

    private void OnBuyGoldIAPClicked()
    {
        IAPManager.Instance.Buy100Gold();
    }
}