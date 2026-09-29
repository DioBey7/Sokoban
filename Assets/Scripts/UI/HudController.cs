using UnityEngine;
using TMPro; 

public class HudController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI pushesText;
    [SerializeField] private TextMeshProUGUI bestText;

    public void UpdateCounters(int moves, int pushes, int bestScore, int maxMoves = 0)
    {
        if (movesText != null)
        {
            if (maxMoves > 0)
            {
                int remaining = Mathf.Max(0, maxMoves - moves);
                movesText.text = $"MOVES: {moves} / {maxMoves}";
            }
            else
            {
                movesText.text = $"MOVES: {moves}";
            }
        }

        if (pushesText != null)
        {
            pushesText.text = $"PUSHES: {pushes}";
        }

        if (bestText != null)
        {
            if (bestScore == int.MaxValue || bestScore <= 0)
            {
                bestText.text = "BEST: --";
            }
            else
            {
                bestText.text = $"BEST: {bestScore}";
            }
        }
    }

    public void SetDeadlockWarning(bool isActive)
    {
        Debug.Log($"Deadlock warning set to: {isActive}");
    }
}