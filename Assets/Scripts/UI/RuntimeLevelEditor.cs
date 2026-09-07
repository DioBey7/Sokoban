using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EditorBrushType { Player, Wall, Box, FragileBox, Goal, Ice, Switch, Door }

public class RuntimeLevelEditor : MonoBehaviour
{
    public static RuntimeLevelEditor Instance { get; private set; }
    public static bool IsEditorActive = false;

    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject goalPrefab;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject icePrefab;
    [SerializeField] private GameObject fragileBoxPrefab;
    [SerializeField] private GameObject switchPrefab;
    [SerializeField] private GameObject doorPrefab;

    [SerializeField] private Transform editorRoot;
    [SerializeField] private GameObject editorUIPanel;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject swipeZone;

    private EditorBrushType currentBrush = EditorBrushType.Wall;
    private bool isEraserMode = false;
    private Camera mainCam;
    private Coroutine editorRoutine;
    private List<GameObject> userDrawnObjects = new List<GameObject>();
    private const float CELL_SIZE = 100f; 

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
        }
        else
        {
            if (editorRoutine != null)
            {
                StopCoroutine(editorRoutine);
                editorRoutine = null;
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
            currentBrush = (EditorBrushType)kindIndex;
        }
        UpdateStatusText();
    }

    public void ClearLevel()
    {
        foreach (var obj in userDrawnObjects)
        {
            if (obj != null) Destroy(obj);
        }
        userDrawnObjects.Clear();
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            statusText.text = isEraserMode ? "Brush: Eraser" : $"Brush: {currentBrush.ToString()}";
        }
    }

    private bool IsPointerOverEditorUI()
    {
        if (EventSystem.current == null) return false;
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
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
        Vector2 inputPos = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(editorRoot as RectTransform, inputPos, null, out Vector2 localPoint);

        int col = Mathf.RoundToInt(localPoint.x / CELL_SIZE);
        int row = Mathf.RoundToInt(localPoint.y / CELL_SIZE);
        return new Vector2Int(col, row);
    }

    private void PlaceObject()
    {
        if (editorRoot == null) return;
        Vector2Int gridPos = GetGridPositionFromMouse();
        GridObject[] allObjects = editorRoot.GetComponentsInChildren<GridObject>(true);

        GameObject prefab = GetPrefabForBrush();
        if (prefab == null) return;

        foreach (var obj in allObjects)
        {
            if (obj == null || !obj.gameObject.activeInHierarchy) continue;
            RectTransform objRect = obj.GetComponent<RectTransform>();
            if (objRect == null) continue;

            int col = Mathf.RoundToInt(objRect.anchoredPosition.x / CELL_SIZE);
            int row = Mathf.RoundToInt(objRect.anchoredPosition.y / CELL_SIZE);

            if (col == gridPos.x && row == gridPos.y)
            {
                Destroy(obj.gameObject);
            }
        }

        GameObject newObj = Instantiate(prefab, editorRoot);
        RectTransform rect = newObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(gridPos.x * CELL_SIZE, gridPos.y * CELL_SIZE);
        }
        userDrawnObjects.Add(newObj);
    }

    private void RemoveObject()
    {
        if (editorRoot == null) return;
        Vector2Int gridPos = GetGridPositionFromMouse();
        Transform[] allTransforms = editorRoot.GetComponentsInChildren<Transform>(true);

        foreach (var t in allTransforms)
        {
            if (t == null || t == editorRoot || !t.gameObject.activeInHierarchy) continue;
            RectTransform objRect = t.GetComponent<RectTransform>();
            if (objRect == null) continue;

            int col = Mathf.RoundToInt(objRect.anchoredPosition.x / CELL_SIZE);
            int row = Mathf.RoundToInt(objRect.anchoredPosition.y / CELL_SIZE);

            if (col == gridPos.x && row == gridPos.y)
            {
                Destroy(t.gameObject);
            }
        }
    }

    private GameObject GetPrefabForBrush()
    {
        switch (currentBrush)
        {
            case EditorBrushType.Player: return playerPrefab;
            case EditorBrushType.Wall: return wallPrefab;
            case EditorBrushType.Box: return boxPrefab;
            case EditorBrushType.FragileBox: return fragileBoxPrefab;
            case EditorBrushType.Goal: return goalPrefab;
            case EditorBrushType.Ice: return icePrefab;
            case EditorBrushType.Switch: return switchPrefab;
            case EditorBrushType.Door: return doorPrefab;
            default: return null;
        }
    }

    public void SaveLevelAsPrefab()
    {
#if UNITY_EDITOR
        if (editorRoot == null || editorRoot.childCount == 0) return;

        string path = EditorUtility.SaveFilePanelInProject("Save Custom Level", "Level_XX", "prefab", "Choose folder", "Assets/Prefabs/Resources/Levels");
        if (string.IsNullOrEmpty(path)) return;

        GameObject tempRoot = new GameObject("CustomLevelRoot");
        GameObject staticRoot = new GameObject("Static");
        GameObject dynamicRoot = new GameObject("Dynamic");

        staticRoot.transform.SetParent(tempRoot.transform);
        dynamicRoot.transform.SetParent(tempRoot.transform);

        GridObject[] allObjects = editorRoot.GetComponentsInChildren<GridObject>(true);

        foreach (var gridObj in allObjects)
        {
            if (gridObj == null || !gridObj.gameObject.activeInHierarchy) continue;

            Transform parent = (gridObj.category == ObjectCategory.StaticEnvironment) ? staticRoot.transform : dynamicRoot.transform;
            GameObject clone = Instantiate(gridObj.gameObject, parent);
            clone.name = clone.name.Replace("(Clone)", "");

            RectTransform originalRect = gridObj.GetComponent<RectTransform>();
            RectTransform cloneRect = clone.GetComponent<RectTransform>();
            if (originalRect != null && cloneRect != null)
            {
                cloneRect.anchoredPosition = originalRect.anchoredPosition;
            }
        }

        foreach (Transform child in editorRoot)
        {
            if (child.CompareTag("Player") || child.name.Contains("Player"))
            {
                GameObject pClone = Instantiate(child.gameObject, dynamicRoot.transform);
                pClone.name = "Player";
                RectTransform originalRect = child.GetComponent<RectTransform>();
                RectTransform cloneRect = pClone.GetComponent<RectTransform>();
                if (originalRect != null && cloneRect != null) cloneRect.anchoredPosition = originalRect.anchoredPosition;
            }
        }

        PrefabUtility.SaveAsPrefabAsset(tempRoot, path);
        if (Application.isPlaying) Destroy(tempRoot);
        else DestroyImmediate(tempRoot);
#endif
    }
}