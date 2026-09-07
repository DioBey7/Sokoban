using UnityEngine;
using System;
using DG.Tweening;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }
    public event Action OnEconomyChanged;

    private const string UNDO_KEY = "Player_UndoCount";
    private const string GOLD_KEY = "Player_GoldCount";

    public int UndoCount { get; private set; }
    public int GoldCount { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadEconomy();
    }

    private void LoadEconomy()
    {
        UndoCount = PlayerPrefs.GetInt(UNDO_KEY, 3);
        GoldCount = PlayerPrefs.GetInt(GOLD_KEY, 0);
    }

    public bool TryUseUndo()
    {
        if (UndoCount > 0)
        {
            UndoCount--;
            PlayerPrefs.SetInt(UNDO_KEY, UndoCount);
            PlayerPrefs.Save();
            OnEconomyChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void AddUndo(int amount)
    {
        UndoCount += amount;
        PlayerPrefs.SetInt(UNDO_KEY, UndoCount);
        PlayerPrefs.Save();
        OnEconomyChanged?.Invoke();
    }

    public bool TrySpendGold(int amount)
    {
        if (GoldCount >= amount)
        {
            GoldCount -= amount;
            PlayerPrefs.SetInt(GOLD_KEY, GoldCount);
            PlayerPrefs.Save();
            OnEconomyChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void AddGold(int amount)
    {
        GoldCount += amount;
        PlayerPrefs.SetInt(GOLD_KEY, GoldCount);
        PlayerPrefs.Save();
        OnEconomyChanged?.Invoke();
    }
}