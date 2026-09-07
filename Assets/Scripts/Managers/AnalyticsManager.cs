using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions; 
using System.Collections.Generic;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }
    private bool isFirebaseInitialized = false;
    private Queue<System.Action> pendingEvents = new Queue<System.Action>();

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
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                isFirebaseInitialized = true;
                Debug.Log("<color=green>FIREBASE INITIALIZED SUCCESSFULLY ON MAIN THREAD!</color>");

                while (pendingEvents.Count > 0)
                {
                    pendingEvents.Dequeue()?.Invoke();
                }
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
        else
        {
            pendingEvents.Enqueue(() => LogAction(eventName));
        }
    }

    public void LogActionWithParam(string eventName, string paramName, string paramValue)
    {
        if (isFirebaseInitialized)
        {
            FirebaseAnalytics.LogEvent(eventName, new Parameter(paramName, paramValue));
            Debug.Log($"<color=cyan>FIREBASE LOG SENT: {eventName} | {paramName}:{paramValue}</color>");
        }
        else
        {
            pendingEvents.Enqueue(() => LogActionWithParam(eventName, paramName, paramValue));
        }
    }
}