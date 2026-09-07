using UnityEngine;

[ExecuteAlways]
public class SnapToGrid : MonoBehaviour
{
#if UNITY_EDITOR
    private void Update()
    {
        if (Application.isPlaying) return;

        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            float x = Mathf.Round(rect.anchoredPosition.x / GridGeometry.CELL_SIZE) * GridGeometry.CELL_SIZE;
            float y = Mathf.Round(rect.anchoredPosition.y / GridGeometry.CELL_SIZE) * GridGeometry.CELL_SIZE;

            if (Mathf.Abs(rect.anchoredPosition.x - x) > 0.01f || Mathf.Abs(rect.anchoredPosition.y - y) > 0.01f)
            {
                rect.anchoredPosition = new Vector2(x, y);
            }
        }
    }
#endif
}