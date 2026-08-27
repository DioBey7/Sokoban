using UnityEngine;

public class LevelRoot : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
        GridObject[] objects = GetComponentsInChildren<GridObject>();

        if (objects.Length == 0) return;

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var obj in objects)
        {
            Vector3 pos = obj.transform.localPosition;
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        for (float x = minX - 0.5f; x <= maxX + 0.5f; x++)
        {
            Gizmos.DrawLine(new Vector3(x, minY - 0.5f, 0), new Vector3(x, maxY + 0.5f, 0));
        }
        for (float y = minY - 0.5f; y <= maxY + 0.5f; y++)
        {
            Gizmos.DrawLine(new Vector3(minX - 0.5f, y, 0), new Vector3(maxX + 0.5f, y, 0));
        }
    }
}