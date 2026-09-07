#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class EditorSaveManager
{
    [MenuItem("DevTools/Hard Reset")]
    public static void ClearAllSaves()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("<color=red>SYSTEM: ALL SAVES (Gold, Undo, Levels) deleted.</color> The game is now in its initial state.");
    }
}
#endif