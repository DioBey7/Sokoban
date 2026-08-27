using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RuntimeLevelEditor : MonoBehaviour
{
    public static RuntimeLevelEditor Instance { get; private set; }
    public static bool IsEditorActive = false;

    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject goalPrefab;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private Transform editorRoot;
    [SerializeField] private GameObject editorUIPanel;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject swipeZone;

    private CellKind currentBrush = CellKind.Wall;
    private bool isEraserMode = false;
    private Camera mainCam;
    private Coroutine editorRoutine;
    private List<GameObject> userDrawnObjects = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
        IsEditorActive = false;
        if (editorUIPanel != null) editorUIPanel.SetActive(false);
    }

    public void ToggleEditor()
    {
        IsEditorActive = !IsEditorActive;
        if (editorUIPanel != null) editorUIPanel.SetActive(IsEditorActive);

        if (swipeZone != null) swipeZone.SetActive(!IsEditorActive);

        UpdateStatusText();

        if (IsEditorActive)
        {
            if (editorRoutine == null) editorRoutine = StartCoroutine(EditorRoutine());
            if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("editor_opened");
        }
        else
        {
            if (editorRoutine != null)
            {
                StopCoroutine(editorRoutine);
                editorRoutine = null;
            }

            if (LevelManager.Instance != null && editorRoot != null)
            {
                LevelManager.Instance.ReloadFromEditor(editorRoot);
            }
        }
    }

    private System.Collections.IEnumerator EditorRoutine()
    {
        while (IsEditorActive)
        {
            HandleMouseInput();
            yield return null;
        }
    }

    public void SetBrush(int kindIndex)
    {
        if (kindIndex == -1)
        {
            isEraserMode = true;
        }
        else
        {
            isEraserMode = false;
            currentBrush = (CellKind)kindIndex;
        }
        UpdateStatusText();
    }

    public void ClearLevel()
    {
        foreach (var obj in userDrawnObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Destroy(obj);
            }
        }
        userDrawnObjects.Clear();

        if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("editor_level_cleared");
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            statusText.text = isEraserMode ? "Brush: ERASER" : $"Brush: {currentBrush.ToString()}";
        }
    }

    private bool IsPointerOverEditorUI()
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (editorUIPanel != null && result.gameObject.transform.IsChildOf(editorUIPanel.transform)) return true;
            if (result.gameObject.GetComponent<Button>() != null) return true;
        }
        return false;
    }

    private void HandleMouseInput()
    {
        if (IsPointerOverEditorUI()) return;

        if (Input.GetMouseButton(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Began)))
        {
            if (isEraserMode) RemoveObject();
            else PlaceObject();
        }
    }

    private Vector2Int GetGridPositionFromMouse()
    {
        Canvas canvas = editorRoot.GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : mainCam;

        Vector2 inputPos = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(editorRoot as RectTransform, inputPos, cam, out Vector2 localPoint);

        int col = Mathf.RoundToInt(localPoint.x / GridGeometry.CELL_SIZE);
        int row = Mathf.RoundToInt(localPoint.y / GridGeometry.CELL_SIZE);
        return new Vector2Int(col, row);
    }

    private void PlaceObject()
    {
        if (editorRoot == null) return;
        Vector2Int gridPos = GetGridPositionFromMouse();
        GridObject[] allObjects = editorRoot.GetComponentsInChildren<GridObject>();

        if (currentBrush == CellKind.Player)
        {
            foreach (var obj in allObjects)
            {
                if (obj == null) continue;

                if (obj.kind == CellKind.Player)
                {
                    obj.gameObject.SetActive(false);
                    Destroy(obj.gameObject);
                }
            }
        }

        bool isPlacingDynamic = (currentBrush == CellKind.Player || currentBrush == CellKind.Box);

        foreach (var obj in allObjects)
        {
            if (obj == null || !obj.gameObject.activeSelf) continue;

            Vector3 localPos = editorRoot.InverseTransformPoint(obj.transform.position);
            int col = Mathf.RoundToInt(localPos.x / GridGeometry.CELL_SIZE);
            int row = Mathf.RoundToInt(localPos.y / GridGeometry.CELL_SIZE);

            if (col == gridPos.x && row == gridPos.y)
            {
                if (obj.kind == currentBrush) return;

                if (isPlacingDynamic && obj.kind == CellKind.Goal) continue;

                obj.gameObject.SetActive(false);
                Destroy(obj.gameObject);
            }
        }

        GameObject prefab = GetPrefabForBrush();
        if (prefab != null)
        {
            GameObject newObj = Instantiate(prefab, editorRoot);
            RectTransform rect = newObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(gridPos.x * GridGeometry.CELL_SIZE, gridPos.y * GridGeometry.CELL_SIZE);
            }
            userDrawnObjects.Add(newObj);

            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.LogActionWithParam("obstacle_placed", "type", currentBrush.ToString().ToLower());
            }
        }
    }

    private void RemoveObject()
    {
        if (editorRoot == null) return;
        Vector2Int gridPos = GetGridPositionFromMouse();
        GridObject[] allObjects = editorRoot.GetComponentsInChildren<GridObject>();

        GridObject objectToDelete = null;

        foreach (var obj in allObjects)
        {
            if (obj == null || !obj.gameObject.activeSelf) continue;

            Vector3 localPos = editorRoot.InverseTransformPoint(obj.transform.position);
            int col = Mathf.RoundToInt(localPos.x / GridGeometry.CELL_SIZE);
            int row = Mathf.RoundToInt(localPos.y / GridGeometry.CELL_SIZE);

            if (col == gridPos.x && row == gridPos.y)
            {
                if (obj.kind == CellKind.Player || obj.kind == CellKind.Box)
                {
                    objectToDelete = obj;
                    break;
                }
                objectToDelete = obj;
            }
        }

        if (objectToDelete != null)
        {
            objectToDelete.gameObject.SetActive(false);
            Destroy(objectToDelete.gameObject);

            if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("object_removed");
        }
    }

    private GameObject GetPrefabForBrush()
    {
        switch (currentBrush)
        {
            case CellKind.Player: return playerPrefab;
            case CellKind.Wall: return wallPrefab;
            case CellKind.Box: return boxPrefab;
            case CellKind.Goal: return goalPrefab;
            default: return null;
        }
    }

    public void SaveLevelAsPrefab()
    {
#if UNITY_EDITOR
        if (editorRoot == null || editorRoot.childCount == 0) return;

        string defaultDir = "Assets/Prefabs/Resources/Levels";
        string path = EditorUtility.SaveFilePanelInProject("Save Custom Level", "NewCustomLevel", "prefab", "Choose folder to save level prefab.", defaultDir);
        if (string.IsNullOrEmpty(path)) return;

        GameObject tempRoot = new GameObject("CustomLevelRoot");
        tempRoot.AddComponent<LevelRoot>();

        GameObject staticRoot = new GameObject("Static");
        GameObject dynamicRoot = new GameObject("Dynamic");
        staticRoot.transform.SetParent(tempRoot.transform);
        dynamicRoot.transform.SetParent(tempRoot.transform);

        GridObject[] allObjects = editorRoot.GetComponentsInChildren<GridObject>();

        foreach (var gridObj in allObjects)
        {
            if (gridObj == null) continue;

            CellKind kind = gridObj.kind;
            Transform parent = (kind == CellKind.Wall || kind == CellKind.Goal) ? staticRoot.transform : dynamicRoot.transform;

            GameObject clone = Instantiate(gridObj.gameObject, parent);
            clone.name = clone.name.Replace("(Clone)", "");

            RectTransform originalRect = gridObj.GetComponent<RectTransform>();
            RectTransform cloneRect = clone.GetComponent<RectTransform>();
            if (originalRect != null && cloneRect != null)
            {
                cloneRect.anchoredPosition = originalRect.anchoredPosition;
                cloneRect.sizeDelta = originalRect.sizeDelta;
            }
        }

        PrefabUtility.SaveAsPrefabAsset(tempRoot, path);
        if (Application.isPlaying) Destroy(tempRoot);
        else DestroyImmediate(tempRoot);

        if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.LogAction("editor_level_saved");
#endif
    }
}