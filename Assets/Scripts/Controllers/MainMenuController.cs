using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button playButton;

    private void Start()
    {
        if (playButton != null) playButton.onClick.AddListener(StartGame);
    }

    public void StartGame()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (LevelManager.Instance != null) LevelManager.Instance.StartGame();
    }
}