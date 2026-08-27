using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameView : MonoBehaviour
{
    public bool IsAnimating { get; private set; }

    [SerializeField] private HudController hudController;
    [SerializeField] private GameObject highlightPrefab;

    private GameState state;
    private Dictionary<Vector2Int, BoxView> boxViews;
    private RectTransform playerView;

    private float minX;
    private float maxY;
    private const float ANIM_DURATION = 0.08f;
    private int currentBestScore;

    private List<GameObject> activeHighlights = new List<GameObject>();
    private List<GameObject> backgroundGrid = new List<GameObject>();
    private Coroutine solveRoutine;

    public void Initialize(LevelDataPayload payload, int bestScore)
    {
        IsAnimating = false;
        if (solveRoutine != null) { StopCoroutine(solveRoutine); solveRoutine = null; }

        this.state = payload.State;
        this.playerView = payload.PlayerView;
        this.minX = payload.MinX;
        this.maxY = payload.MaxY;
        this.currentBestScore = bestScore;

        boxViews = new Dictionary<Vector2Int, BoxView>();

        foreach (var kvp in payload.BoxViews)
        {
            BoxView bv = kvp.Value.GetComponent<BoxView>();
            boxViews.Add(kvp.Key, bv);
        }

        UpdateAllBoxVisuals();
        ClearHighlights();
        CreateBackgroundGrid();

        if (hudController != null)
        {
            hudController.SetDeadlockWarning(false);
            hudController.UpdateCounters(this.state.MoveCount, this.state.PushCount, currentBestScore);
        }
    }

    private void CreateBackgroundGrid()
    {
        foreach (var bg in backgroundGrid) Destroy(bg);
        backgroundGrid.Clear();

        int cols = state.StaticGrid.GetLength(0);
        int rows = state.StaticGrid.GetLength(1);
        float c = GridGeometry.CELL_SIZE;

        float totalWidth = cols * c;
        float totalHeight = rows * c;

        Vector2 topLeftCellCenter = GridGeometry.GridToWorld(0, 0, minX, maxY);
        Vector2 gridTopLeftEdge = topLeftCellCenter + new Vector2(-c / 2f, c / 2f);

        GameObject gridRoot = new GameObject("ProceduralGridLines");
        gridRoot.transform.SetParent(playerView.parent);
        gridRoot.transform.SetAsFirstSibling(); 

        RectTransform rootRt = gridRoot.AddComponent<RectTransform>();
        rootRt.anchoredPosition = Vector2.zero;
        rootRt.localScale = Vector3.one;
        backgroundGrid.Add(gridRoot);

        Color lineColor = new Color(0f, 0f, 0f, 0.15f);
        float lineThickness = 2.5f; 

        for (int x = 0; x <= cols; x++)
        {
            GameObject vLine = new GameObject($"VLine_{x}");
            vLine.transform.SetParent(gridRoot.transform);
            RectTransform rt = vLine.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(gridTopLeftEdge.x + (x * c), gridTopLeftEdge.y - (totalHeight / 2f));
            rt.sizeDelta = new Vector2(lineThickness, totalHeight);
            rt.localScale = Vector3.one;

            UnityEngine.UI.Image img = vLine.AddComponent<UnityEngine.UI.Image>();
            img.color = lineColor;
        }

        for (int y = 0; y <= rows; y++)
        {
            GameObject hLine = new GameObject($"HLine_{y}");
            hLine.transform.SetParent(gridRoot.transform);
            RectTransform rt = hLine.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(gridTopLeftEdge.x + (totalWidth / 2f), gridTopLeftEdge.y - (y * c));
            rt.sizeDelta = new Vector2(totalWidth, lineThickness);
            rt.localScale = Vector3.one;

            UnityEngine.UI.Image img = hLine.AddComponent<UnityEngine.UI.Image>();
            img.color = lineColor;
        }
    }

    public void HandleMove(Vector2Int dir)
    {
        if (RuntimeLevelEditor.IsEditorActive || state == null) return;
        if (IsAnimating || solveRoutine != null) return;

        Vector2Int playerOldPos = state.Player;
        Vector2Int targetPos = playerOldPos + dir;
        bool isBoxPush = state.Boxes.Contains(targetPos);

        if (state.TryMove(dir))
        {
            ClearHighlights();
            if (hudController != null)
            {
                hudController.SetDeadlockWarning(false);
                hudController.UpdateCounters(state.MoveCount, state.PushCount, currentBestScore);
            }
            StartCoroutine(AnimateMove(playerOldPos, dir, isBoxPush));

            LogMoveEvent(dir);
            if (isBoxPush && AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("box_pushed");
        }
    }

    private void LogMoveEvent(Vector2Int dir)
    {
        if (AnalyticsManager.Instance == null) return;

        string eventName = "move_unknown";
        if (dir.x > 0) eventName = "move_right";
        else if (dir.x < 0) eventName = "move_left";
        else if (dir.y > 0) eventName = "move_down";
        else if (dir.y < 0) eventName = "move_up";

        AnalyticsManager.Instance.LogAction(eventName);
    }

    public void HandleUndo()
    {
        if (RuntimeLevelEditor.IsEditorActive || state == null) return;
        if (IsAnimating || solveRoutine != null) return;

        if (state.TryUndo(out MoveRecord record, out Vector2Int prevPlayer))
        {
            ClearHighlights();
            if (hudController != null)
            {
                hudController.SetDeadlockWarning(state.IsDeadlocked());
                hudController.UpdateCounters(state.MoveCount, state.PushCount, currentBestScore);
            }
            StartCoroutine(AnimateUndo(prevPlayer, record));

            if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("undo_used");
        }
    }

    public void SyncVisualsInstantly(int bestScore)
    {
        if (RuntimeLevelEditor.IsEditorActive || state == null) return;

        if (solveRoutine != null) StopCoroutine(solveRoutine);
        solveRoutine = null;
        this.currentBestScore = bestScore;

        Vector2Int pPos = state.Player;
        playerView.anchoredPosition = GridGeometry.GridToWorld(pPos.x, pPos.y, minX, maxY);

        Dictionary<Vector2Int, BoxView> newBoxViews = new Dictionary<Vector2Int, BoxView>();
        Queue<BoxView> unassignedViews = new Queue<BoxView>(boxViews.Values);

        foreach (var boxPos in state.Boxes)
        {
            BoxView bv = unassignedViews.Dequeue();
            bv.GetComponent<RectTransform>().anchoredPosition = GridGeometry.GridToWorld(boxPos.x, boxPos.y, minX, maxY);
            newBoxViews.Add(boxPos, bv);
        }

        boxViews = newBoxViews;
        UpdateAllBoxVisuals();
        ClearHighlights();

        if (hudController != null)
        {
            hudController.SetDeadlockWarning(false);
            hudController.UpdateCounters(state.MoveCount, state.PushCount, currentBestScore);
        }
    }

    public void ToggleFloodFill()
    {
        if (RuntimeLevelEditor.IsEditorActive || state == null) return;
        if (IsAnimating) return;

        if (activeHighlights.Count > 0)
        {
            ClearHighlights();
            return;
        }

        if (highlightPrefab == null) return;

        HashSet<Vector2Int> reachable = state.GetReachableCells();
        foreach (var cell in reachable)
        {
            GameObject hl = Instantiate(highlightPrefab, playerView.parent);
            RectTransform rt = hl.GetComponent<RectTransform>();
            rt.anchoredPosition = GridGeometry.GridToWorld(cell.x, cell.y, minX, maxY);
            rt.SetAsFirstSibling();
            activeHighlights.Add(hl);
        }

        if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("floodfill_used");
    }

    public void AutoSolve()
    {
        if (RuntimeLevelEditor.IsEditorActive || state == null) return;
        if (IsAnimating || solveRoutine != null || state.IsSolved()) return;

        if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("autosolve_started");
        solveRoutine = StartCoroutine(SolveRoutine());
    }

    private IEnumerator SolveRoutine()
    {
        List<Vector2Int> path = SokobanSolver.Solve(state);

        if (path == null || path.Count == 0)
        {
            if (hudController != null) hudController.SetDeadlockWarning(true);
            solveRoutine = null;
            yield break;
        }

        if (hudController != null) hudController.SetDeadlockWarning(false);

        foreach (Vector2Int dir in path)
        {
            Vector2Int playerOldPos = state.Player;
            Vector2Int targetPos = playerOldPos + dir;
            bool isBoxPush = state.Boxes.Contains(targetPos);

            state.TryMove(dir);
            if (hudController != null) hudController.UpdateCounters(state.MoveCount, state.PushCount, currentBestScore);

            yield return StartCoroutine(AnimateMove(playerOldPos, dir, isBoxPush));
        }
        solveRoutine = null;
    }

    private void ClearHighlights()
    {
        foreach (var hl in activeHighlights) Destroy(hl);
        activeHighlights.Clear();
    }

    private IEnumerator AnimateMove(Vector2Int oldPlayerPos, Vector2Int dir, bool pushedBox)
    {
        IsAnimating = true;

        Vector2Int newPlayerPos = state.Player;
        Vector2 playerStart = playerView.anchoredPosition;
        Vector2 playerEnd = GridGeometry.GridToWorld(newPlayerPos.x, newPlayerPos.y, minX, maxY);

        BoxView boxView = null;
        Vector2 boxStart = Vector2.zero;
        Vector2 boxEnd = Vector2.zero;
        Vector2Int newBoxPos = Vector2Int.zero;

        if (pushedBox)
        {
            Vector2Int oldBoxPos = oldPlayerPos + dir;
            newBoxPos = newPlayerPos + dir;

            boxView = boxViews[oldBoxPos];
            boxViews.Remove(oldBoxPos);
            boxViews.Add(newBoxPos, boxView);

            boxStart = boxView.GetComponent<RectTransform>().anchoredPosition;
            boxEnd = GridGeometry.GridToWorld(newBoxPos.x, newBoxPos.y, minX, maxY);
        }

        float elapsed = 0f;
        while (elapsed < ANIM_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ANIM_DURATION;

            playerView.anchoredPosition = Vector2.Lerp(playerStart, playerEnd, t);
            if (pushedBox)
            {
                boxView.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(boxStart, boxEnd, t);
            }
            yield return null;
        }

        playerView.anchoredPosition = playerEnd;
        if (pushedBox)
        {
            boxView.GetComponent<RectTransform>().anchoredPosition = boxEnd;
            boxView.SetOnGoal(state.IsOnGoal(newBoxPos));
        }

        IsAnimating = false;

        if (state.IsSolved())
        {
            if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("level_completed");
            LevelManager.Instance.LevelCompleted();
        }
        else if (state.IsDeadlocked())
        {
            if (hudController != null) hudController.SetDeadlockWarning(true);
            if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("deadlock_reached");
        }
    }

    private IEnumerator AnimateUndo(Vector2Int targetPlayerPos, MoveRecord record)
    {
        IsAnimating = true;

        Vector2 playerStart = playerView.anchoredPosition;
        Vector2 playerEnd = GridGeometry.GridToWorld(targetPlayerPos.x, targetPlayerPos.y, minX, maxY);

        BoxView boxView = null;
        Vector2 boxStart = Vector2.zero;
        Vector2 boxEnd = Vector2.zero;
        Vector2Int targetBoxPos = Vector2Int.zero;

        if (record.pushed)
        {
            Vector2Int currentBoxPos = targetPlayerPos + (record.dir * 2);
            targetBoxPos = targetPlayerPos + record.dir;

            boxView = boxViews[currentBoxPos];
            boxViews.Remove(currentBoxPos);
            boxViews.Add(targetBoxPos, boxView);

            boxStart = boxView.GetComponent<RectTransform>().anchoredPosition;
            boxEnd = GridGeometry.GridToWorld(targetBoxPos.x, targetBoxPos.y, minX, maxY);
        }

        float elapsed = 0f;
        while (elapsed < ANIM_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / ANIM_DURATION;

            playerView.anchoredPosition = Vector2.Lerp(playerStart, playerEnd, t);
            if (record.pushed)
            {
                boxView.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(boxStart, boxEnd, t);
            }
            yield return null;
        }

        playerView.anchoredPosition = playerEnd;
        if (record.pushed)
        {
            boxView.GetComponent<RectTransform>().anchoredPosition = boxEnd;
            boxView.SetOnGoal(state.IsOnGoal(targetBoxPos));
        }

        IsAnimating = false;
    }

    private void UpdateAllBoxVisuals()
    {
        foreach (var kvp in boxViews)
        {
            kvp.Value.SetOnGoal(state.IsOnGoal(kvp.Key));
        }
    }
}