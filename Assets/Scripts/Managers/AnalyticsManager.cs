using UnityEngine;
using Firebase;
using Firebase.Analytics;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }
    private bool isFirebaseInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            if (task.Result == DependencyStatus.Available)
            {
                isFirebaseInitialized = true;
                Debug.Log("<color=green>FIREBASE INITIALIZED SUCCESSFULLY!</color>");
            }
            else
            {
                Debug.LogError($"FIREBASE FAILED TO INITIALIZE: {task.Result}");
            }
        });
    }

    public void LogAction(string eventName)
    {
        if (isFirebaseInitialized)
        {
            FirebaseAnalytics.LogEvent(eventName);
            Debug.Log($"<color=cyan>FIREBASE LOG SENT: {eventName}</color>");
        }
    }

    public void LogActionWithParam(string eventName, string paramName, string paramValue)
    {
        if (isFirebaseInitialized)
        {
            FirebaseAnalytics.LogEvent(eventName, new Parameter(paramName, paramValue));
            Debug.Log($"<color=cyan>FIREBASE LOG SENT: {eventName} | {paramName}:{paramValue}</color>");
        }
    }
}