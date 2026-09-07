using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private GameMechanicsConfig config;
    [SerializeField] private Transform levelRoot;
    [SerializeField] private RectTransform levelArea;

    private int currentLevelIndex = 0;
    private GameObject currentLevelInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentLevelIndex = PlayerPrefs.GetInt("SavedLevelIndex", 0);
    }

    public void StartGame()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        if (config == null || config.levels.Count == 0) return;

        if (index >= config.levels.Count)
        {
            index = 0;
        }

        currentLevelIndex = index;

        PlayerPrefs.SetInt("SavedLevelIndex", currentLevelIndex);
        PlayerPrefs.Save();

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateLevelText(currentLevelIndex + 1);
            GameUIManager.Instance.HideLevelCompletePanel();
        }

        DOTween.KillAll();

        if (levelRoot != null)
        {
            levelRoot.localScale = Vector3.one;
            RectTransform rootRect = levelRoot.GetComponent<RectTransform>();
            if (rootRect != null) rootRect.anchoredPosition = Vector2.zero;

            foreach (Transform child in levelRoot)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
            levelRoot.DetachChildren();
        }

        currentLevelInstance = null;

        GameObject prefab = config.levels[currentLevelIndex].levelPrefab;
        if (prefab != null)
        {
            currentLevelInstance = Instantiate(prefab, levelRoot);

            var inputModules = currentLevelInstance.GetComponentsInChildren<BaseInputModule>(true);
            foreach (var module in inputModules)
            {
                DestroyImmediate(module);
            }

            var extraEventSystems = currentLevelInstance.GetComponentsInChildren<EventSystem>(true);
            foreach (var es in extraEventSystems)
            {
                DestroyImmediate(es);
            }

            if (GameController.Instance != null)
            {
                GameController.Instance.InitializeLevel(currentLevelInstance.transform, config, currentLevelIndex);
            }

            AutoScaleLevelRoot();
        }
    }

    private void AutoScaleLevelRoot()
    {
        if (levelRoot == null) return;

        Canvas.ForceUpdateCanvases();

        RectTransform rootRect = levelRoot.GetComponent<RectTransform>();
        Canvas canvas = levelRoot.GetComponentInParent<Canvas>();
        if (rootRect == null || canvas == null) return;

        if (levelArea != null && levelArea != rootRect)
        {
            levelRoot.SetParent(levelArea, false);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
        }

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        bool hasElements = false;

        RectTransform[] allRects = levelRoot.GetComponentsInChildren<RectTransform>();
        foreach (var rect in allRects)
        {
            if (rect == rootRect || rect.GetComponent<Canvas>() != null) continue;

            Vector3 localPos = rootRect.InverseTransformPoint(rect.position);

            hasElements = true;
            if (localPos.x < minX) minX = localPos.x;
            if (localPos.x > maxX) maxX = localPos.x;
            if (localPos.y < minY) minY = localPos.y;
            if (localPos.y > maxY) maxY = localPos.y;
        }

        if (!hasElements) return;

        float width = (maxX - minX) + 120f;
        float height = (maxY - minY) + 120f;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float screenW = canvasRect.rect.width * 0.90f;
        float screenH = canvasRect.rect.height * 0.65f;

        if (levelArea != null && levelArea.rect.width > 100f && levelArea.rect.height > 100f)
        {
            screenW = levelArea.rect.width * 0.95f;
            screenH = levelArea.rect.height * 0.95f;
        }

        float scale = Mathf.Min(screenW / width, screenH / height);
        scale = Mathf.Clamp(scale, 0.5f, 4.0f);

        levelRoot.localScale = new Vector3(scale, scale, 1f);

        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;

        rootRect.localPosition = new Vector3(-centerX * scale, -centerY * scale, 0f);
    }

    public void NextLevel()
    {
        LoadLevel(currentLevelIndex + 1);
    }

    public void RestartLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("SavedLevelIndex");
        PlayerPrefs.Save();
        LoadLevel(0);
    }
}