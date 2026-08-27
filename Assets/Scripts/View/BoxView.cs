using UnityEngine;
using UnityEngine.UI;

public class BoxView : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color onGoalColor = Color.green;

    public void SetOnGoal(bool isOnGoal)
    {
        if (image != null)
        {
            image.color = isOnGoal ? onGoalColor : normalColor;
        }
    }
}