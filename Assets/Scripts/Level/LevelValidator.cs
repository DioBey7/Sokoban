using System.Collections.Generic;
using UnityEngine;

public static class LevelValidator
{
    public static void Validate(StaticElement[,] grid, Dictionary<Vector2Int, BoxData> boxes, Vector2Int playerPos, int goalCount, GridObject[] objects)
    {
        foreach (var obj in objects)
        {
            float diffX = Mathf.Abs(obj.transform.localPosition.x - Mathf.Round(obj.transform.localPosition.x));
            float diffY = Mathf.Abs(obj.transform.localPosition.y - Mathf.Round(obj.transform.localPosition.y));

            if (diffX > 0.01f || diffY > 0.01f)
            {
                Debug.LogError($"Grid alignment error! Object not snapped: {obj.gameObject.name}", obj.gameObject);
            }
        }

        if (boxes.Count == 0)
        {
            Debug.LogError("No boxes found in the level.");
        }

        if (boxes.Count != goalCount)
        {
            Debug.LogError($"Mismatch! Box count: {boxes.Count}, Goal count: {goalCount}");
        }

        if (grid[playerPos.x, playerPos.y] == StaticElement.Wall)
        {
            Debug.LogError("Player is overlapping a Wall.");
        }

        foreach (var kvp in boxes)
        {
            if (grid[kvp.Key.x, kvp.Key.y] == StaticElement.Wall)
            {
                Debug.LogError($"Box at {kvp.Key} is overlapping a Wall.");
            }
        }
    }
}