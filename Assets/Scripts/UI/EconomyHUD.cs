using UnityEngine;
using TMPro;

public class EconomyHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI undoButtonText;
    [SerializeField] private TextMeshProUGUI goldAmountText;

    private void Start()
    {
        UpdateUI();
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnEconomyChanged += UpdateUI;
        }
    }

    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnEconomyChanged -= UpdateUI;
        }
    }

    private void UpdateUI()
    {
        if (undoButtonText != null)
        {
            undoButtonText.text = $"Undo: {GameUtils.FormatNumber(EconomyManager.Instance.UndoCount)}";
        }

        if (goldAmountText != null)
        {
            goldAmountText.text = GameUtils.FormatNumber(EconomyManager.Instance.GoldCount);
        }
    }
}