using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private GameView gameView;
    [SerializeField] private float swipeThreshold = 50f;

    private Vector2 startTouchPosition;
    private bool isSwiping = false;
    private bool hasMovedInCurrentSwipe = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        startTouchPosition = eventData.position;
        isSwiping = true;
        hasMovedInCurrentSwipe = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isSwiping || gameView == null || gameView.IsAnimating || hasMovedInCurrentSwipe) return;

        Vector2 swipeDelta = eventData.position - startTouchPosition;

        if (swipeDelta.magnitude > swipeThreshold)
        {
            Vector2Int dir = Vector2Int.zero;

            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                dir = swipeDelta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                dir = swipeDelta.y > 0 ? Vector2Int.down : Vector2Int.up;
            }

            gameView.HandleMove(dir);
            hasMovedInCurrentSwipe = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isSwiping = false;
        hasMovedInCurrentSwipe = false;
    }
}