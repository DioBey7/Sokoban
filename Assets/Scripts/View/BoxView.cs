using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class BoxView : MonoBehaviour
{
    [SerializeField] private GameObject boxVisual;
    [SerializeField] private TextMeshProUGUI durabilityText;

    private bool wasOnGoal = false;
    private Vector2Int? lastGoalPos = null;
    private bool isInitialized = false;

    public void UpdateVisuals(bool isOnGoal, int durability, Vector2Int currentPos, Action<Vector2Int, bool> onVisualStateChange)
    {
        if (!isInitialized)
        {
            wasOnGoal = isOnGoal;
            lastGoalPos = isOnGoal ? currentPos : null;

            if (boxVisual != null) boxVisual.SetActive(!isOnGoal);
            if (isOnGoal)
            {
                transform.localScale = Vector3.zero;
                onVisualStateChange?.Invoke(currentPos, true);
            }

            isInitialized = true;
        }
        else
        {
            if (isOnGoal && !wasOnGoal)
            {
                lastGoalPos = currentPos;
                transform.DOKill(true);

                Sequence popSeq = DOTween.Sequence();
                popSeq.Append(transform.DOScale(Vector3.one * 1.25f, 0.3f).SetEase(Ease.OutQuad));
                popSeq.Append(transform.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InBack));
                popSeq.OnComplete(() =>
                {
                    if (boxVisual != null) boxVisual.SetActive(false);
                    onVisualStateChange?.Invoke(currentPos, true);
                });
            }
            else if (!isOnGoal && wasOnGoal)
            {
                transform.DOKill(true);

                if (lastGoalPos.HasValue)
                {
                    onVisualStateChange?.Invoke(lastGoalPos.Value, false);
                    lastGoalPos = null;
                }

                if (boxVisual != null) boxVisual.SetActive(true);
                transform.localScale = Vector3.zero;
                transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
            }

            wasOnGoal = isOnGoal;
        }

        if (durabilityText != null)
        {
            if (durability > 0)
            {
                durabilityText.text = durability.ToString();
                durabilityText.gameObject.SetActive(!isOnGoal);
            }
            else
            {
                durabilityText.gameObject.SetActive(false);
            }
        }
    }
}