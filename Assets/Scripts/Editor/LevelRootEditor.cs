using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelRoot))]
public class LevelRootEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelRoot root = (LevelRoot)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Validate Level", GUILayout.Height(30)))
        {
            GameMechanicsConfig config = Resources.Load<GameMechanicsConfig>("GameMechanicsConfig");

            LevelDataPayload payload = LevelScanner.Scan(root.transform, config);

            if (payload != null)
            {
                Debug.Log("<color=green>Level validation successful! Matrix is fully synchronized.</color>");
            }
            else
            {
                Debug.LogError("<color=red>Level validation failed! Ensure a Player object exists.</color>");
            }
        }
    }
}