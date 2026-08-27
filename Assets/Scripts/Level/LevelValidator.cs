using System.Collections.Generic;
using UnityEngine;

public static class LevelValidator
{
    public static void Validate(CellType[,] grid, HashSet<Vector2Int> boxes, Vector2Int playerPos, int goalCount, GridObject[] objects)
    {
        int playerCount = 0;

        foreach (var obj in objects)
        {
            if (obj.kind == CellKind.Player) playerCount++;

            float diffX = Mathf.Abs(obj.transform.localPosition.x - Mathf.Round(obj.transform.localPosition.x));
            float diffY = Mathf.Abs(obj.transform.localPosition.y - Mathf.Round(obj.transform.localPosition.y));

            if (diffX > 0.01f || diffY > 0.01f)
            {
                Debug.LogError($"Grid alignment error! Object not snapped: {obj.gameObject.name}", obj.gameObject);
            }
        }

        if (playerCount != 1)
        {
            Debug.LogError($"Invalid player count: {playerCount}. Exactly 1 player is required.");
        }

        if (boxes.Count == 0)
        {
            Debug.LogError("No boxes found in the level.");
        }

        if (boxes.Count != goalCount)
        {
            Debug.LogError($"Mismatch! Box count: {boxes.Count}, Goal count: {goalCount}");
        }

        if (grid[playerPos.x, playerPos.y] == CellType.Wall)
        {
            Debug.LogError("Player is overlapping a Wall.");
        }

        foreach (var box in boxes)
        {
            if (grid[box.x, box.y] == CellType.Wall)
            {
                Debug.LogError($"Box at {box} is overlapping a Wall.");
            }
        }
    }
}