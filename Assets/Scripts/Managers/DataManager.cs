using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveBestMove(int levelIndex, int moveCount)
    {
        string key = $"BestMove_Level_{levelIndex}";
        int currentBest = PlayerPrefs.GetInt(key, int.MaxValue);

        if (moveCount < currentBest)
        {
            PlayerPrefs.SetInt(key, moveCount);
            PlayerPrefs.Save();
        }
    }

    public int GetBestMove(int levelIndex)
    {
        return PlayerPrefs.GetInt($"BestMove_Level_{levelIndex}", 0);
    }
}