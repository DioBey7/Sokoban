using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance { get; private set; }

    public GameObject mainMenuPanel;
    [SerializeField] private Button playButton;

    [Header("Best Moves UI")]
    [SerializeField] private GameObject bestMovesPanel;
    [SerializeField] private CanvasGroup bestMovesCanvasGroup;
    [SerializeField] private Transform bestMovesWindow;
    [SerializeField] private TextMeshProUGUI bestMovesText;

    [SerializeField] private Button openBestMovesButton;
    [SerializeField] private Button closeBestMovesButton;

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
        if (playButton != null) playButton.onClick.AddListener(StartGame);
        if (openBestMovesButton != null) openBestMovesButton.onClick.AddListener(OpenBestMovesPanel);
        if (closeBestMovesButton != null) closeBestMovesButton.onClick.AddListener(CloseBestMovesPanel);

        if (bestMovesPanel != null) bestMovesPanel.SetActive(false);
    }

    public void StartGame()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCoreSFX(SoundManager.Instance.uiClickSound);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (LevelManager.Instance != null) LevelManager.Instance.StartGame();
    }

    public void OpenBestMovesPanel()
    {
        if (bestMovesPanel == null) return;

        if (SoundManager.Instance != null) SoundManager.Instance.PlayCoreSFX(SoundManager.Instance.uiClickSound);

        bestMovesPanel.SetActive(true);
        PopulateBestMoves();

        if (bestMovesCanvasGroup != null)
        {
            bestMovesCanvasGroup.DOKill();
            bestMovesCanvasGroup.alpha = 0f;
            bestMovesCanvasGroup.DOFade(1f, 0.3f);
        }

        if (bestMovesWindow != null)
        {
            bestMovesWindow.DOKill();
            bestMovesWindow.localScale = Vector3.one * 0.8f;
            bestMovesWindow.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }
    }

    public void CloseBestMovesPanel()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCoreSFX(SoundManager.Instance.uiClickSound);

        if (bestMovesWindow != null)
        {
            bestMovesWindow.DOKill();
            bestMovesWindow.DOScale(Vector3.one * 0.8f, 0.2f).SetEase(Ease.InBack);
        }

        if (bestMovesCanvasGroup != null)
        {
            bestMovesCanvasGroup.DOKill();
            bestMovesCanvasGroup.DOFade(0f, 0.2f).OnComplete(() => bestMovesPanel.SetActive(false));
        }
        else
        {
            if (bestMovesPanel != null) bestMovesPanel.SetActive(false);
        }
    }

    private void PopulateBestMoves()
    {
        if (bestMovesText == null || DataManager.Instance == null) return;

        string content = "<color=#FFD700>BEST MOVES</color>\n\n";
        bool hasData = false;

        for (int i = 0; i < 50; i++)
        {
            int best = DataManager.Instance.GetBestMove(i);
            if (best > 0)
            {
                content += $"Level {i + 1}: <color=#00FF00>{best}</color> Move\n";
                hasData = true;
            }
        }

        if (!hasData)
        {
            content += "No completed levels yet.";
        }

        bestMovesText.text = content;
    }
}