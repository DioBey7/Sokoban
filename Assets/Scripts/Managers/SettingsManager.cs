using UnityEngine;

public static class SettingsManager
{
    public static bool IsMusicOn
    {
        get => PlayerPrefs.GetInt("IsMusicOn", 1) == 1;
        set => PlayerPrefs.SetInt("IsMusicOn", value ? 1 : 0);
    }

    public static bool IsSFXOn
    {
        get => PlayerPrefs.GetInt("IsSFXOn", 1) == 1;
        set => PlayerPrefs.SetInt("IsSFXOn", value ? 1 : 0);
    }

    public static bool IsHapticsOn
    {
        get => PlayerPrefs.GetInt("IsHapticsOn", 1) == 1;
        set => PlayerPrefs.SetInt("IsHapticsOn", value ? 1 : 0);
    }
}