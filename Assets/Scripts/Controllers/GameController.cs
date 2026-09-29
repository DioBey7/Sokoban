using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [SerializeField] private GameView gameView;
    [SerializeField] private HudController hudController;

    private GameMechanicsConfig currentConfig;
    private int currentLevelIndex;

    private GameState currentState;
    private Coroutine solveRoutine;
    private Vector2Int lastMoveDirection;

    private int currentMaxMoves;
    private int currentLevelReward;
    private float currentMoveDuration = 0.15f;
    private float currentIceSlideDuration = 0.10f;
    private float portalRejectDuration = 0.3f;

    private bool isUndoing = false;
    private bool wasLastMoveRejected = false;

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
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnMoveInput += HandleMovePlayerInput;
        }
    }

    private void OnDestroy()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnMoveInput -= HandleMovePlayerInput;
        }
    }

    public void InitializeLevel(Transform levelRoot, GameMechanicsConfig config, int levelIndex)
    {
        this.currentConfig = config;
        this.currentLevelIndex = levelIndex;
        this.isUndoing = false;
        this.wasLastMoveRejected = false;

        if (solveRoutine != null)
        {
            StopCoroutine(solveRoutine);
            solveRoutine = null;
        }

        if (InputHandler.Instance != null) InputHandler.Instance.IsInputLocked = false;

        LevelDataPayload payload = LevelScanner.Scan(levelRoot, config, levelIndex);
        if (payload == null) return;

        currentState = payload.State;
        gameView.InitializeVisuals(payload);

        gameView.SyncDoors(currentState, false);
        gameView.UpdateAllBoxes(currentState.Boxes, currentState.Goals);

        currentMaxMoves = 0;

        if (config != null)
        {
            currentLevelReward = config.defaultCompletionBonus;
            currentMoveDuration = config.defaultMoveDuration;
            currentIceSlideDuration = config.defaultIceSlideDuration;
        }

        if (config != null && levelIndex >= 0 && levelIndex < config.levels.Count)
        {
            currentMaxMoves = config.levels[levelIndex].maxMoves;
            currentLevelReward = config.levels[levelIndex].completionBonusGold;
            currentMoveDuration = config.levels[levelIndex].moveDuration;
            currentIceSlideDuration = config.levels[levelIndex].iceSlideDuration;
        }

        UpdateHUD();
    }

    private void HandleMovePlayerInput(Vector2Int direction)
    {
        HandleMove(direction, false);
    }

    public void HandleMove(Vector2Int direction, bool isIceSlide = false)
    {
        if (gameView.IsAnimating || currentState == null || solveRoutine != null) return;

        lastMoveDirection = direction;
        MoveResult result = currentState.TryMove(direction, out MoveRecord record);

        wasLastMoveRejected = (result == MoveResult.PortalRejected);

        if (result == MoveResult.Success || result == MoveResult.PortalRejected)
        {
            if (result == MoveResult.Success)
            {
                if (record.Box.IsPushed && HapticManager.Instance != null) HapticManager.Instance.PlayWarning();
                LogInteraction(direction, record);
            }

            if (InputHandler.Instance != null) InputHandler.Instance.IsInputLocked = true;

            float duration = isIceSlide ? currentIceSlideDuration : currentMoveDuration;
            gameView.AnimateMove(record, OnAnimationComplete, duration);

            if (result == MoveResult.Success) UpdateHUD();
        }
        else
        {
            if (InputHandler.Instance != null) InputHandler.Instance.IsInputLocked = false;
        }
    }

    private void HandleUndo()
    {
        if (gameView.IsAnimating || currentState == null || solveRoutine != null) return;

        if (currentState.MoveCount <= 0)
        {
            if (HapticManager.Instance != null) HapticManager.Instance.PlayWarning();
            return;
        }

        if (EconomyManager.Instance != null && !EconomyManager.Instance.TryUseUndo())
        {
            if (GameUIManager.Instance != null) GameUIManager.Instance.ShowZeroUndoWarningPanel();
            if (HapticManager.Instance != null) HapticManager.Instance.PlayError();
            return;
        }

        if (currentState.TryUndo(out MoveRecord record))
        {
            isUndoing = true;
            if (InputHandler.Instance != null) InputHandler.Instance.IsInputLocked = true;

            float duration = currentConfig != null ? currentConfig.levels[currentLevelIndex].moveDuration : 0.15f;
            gameView.AnimateUndo(record, OnAnimationComplete, duration);

            UpdateHUD();
        }
    }

    private void LogInteraction(Vector2Int dir, MoveRecord record)
    {
        if (AnalyticsManager.Instance == null) return;

        string dirName = dir == Vector2Int.down ? "up" :
                         dir == Vector2Int.up ? "down" :
                         dir == Vector2Int.left ? "left" : "right";

        AnalyticsManager.Instance.LogActionWithParam("player_move", "direction", dirName);

        if (record.Box.IsPushed)
        {
            string boxType = record.Box.Snapshot.Type.ToString().ToLower();
            AnalyticsManager.Instance.LogActionWithParam("box_push", "type", boxType);

            if (record.Box.IsBroken)
            {
                AnalyticsManager.Instance.LogAction("box_broken");
            }
        }

        Vector2Int pos = record.Player.EndPos;

        if (currentState.StaticGrid != null && pos.x >= 0 && pos.x < currentState.StaticGrid.GetLength(0) && pos.y >= 0 && pos.y < currentState.StaticGrid.GetLength(1))
        {
            if (currentState.StaticGrid[pos.x, pos.y] == StaticElement.Ice)
            {
                AnalyticsManager.Instance.LogAction("ice_slide");
            }
        }

        if (currentState.Switches != null && currentState.Switches.TryGetValue(pos, out int switchId))
        {
            AnalyticsManager.Instance.LogActionWithParam("switch_press", "link_id", switchId.ToString());
        }
    }

    public void OnAnimationComplete()
    {
        if (currentState != null)
        {
            gameView.SyncDoors(currentState, true);
            gameView.UpdateAllBoxes(currentState.Boxes, currentState.Goals);
        }

        bool skipIceSlide = isUndoing || wasLastMoveRejected;

        isUndoing = false;
        wasLastMoveRejected = false;

        if (InputHandler.Instance != null) InputHandler.Instance.IsInputLocked = false;

        if (currentState.IsSolved())
        {
            if (InputHandler.Instance != null) InputHandler.Instance.IsInputLocked = true;

            if (DataManager.Instance != null)
            {
                DataManager.Instance.SaveBestMove(currentLevelIndex, currentState.MoveCount);
            }

            int bonus = currentConfig != null ? currentConfig.levels[currentLevelIndex].completionBonusGold : 100;
            if (EconomyManager.Instance != null) EconomyManager.Instance.AddGold(bonus);
            if (GameUIManager.Instance != null) GameUIManager.Instance.ShowLevelCompletePanel(bonus);
            if (HapticManager.Instance != null) HapticManager.Instance.PlaySuccess();
            if (SoundManager.Instance != null) SoundManager.Instance.PlayCoreSFX(SoundManager.Instance.levelCompleteSound);

            return;
        }

        int maxLimit = currentConfig != null ? currentConfig.levels[currentLevelIndex].maxMoves : 20;

        if (currentState.MoveCount >= maxLimit)
        {
            if (InputHandler.Instance != null) InputHandler.Instance.IsInputLocked = true;
            if (GameUIManager.Instance != null) GameUIManager.Instance.ShowFailPanel();
            if (HapticManager.Instance != null) HapticManager.Instance.PlayError();
            if (SoundManager.Instance != null) SoundManager.Instance.PlayCoreSFX(SoundManager.Instance.errorSound);
            return;
        }

        if (!skipIceSlide && currentState != null)
        {
            Vector2Int p = currentState.Player;
            if (currentState.StaticGrid != null &&
                p.x >= 0 && p.x < currentState.StaticGrid.GetLength(0) &&
                p.y >= 0 && p.y < currentState.StaticGrid.GetLength(1))
            {
                if (currentState.StaticGrid[p.x, p.y] == StaticElement.Ice)
                {
                    if (InputHandler.Instance != null) InputHandler.Instance.IsInputLocked = true;
                    HandleMove(lastMoveDirection, true);
                }
            }
        }
    }

    public void AutoSolve()
    {
        if (gameView.IsAnimating || currentState == null || solveRoutine != null || currentState.IsSolved()) return;
        if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("autosolve_used");
        solveRoutine = StartCoroutine(SolveRoutine());
    }

    private IEnumerator SolveRoutine()
    {
        List<Vector2Int> path = SokobanSolver.Solve(currentState);
        if (path == null || path.Count == 0)
        {
            if (hudController != null) hudController.SetDeadlockWarning(true);
            if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("autosolve_deadlock");
            solveRoutine = null;
            yield break;
        }

        if (InputHandler.Instance != null) InputHandler.Instance.IsInputLocked = true;
        if (hudController != null) hudController.SetDeadlockWarning(false);

        foreach (Vector2Int dir in path)
        {
            if (currentState.TryMove(dir, out MoveRecord record) == MoveResult.Success)
            {
                bool isAnimating = true;
                UpdateHUD();

                gameView.AnimateMove(record, () => isAnimating = false, currentMoveDuration);
                yield return new WaitWhile(() => isAnimating);

                gameView.SyncDoors(currentState, true);
                gameView.UpdateAllBoxes(currentState.Boxes, currentState.Goals);
            }
        }

        if (currentState.IsSolved())
        {
            if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("level_complete_autosolve");
            if (EconomyManager.Instance != null && currentLevelReward > 0) EconomyManager.Instance.AddGold(currentLevelReward);
            if (LevelManager.Instance != null) LevelManager.Instance.NextLevel();
        }

        if (InputHandler.Instance != null) InputHandler.Instance.IsInputLocked = false;
        solveRoutine = null;
    }

    private void UpdateHUD()
    {
        if (hudController != null && currentState != null)
        {
            int bestMove = 0;
            if (DataManager.Instance != null)
            {
                bestMove = DataManager.Instance.GetBestMove(currentLevelIndex);
            }

            hudController.UpdateCounters(currentState.MoveCount, currentState.PushCount, bestMove, currentMaxMoves);
        }
    }

    public void OnClickUndo() => HandleUndo();

    public void OnClickReset()
    {
        if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("level_reset");
        if (LevelManager.Instance != null) LevelManager.Instance.RestartLevel();
    }

    public void OnClickStore()
    {
        if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("store_opened");
        if (StorePopupController.Instance != null) StorePopupController.Instance.ShowPopup();
    }
}