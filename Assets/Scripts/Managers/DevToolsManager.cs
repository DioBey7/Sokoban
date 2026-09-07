using UnityEngine;

public class DevToolsManager : MonoBehaviour
{
    [SerializeField] private GameObject[] devUIElements;

    private void Awake()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        foreach (var element in devUIElements)
        {
            if (element != null)
            {
                element.SetActive(false);
            }
        }
#endif
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (RuntimeLevelEditor.Instance != null)
            {
                RuntimeLevelEditor.Instance.ToggleEditor();
            }
        }
#endif
    }
}