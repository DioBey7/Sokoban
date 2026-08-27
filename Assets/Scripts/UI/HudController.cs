using UnityEngine;
using TMPro;

public class HudController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moveText;
    [SerializeField] private TextMeshProUGUI pushText;
    [SerializeField] private TextMeshProUGUI bestScoreText;

    public void UpdateCounters(int moves, int pushes, int bestScore)
    {
        if (moveText != null) moveText.text = $"MOVES: {moves}";
        if (pushText != null) pushText.text = $"PUSHES: {pushes}";
        if (bestScoreText != null) bestScoreText.text = bestScore == int.MaxValue ? "BEST: -" : $"BEST: {bestScore}";
    }

    public void SetDeadlockWarning(bool isActive)
    {
        Debug.Log($"Deadlock warning set to: {isActive}");
    }
}