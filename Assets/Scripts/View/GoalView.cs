using UnityEngine;
using UnityEngine.UI;

public class GoalView : MonoBehaviour
{
    [SerializeField] private GameObject defaultVisual;

    private void Awake() 
    {
        ApplyConfigColor();
        SetState(false);
    }

    private void ApplyConfigColor()
    {
        GoalGridObject goalData = GetComponent<GoalGridObject>();
        if (goalData == null) return;

        GameMechanicsConfig config = Resources.Load<GameMechanicsConfig>("GameMechanicsConfig");
        if (config == null) return;

        foreach (var mapping in config.colorMappings)
        {
            if (mapping.colorID == goalData.color)
            {
                Image img = defaultVisual != null ? defaultVisual.GetComponent<Image>() : GetComponent<Image>();
                if (img != null)
                {
                    Color appliedColor = mapping.baseColor;
                    if (appliedColor.a <= 0.05f)
                    {
                        appliedColor.a = 1f;
                    }
                    img.color = appliedColor;
                }
                break;
            }
        }
    }

    public void SetState(bool isCompleted)
    {
        if (defaultVisual != null)
        {
            if (defaultVisual == this.gameObject)
            {
                Image img = defaultVisual.GetComponent<Image>();
                if (img != null) img.enabled = !isCompleted;
            }
            else
            {
                defaultVisual.SetActive(!isCompleted);
            }
        }
        else
        {
            Image img = GetComponent<Image>();
            if (img != null) img.enabled = !isCompleted;
        }
    }
}