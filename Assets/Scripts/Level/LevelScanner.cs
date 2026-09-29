using System.Collections.Generic;
using UnityEngine;

public class LevelDataPayload
{
    public GameState State;
    public Dictionary<Vector2Int, GameObject> BoxViews;
    public Dictionary<Vector2Int, GameObject> DoorViews;
    public RectTransform PlayerView;
    public float MinX;
    public float MaxY;
}

public static class LevelScanner
{
    private const float CELL_SIZE = 100f;

    public static LevelDataPayload Scan(Transform levelRoot, GameMechanicsConfig config, int levelIndex = -1)
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        GridObject[] gridObjects = levelRoot.GetComponentsInChildren<GridObject>();
        GameObject playerObj = null;

        foreach (Transform child in levelRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag("Player"))
            {
                playerObj = child.gameObject;
                break;
            }
        }

        if (playerObj == null) return null;

        List<RectTransform> allRects = new List<RectTransform>();
        foreach (var go in gridObjects) allRects.Add(go.GetComponent<RectTransform>());
        RectTransform pRect = playerObj.GetComponent<RectTransform>();
        allRects.Add(pRect);

        foreach (var rect in allRects)
        {
            if (rect == null) continue;

            Vector3 unifiedPos = levelRoot.InverseTransformPoint(rect.position);
            float px = unifiedPos.x;
            float py = unifiedPos.y;

            if (px < minX) minX = px;
            if (px > maxX) maxX = px;
            if (py < minY) minY = py;
            if (py > maxY) maxY = py;
        }

        int cols = Mathf.RoundToInt((maxX - minX) / CELL_SIZE) + 1;
        int rows = Mathf.RoundToInt((maxY - minY) / CELL_SIZE) + 1;

        StaticElement[,] staticGrid = new StaticElement[cols, rows];
        Dictionary<Vector2Int, BoxData> boxes = new Dictionary<Vector2Int, BoxData>();
        Dictionary<Vector2Int, ObjectColor> goals = new Dictionary<Vector2Int, ObjectColor>();
        Dictionary<Vector2Int, int> doors = new Dictionary<Vector2Int, int>();
        Dictionary<Vector2Int, int> switches = new Dictionary<Vector2Int, int>();
        Dictionary<Vector2Int, int> portals = new Dictionary<Vector2Int, int>();
        Dictionary<Vector2Int, GameObject> boxViews = new Dictionary<Vector2Int, GameObject>();
        Dictionary<Vector2Int, GameObject> doorViews = new Dictionary<Vector2Int, GameObject>();

        List<Transform> floorTiles = new List<Transform>();
        List<Transform> walls = new List<Transform>();
        List<Transform> interactables = new List<Transform>();

        foreach (var gridObj in gridObjects)
        {
            RectTransform rect = gridObj.GetComponent<RectTransform>();
            if (rect == null) continue;

            Vector3 unifiedPos = levelRoot.InverseTransformPoint(rect.position);
            int x = Mathf.RoundToInt((unifiedPos.x - minX) / CELL_SIZE);
            int y = Mathf.RoundToInt((maxY - unifiedPos.y) / CELL_SIZE);
            Vector2Int pos = new Vector2Int(x, y);

            if (gridObj is DoorGridObject doorObj)
            {
                staticGrid[x, y] = StaticElement.Door;
                doors[pos] = doorObj.linkID;
                doorViews[pos] = gridObj.gameObject;
                interactables.Add(gridObj.transform);
            }
            else if (gridObj is SwitchGridObject switchObj)
            {
                staticGrid[x, y] = StaticElement.Switch;
                switches[pos] = switchObj.linkID;
                interactables.Add(gridObj.transform);
            }
            else if (gridObj is PortalGridObject portalObj)
            {
                portals[pos] = portalObj.linkID;
                interactables.Add(gridObj.transform);
            }
            else if (gridObj is StaticGridObject staticObj)
            {
                staticGrid[x, y] = staticObj.staticElement;
                if (staticObj.staticElement == StaticElement.Wall) walls.Add(gridObj.transform);
                else floorTiles.Add(gridObj.transform);
            }
            else if (gridObj is GoalGridObject goalObj)
            {
                staticGrid[x, y] = StaticElement.Goal;
                goals[pos] = goalObj.color;
                interactables.Add(gridObj.transform);
            }
            else if (gridObj is BoxGridObject boxObj)
            {
                boxes[pos] = boxObj.boxData;
                boxViews[pos] = gridObj.gameObject;
            }
        }

        Vector3 playerUnifiedPos = levelRoot.InverseTransformPoint(pRect.position);
        int pxGrid = Mathf.RoundToInt((playerUnifiedPos.x - minX) / CELL_SIZE);
        int pyGrid = Mathf.RoundToInt((maxY - playerUnifiedPos.y) / CELL_SIZE);
        Vector2Int playerStartPos = new Vector2Int(pxGrid, pyGrid);

        int minInnerX = int.MaxValue, maxInnerX = int.MinValue;
        int minInnerY = int.MaxValue, maxInnerY = int.MinValue;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        bool[,] visited = new bool[cols, rows];

        void ExpandBounds(int bx, int by)
        {
            if (bx < minInnerX) minInnerX = bx;
            if (bx > maxInnerX) maxInnerX = bx;
            if (by < minInnerY) minInnerY = by;
            if (by > maxInnerY) maxInnerY = by;
        }

        queue.Enqueue(playerStartPos);
        visited[playerStartPos.x, playerStartPos.y] = true;
        ExpandBounds(playerStartPos.x, playerStartPos.y);

        foreach (var kvp in portals)
        {
            if (!visited[kvp.Key.x, kvp.Key.y])
            {
                queue.Enqueue(kvp.Key);
                visited[kvp.Key.x, kvp.Key.y] = true;
                ExpandBounds(kvp.Key.x, kvp.Key.y);
            }
        }

        while (queue.Count > 0)
        {
            Vector2Int curr = queue.Dequeue();

            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in dirs)
            {
                Vector2Int next = curr + dir;
                if (next.x >= 0 && next.x < cols && next.y >= 0 && next.y < rows)
                {
                    if (!visited[next.x, next.y])
                    {
                        visited[next.x, next.y] = true;

                        if (staticGrid[next.x, next.y] != StaticElement.Wall)
                        {
                            queue.Enqueue(next);
                            ExpandBounds(next.x, next.y);
                        }
                    }
                }
            }
        }

        float localPayloadMinX = pRect.anchoredPosition.x - (pxGrid * CELL_SIZE);
        float localPayloadMaxY = pRect.anchoredPosition.y + (pyGrid * CELL_SIZE);

        Transform floorGridTransform = null;
        foreach (Transform t in levelRoot.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.Replace(" ", "").ToLower().Contains("floorgrid"))
            {
                floorGridTransform = t;
                break;
            }
        }

        if (floorGridTransform != null && minInnerX <= maxInnerX && minInnerY <= maxInnerY)
        {
            RectTransform floorGridRect = floorGridTransform.GetComponent<RectTransform>();
            if (floorGridRect != null)
            {
                floorGridTransform.SetParent(playerObj.transform.parent);

                floorGridRect.anchorMin = new Vector2(0.5f, 0.5f);
                floorGridRect.anchorMax = new Vector2(0.5f, 0.5f);
                floorGridRect.pivot = new Vector2(0.5f, 0.5f);

                int innerCols = (maxInnerX - minInnerX) + 1;
                int innerRows = (maxInnerY - minInnerY) + 1;

                float centerX = localPayloadMinX + ((minInnerX + maxInnerX) / 2f) * CELL_SIZE;
                float centerY = localPayloadMaxY - ((minInnerY + maxInnerY) / 2f) * CELL_SIZE;

                floorGridRect.sizeDelta = new Vector2(innerCols * CELL_SIZE, innerRows * CELL_SIZE);
                floorGridRect.anchoredPosition = new Vector2(centerX, centerY);

                UnityEngine.UI.RawImage rawImg = floorGridTransform.GetComponent<UnityEngine.UI.RawImage>();
                if (rawImg != null)
                {
                    rawImg.uvRect = new Rect(0, 0, innerCols, innerRows);
                }
            }
        }

        if (floorGridTransform != null) floorGridTransform.SetAsFirstSibling();
        foreach (var t in floorTiles) t.SetAsLastSibling();

        foreach (var w in walls) w.SetAsLastSibling();
        foreach (var i in interactables) i.SetAsLastSibling();

        GameState newState = new GameState(staticGrid, boxes, playerStartPos, goals, doors, switches, portals);

        return new LevelDataPayload
        {
            State = newState,
            BoxViews = boxViews,
            DoorViews = doorViews,
            PlayerView = pRect,
            MinX = localPayloadMinX,
            MaxY = localPayloadMaxY
        };
    }
}