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
            LevelDataPayload payload = LevelScanner.Scan(root.transform);

            if (payload != null)
            {
                Debug.Log("<color=green>Level validation successful! Matrix is fully synchronized.</color>");
            }
        }
    }
}