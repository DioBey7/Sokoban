using UnityEngine;

public static class GridGeometry
{
    public const float CELL_SIZE = 100f;

    public static Vector2 GridToWorld(int col, int row, float startX, float startY)
    {
        float x = startX + (col * CELL_SIZE);
        float y = startY - (row * CELL_SIZE);
        return new Vector2(x, y);
    }

    public static Vector2Int WorldToGrid(Vector2 anchoredPos, float startX, float startY)
    {
        int col = Mathf.RoundToInt((anchoredPos.x - startX) / CELL_SIZE);
        int row = Mathf.RoundToInt((startY - anchoredPos.y) / CELL_SIZE);
        return new Vector2Int(col, row);
    }
}