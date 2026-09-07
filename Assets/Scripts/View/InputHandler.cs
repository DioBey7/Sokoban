using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    public event Action<Vector2Int> OnMoveInput;

    [HideInInspector] public bool IsInputLocked = false;

    [SerializeField] private float swipeThreshold = 40f;
    private Vector2 swipeStartPos;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OnSwipeBegin(BaseEventData eventData)
    {
        if (eventData is PointerEventData pointerData)
        {
            swipeStartPos = pointerData.position;
        }
    }

    public void OnSwipeEnd(BaseEventData eventData)
    {
        if (IsInputLocked) return;

        if (eventData is PointerEventData pointerData)
        {
            Vector2 swipeDelta = pointerData.position - swipeStartPos;

            if (swipeDelta.magnitude > swipeThreshold)
            {
                if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                {
                    OnMoveInput?.Invoke(swipeDelta.x > 0 ? Vector2Int.right : Vector2Int.left);
                }
                else
                {
                    OnMoveInput?.Invoke(swipeDelta.y > 0 ? Vector2Int.down : Vector2Int.up);
                }
            }
        }
    }
}