using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ColorMapping
{
    public ObjectColor colorID;
    public Color baseColor;
    public Color onGoalColor;
}

[System.Serializable]
public class LevelConfig
{
    public GameObject levelPrefab;

    [Header("Rules & Limits")]
    [Min(0)] public int maxMoves = 20;

    [Header("Mechanics Settings")]
    public float moveDuration = 0.15f;
    public float iceSlideDuration = 0.10f;

    [Header("Economy Settings")]
    public int completionBonusGold = 100;
}

[CreateAssetMenu(fileName = "GameMechanicsConfig", menuName = "Sokoban/GameMechanicsConfig")]
public class GameMechanicsConfig : ScriptableObject
{
    [Header("Global Defaults (For Editor/Testing)")]
    public float defaultMoveDuration = 0.15f;
    public float defaultIceSlideDuration = 0.10f;
    public int defaultCompletionBonus = 100;

    [Header("Visual Color Palette")]
    public List<ColorMapping> colorMappings = new List<ColorMapping>();

    [Header("Level List")]
    public List<LevelConfig> levels = new List<LevelConfig>();
}