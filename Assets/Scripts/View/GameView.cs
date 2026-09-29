using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameView : MonoBehaviour
{
    public bool IsAnimating { get; private set; }

    private Dictionary<Vector2Int, GameObject> boxViews;
    private Dictionary<Vector2Int, GameObject> brokenBoxViews;
    private Dictionary<Vector2Int, GameObject> matchedBoxViews;
    private Dictionary<Vector2Int, GameObject> doorViews;
    private Dictionary<Vector2Int, bool> doorStates;
    private Dictionary<Vector2Int, GoalView> goalViews;
    private RectTransform playerView;
    private float minX;
    private float maxY;
    private const float CELL_SIZE = 100f;

    public void InitializeVisuals(LevelDataPayload payload)
    {
        MonoBehaviour[] allScripts = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour script in allScripts)
        {
            if (script != null && script.GetType().Name.Contains("SnapToGrid"))
            {
                Destroy(script);
            }
        }

        boxViews = payload.BoxViews;
        brokenBoxViews = new Dictionary<Vector2Int, GameObject>();
        matchedBoxViews = new Dictionary<Vector2Int, GameObject>();
        doorViews = payload.DoorViews;
        doorStates = new Dictionary<Vector2Int, bool>();
        goalViews = new Dictionary<Vector2Int, GoalView>();
        playerView = payload.PlayerView;
        minX = payload.MinX;
        maxY = payload.MaxY;
        IsAnimating = false;

        if (playerView != null)
        {
            playerView.anchorMin = new Vector2(0.5f, 0.5f);
            playerView.anchorMax = new Vector2(0.5f, 0.5f);
            playerView.pivot = new Vector2(0.5f, 0.5f);
            playerView.localScale = Vector3.one;

            if (payload.State != null)
            {
                playerView.anchoredPosition = GridToWorld(payload.State.Player);
            }
        }

        if (boxViews != null && playerView != null)
        {
            foreach (var kvp in boxViews)
            {
                if (kvp.Value != null)
                {
                    RectTransform boxRect = kvp.Value.GetComponent<RectTransform>();
                    if (boxRect != null)
                    {
                        boxRect.SetParent(playerView.parent, true);
                        boxRect.anchorMin = new Vector2(0.5f, 0.5f);
                        boxRect.anchorMax = new Vector2(0.5f, 0.5f);
                        boxRect.pivot = new Vector2(0.5f, 0.5f);
                        boxRect.anchoredPosition = GridToWorld(kvp.Key);
                        boxRect.SetAsLastSibling();
                    }
                }
            }
        }

        if (playerView != null)
        {
            playerView.SetAsLastSibling();
        }

        GoalView[] allGoals = FindObjectsOfType<GoalView>();
        foreach (var g in allGoals)
        {
            GoalGridObject goalGrid = g.GetComponent<GoalGridObject>();
            if (goalGrid != null)
            {
                RectTransform r = goalGrid.GetComponent<RectTransform>();
                if (r != null)
                {
                    Transform root = playerView != null ? playerView.parent : r.parent;
                    Vector3 unifiedPos = root.InverseTransformPoint(r.position);
                    int x = Mathf.RoundToInt((unifiedPos.x - minX) / CELL_SIZE);
                    int y = Mathf.RoundToInt((maxY - unifiedPos.y) / CELL_SIZE);
                    goalViews[new Vector2Int(x, y)] = g;

                    r.anchorMin = new Vector2(0.5f, 0.5f);
                    r.anchorMax = new Vector2(0.5f, 0.5f);
                    r.pivot = new Vector2(0.5f, 0.5f);
                    r.anchoredPosition = GridToWorld(new Vector2Int(x, y));
                }
            }
        }
    }

    public void SyncDoors(GameState state, bool animate = true)
    {
        if (doorViews == null) return;
        foreach (var kvp in doorViews)
        {
            bool isOpen = state.IsDoorOpen(kvp.Key);
            if (!doorStates.TryGetValue(kvp.Key, out bool wasOpen) || wasOpen != isOpen)
            {
                doorStates[kvp.Key] = isOpen;
                if (animate)
                {
                    if (HapticManager.Instance != null) HapticManager.Instance.PlayDoorToggle();
                    if (isOpen) kvp.Value.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
                    else kvp.Value.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                }
                else
                {
                    kvp.Value.transform.localScale = isOpen ? Vector3.zero : Vector3.one;
                }
            }
        }
    }

    public void UpdateAllBoxes(Dictionary<Vector2Int, BoxData> boxes, Dictionary<Vector2Int, ObjectColor> goals)
    {
        foreach (var kvp in boxes)
        {
            if (boxViews.TryGetValue(kvp.Key, out GameObject boxObj))
            {
                RectTransform rect = boxObj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = GridToWorld(kvp.Key);
                }

                BoxView bv = boxObj.GetComponent<BoxView>();
                if (bv != null)
                {
                    bool isOnGoal = goals.TryGetValue(kvp.Key, out ObjectColor gColor) && (gColor == ObjectColor.None || gColor == kvp.Value.Color);
                    bv.UpdateVisuals(isOnGoal, kvp.Value.Durability, kvp.Key, HandleGoalVisualState);
                }
            }
        }
    }

    private void HandleGoalVisualState(Vector2Int pos, bool isCompleted)
    {
        if (goalViews != null && goalViews.TryGetValue(pos, out GoalView goalView))
        {
            goalView.SetState(isCompleted);
        }
    }

    public void AnimateMove(MoveRecord record, Action onComplete, float duration)
    {
        IsAnimating = true;
        Sequence seq = DOTween.Sequence();
        float portalDur = 0.4f;
        if (duration <= 0.05f) duration = 0.15f;

        if (playerView != null)
        {
            playerView.DOKill(true);
            playerView.anchoredPosition = GridToWorld(record.Player.StartPos);
        }

        if (record.Player.IsPortalRejected)
        {
            Vector2 entry = GridToWorld(record.Player.PortalEntry);
            Vector2 start = GridToWorld(record.Player.StartPos);
            seq.Insert(0, playerView.DOAnchorPos(entry, portalDur * 0.5f).SetEase(Ease.InBack))
               .Insert(0, playerView.DOScale(Vector3.zero, portalDur * 0.5f).SetEase(Ease.InBack))
               .InsertCallback(portalDur * 0.5f, () => {
                   if (HapticManager.Instance != null) HapticManager.Instance.PlayPortalReject();
                   if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.portalRejectSound);
               })
               .Insert(portalDur * 0.5f, playerView.DOAnchorPos(start, portalDur * 0.5f).SetEase(Ease.OutBack))
               .Insert(portalDur * 0.5f, playerView.DOScale(Vector3.one, portalDur * 0.5f).SetEase(Ease.OutBack));
        }
        else if (record.Box.IsPortalRejected)
        {
            GameObject box = boxViews[record.Box.StartPos];
            RectTransform boxRect = box.GetComponent<RectTransform>();

            boxRect.DOKill(true);
            boxRect.anchoredPosition = GridToWorld(record.Box.StartPos);

            Vector2 bEntry = GridToWorld(record.Box.PortalEntry);
            Vector2 bStart = GridToWorld(record.Box.StartPos);

            seq.Insert(0, boxRect.DOAnchorPos(bEntry, duration).SetEase(Ease.OutQuad))
               .Insert(0, box.transform.DOScale(Vector3.zero, duration).SetEase(Ease.InQuad))
               .InsertCallback(duration, () => {
                   if (HapticManager.Instance != null) HapticManager.Instance.PlayPortalReject();
                   if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.portalRejectSound);
               })
               .Insert(duration, boxRect.DOAnchorPos(bStart, portalDur * 0.5f).SetEase(Ease.OutBack))
               .Insert(duration, box.transform.DOScale(Vector3.one, portalDur * 0.5f).SetEase(Ease.OutBack));

            Vector2 pStart = GridToWorld(record.Player.StartPos);
            seq.Insert(0, playerView.DOAnchorPos(bStart, duration).SetEase(Ease.OutQuad))
               .Insert(duration, playerView.DOAnchorPos(pStart, duration).SetEase(Ease.OutQuad));
        }
        else
        {
            if (record.Player.IsTeleported)
            {
                Vector2 entry = GridToWorld(record.Player.PortalEntry);
                Vector2 exit = GridToWorld(record.Player.EndPos);
                seq.Insert(0, playerView.DOAnchorPos(entry, portalDur * 0.5f).SetEase(Ease.InBack))
                   .Insert(0, playerView.DOScale(Vector3.zero, portalDur * 0.5f).SetEase(Ease.InBack))
                   .InsertCallback(portalDur * 0.5f, () => {
                       playerView.anchoredPosition = exit;
                       if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.portalTeleportSound);
                   })
                   .Insert(portalDur * 0.5f, playerView.DOScale(Vector3.one, portalDur * 0.5f).SetEase(Ease.OutBack));
            }
            else
            {
                seq.Insert(0, playerView.DOAnchorPos(GridToWorld(record.Player.EndPos), duration).SetEase(Ease.OutQuad));
            }

            if (record.Box.IsPushed)
            {
                if (HapticManager.Instance != null) HapticManager.Instance.PlayWarning();

                GameObject box = boxViews[record.Box.StartPos];
                boxViews.Remove(record.Box.StartPos);

                RectTransform boxRect = box.GetComponent<RectTransform>();

                if (boxRect != null)
                {
                    boxRect.DOKill(true);
                    boxRect.anchoredPosition = GridToWorld(record.Box.StartPos);
                }

                if (record.Box.IsTeleported)
                {
                    Vector2 bEntry = GridToWorld(record.Box.PortalEntry);
                    Vector2 bExit = GridToWorld(record.Box.EndPos);

                    seq.Insert(0, boxRect.DOAnchorPos(bEntry, duration).SetEase(Ease.OutQuad))
                       .Insert(0, box.transform.DOScale(Vector3.zero, duration).SetEase(Ease.InQuad))
                       .InsertCallback(duration, () => {
                           boxRect.anchoredPosition = bExit;
                           if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.portalTeleportSound);
                       })
                       .Insert(duration, box.transform.DOScale(Vector3.one, portalDur * 0.5f).SetEase(Ease.OutBack));
                }
                else
                {
                    seq.Insert(0, boxRect.DOAnchorPos(GridToWorld(record.Box.EndPos), duration).SetEase(Ease.OutQuad));
                }

                if (record.Box.IsBroken)
                {
                    if (HapticManager.Instance != null) HapticManager.Instance.PlayError();

                    if (!brokenBoxViews.ContainsKey(record.Box.EndPos))
                    {
                        brokenBoxViews.Add(record.Box.EndPos, box);
                    }

                    BoxView bv = box.GetComponent<BoxView>();
                    if (bv != null) bv.UpdateVisuals(false, 0, record.Box.EndPos, HandleGoalVisualState);

                    Sequence breakSeq = DOTween.Sequence();
                    breakSeq.Append(box.transform.DOShakeScale(0.3f, 0.4f, 10, 90f));
                    breakSeq.Append(box.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));

                    seq.Insert(duration, breakSeq);
                }
                else if (record.Box.IsMatchedGoal)
                {
                    if (HapticManager.Instance != null) HapticManager.Instance.PlaySuccess();

                    if (!matchedBoxViews.ContainsKey(record.Box.EndPos))
                    {
                        matchedBoxViews.Add(record.Box.EndPos, box);
                    }

                    BoxView bv = box.GetComponent<BoxView>();
                    if (bv != null) bv.UpdateVisuals(true, record.Box.Snapshot.Durability, record.Box.EndPos, HandleGoalVisualState);

                    seq.InsertCallback(duration + 0.3f, () => {
                        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.boxMatchSound);
                    });

                    Sequence matchSeq = DOTween.Sequence();
                    matchSeq.Append(box.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));

                    if (goalViews != null && goalViews.TryGetValue(record.Box.EndPos, out GoalView goalView))
                    {
                        matchSeq.Join(goalView.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
                    }

                    seq.Insert(duration, matchSeq);
                }
                else
                {
                    boxViews.Add(record.Box.EndPos, box);
                }
            }
        }

        seq.OnComplete(() =>
        {
            if (playerView != null)
            {
                playerView.anchoredPosition = GridToWorld(record.Player.EndPos);
                playerView.SetAsLastSibling();
            }
            IsAnimating = false;
            onComplete?.Invoke();
        });
    }

    public void AnimateUndo(MoveRecord record, Action onComplete, float duration)
    {
        IsAnimating = true;
        Sequence seq = DOTween.Sequence();
        float portalDur = 0.4f;
        if (duration <= 0.05f) duration = 0.15f;

        if (playerView != null) playerView.SetAsLastSibling();

        if (record.Player.IsTeleported)
        {
            Vector2 entry = GridToWorld(record.Player.PortalEntry);
            Vector2 start = GridToWorld(record.Player.StartPos);
            seq.InsertCallback(0f, () => {
                playerView.anchoredPosition = entry;
                playerView.localScale = Vector3.zero;
                if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.portalTeleportSound);
            })
               .Insert(0f, playerView.DOScale(Vector3.one, portalDur * 0.5f).SetEase(Ease.OutBack))
               .Insert(0f, playerView.DOAnchorPos(start, portalDur * 0.5f).SetEase(Ease.OutQuad));
        }
        else
        {
            seq.Insert(0, playerView.DOAnchorPos(GridToWorld(record.Player.StartPos), duration).SetEase(Ease.OutQuad));
        }

        if (record.Box.IsPushed)
        {
            GameObject box;
            if (record.Box.IsBroken)
            {
                box = brokenBoxViews[record.Box.EndPos];
                brokenBoxViews.Remove(record.Box.EndPos);
                box.SetActive(true);
                box.transform.localScale = Vector3.one;
            }
            else if (record.Box.IsMatchedGoal)
            {
                box = matchedBoxViews[record.Box.EndPos];
                matchedBoxViews.Remove(record.Box.EndPos);
                box.SetActive(true);
                box.transform.localScale = Vector3.one;

                if (goalViews != null && goalViews.TryGetValue(record.Box.EndPos, out GoalView goalView))
                {
                    goalView.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                }
            }
            else
            {
                box = boxViews[record.Box.EndPos];
                boxViews.Remove(record.Box.EndPos);
            }

            boxViews.Add(record.Box.StartPos, box);
            RectTransform boxRect = box.GetComponent<RectTransform>();
            boxRect.SetAsLastSibling();

            if (record.Box.IsTeleported)
            {
                Vector2 entry = GridToWorld(record.Box.PortalEntry);
                Vector2 start = GridToWorld(record.Box.StartPos);
                seq.InsertCallback(0f, () => {
                    boxRect.anchoredPosition = entry;
                    box.transform.localScale = Vector3.zero;
                    if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.portalTeleportSound);
                })
                   .Insert(0f, box.transform.DOScale(Vector3.one, portalDur * 0.5f).SetEase(Ease.OutBack))
                   .Insert(0f, boxRect.DOAnchorPos(start, duration).SetEase(Ease.OutQuad));
            }
            else
            {
                seq.Insert(0, boxRect.DOAnchorPos(GridToWorld(record.Box.StartPos), duration).SetEase(Ease.OutQuad));
            }
        }
        seq.OnComplete(() =>
        {
            if (playerView != null)
            {
                playerView.anchoredPosition = GridToWorld(record.Player.StartPos);
                playerView.SetAsLastSibling();
            }
            IsAnimating = false;
            onComplete?.Invoke();
        });
    }

    private Vector2 GridToWorld(Vector2Int gridPos)
    {
        return new Vector2(minX + (gridPos.x * CELL_SIZE), maxY - (gridPos.y * CELL_SIZE));
    }
}