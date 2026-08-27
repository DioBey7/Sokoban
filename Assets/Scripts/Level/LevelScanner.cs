using System.Collections.Generic;
using UnityEngine;

public class LevelDataPayload
{
    public GameState State;
    public Dictionary<Vector2Int, RectTransform> BoxViews;
    public RectTransform PlayerView;
    public float MinX;
    public float MaxX;
    public float MinY;
    public float MaxY;
}

public static class LevelScanner
{
    public static LevelDataPayload Scan(Transform levelRoot)
    {
        GridObject[] objects = levelRoot.GetComponentsInChildren<GridObject>();
        if (objects.Length == 0) return null;

        foreach (var obj in objects)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                float posX = rect.anchoredPosition.x;
                float posY = rect.anchoredPosition.y;

                if ((Mathf.Abs(posX) > 0 && Mathf.Abs(posX) < 20) || (Mathf.Abs(posY) > 0 && Mathf.Abs(posY) < 20))
                {
                    rect.anchoredPosition = new Vector2(
                        Mathf.Round(posX) * GridGeometry.CELL_SIZE,
                        Mathf.Round(posY) * GridGeometry.CELL_SIZE
                    );
                }
            }
        }

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (var obj in objects)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            Vector2 pos = rect != null ? rect.anchoredPosition : (Vector2)obj.transform.localPosition;

            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        int width = Mathf.RoundToInt((maxX - minX) / GridGeometry.CELL_SIZE) + 1;
        int height = Mathf.RoundToInt((maxY - minY) / GridGeometry.CELL_SIZE) + 1;

        CellType[,] staticGrid = new CellType[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                staticGrid[x, y] = CellType.Floor;
            }
        }

        HashSet<Vector2Int> boxes = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, RectTransform> boxViews = new Dictionary<Vector2Int, RectTransform>();
        Vector2Int playerPos = Vector2Int.zero;
        RectTransform playerView = null;
        int goalCount = 0;

        foreach (var obj in objects)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            Vector2 pos = rect != null ? rect.anchoredPosition : (Vector2)obj.transform.localPosition;

            int col = Mathf.RoundToInt((pos.x - minX) / GridGeometry.CELL_SIZE);
            int row = Mathf.RoundToInt((maxY - pos.y) / GridGeometry.CELL_SIZE);
            Vector2Int gridPos = new Vector2Int(col, row);

            switch (obj.kind)
            {
                case CellKind.Wall:
                    staticGrid[col, row] = CellType.Wall;
                    break;
                case CellKind.Goal:
                    staticGrid[col, row] = CellType.Goal;
                    goalCount++;
                    break;
                case CellKind.Box:
                    boxes.Add(gridPos);
                    boxViews.Add(gridPos, rect);
                    break;
                case CellKind.Player:
                    playerPos = gridPos;
                    playerView = rect;
                    break;
            }
        }

        LevelValidator.Validate(staticGrid, boxes, playerPos, goalCount, objects);

        return new LevelDataPayload
        {
            State = new GameState(staticGrid, playerPos, boxes),
            BoxViews = boxViews,
            PlayerView = playerView,
            MinX = minX,
            MaxX = maxX,
            MinY = minY,
            MaxY = maxY
        };
    }
}