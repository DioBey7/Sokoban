using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private GameObject[] levelPrefabs;
    [SerializeField] private GameView gameView;
    [SerializeField] private RectTransform levelContainer;

    private GameObject currentLevelInstance;
    public GameState CurrentState { get; private set; }

    private Vector2Int initialPlayerPos;
    private HashSet<Vector2Int> initialBoxes;
    private int currentLevelIndex = 0;

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
        StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        if (index < 0 || index >= levelPrefabs.Length) return;

        currentLevelIndex = index;

        if (RuntimeLevelEditor.Instance != null)
        {
            RuntimeLevelEditor.Instance.ClearLevel();
        }

        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }

        currentLevelInstance = Instantiate(levelPrefabs[currentLevelIndex], levelContainer);
        LevelDataPayload payload = LevelScanner.Scan(currentLevelInstance.transform);

        if (payload != null)
        {
            CurrentState = payload.State;
            initialPlayerPos = CurrentState.Player;
            initialBoxes = new HashSet<Vector2Int>(CurrentState.Boxes);

            int bestScore = PlayerPrefs.GetInt($"Level_{currentLevelIndex}_BestScore", int.MaxValue);
            gameView.Initialize(payload, bestScore);

            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.LogActionWithParam("level_loaded", "level_index", currentLevelIndex.ToString());
            }
        }
    }

    public void RestartLevel()
    {
        if (CurrentState == null) return;

        CurrentState.ResetState(initialPlayerPos, new HashSet<Vector2Int>(initialBoxes));
        int bestScore = PlayerPrefs.GetInt($"Level_{currentLevelIndex}_BestScore", int.MaxValue);
        gameView.SyncVisualsInstantly(bestScore);

        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.LogActionWithParam("level_restarted", "level_index", currentLevelIndex.ToString());
        }
    }

    public void LevelCompleted()
    {
        int currentMoves = CurrentState.MoveCount;
        string prefsKey = $"Level_{currentLevelIndex}_BestScore";
        int bestScore = PlayerPrefs.GetInt(prefsKey, int.MaxValue);

        if (currentMoves < bestScore)
        {
            PlayerPrefs.SetInt(prefsKey, currentMoves);
            PlayerPrefs.Save();
        }

        int nextLevelIndex = currentLevelIndex + 1;
        if (nextLevelIndex < levelPrefabs.Length)
        {
            LoadLevel(nextLevelIndex);
        }
    }

    public void ReloadFromEditor(Transform editorRoot)
    {
        StartCoroutine(ReloadRoutine(editorRoot));
    }

    private System.Collections.IEnumerator ReloadRoutine(Transform editorRoot)
    {
        yield return new WaitForEndOfFrame();

        LevelDataPayload payload = LevelScanner.Scan(editorRoot);

        if (payload != null)
        {
            CurrentState = payload.State;
            initialPlayerPos = CurrentState.Player;
            initialBoxes = new HashSet<Vector2Int>(CurrentState.Boxes);

            gameView.Initialize(payload, int.MaxValue);
        }
    }
}