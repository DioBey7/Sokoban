using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }
    private RewardedAd rewardedAd;

#if UNITY_ANDROID
    private const string REWARDED_AD_UNIT_ID = "ca-app-pub-3177849627386974/2579062848";
#elif UNITY_IPHONE
    private const string REWARDED_AD_UNIT_ID = "ca-app-pub-3940256099942544/1712485313";
#else
    private const string REWARDED_AD_UNIT_ID = "unused";
#endif

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

    private void Start()
    {
        MobileAds.Initialize(initStatus => {
            LoadRewardedAd();
        });
    }

    private void LoadRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        AdRequest adRequest = new AdRequest();

        RewardedAd.Load(REWARDED_AD_UNIT_ID, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            Debug.Log("Rewarded ad loaded.");
            if (error != null || ad == null)
            {
                Debug.LogError($"Failed to load rewarded ad: {error}");
                return;
            }
            rewardedAd = ad;
        });
    }

    public void ShowRewardedAd(Action onRewardEarned)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"User earned reward: {reward.Amount} {reward.Type}");
                onRewardEarned?.Invoke();
            });
            LoadRewardedAd();
        }
        else
        {
            LoadRewardedAd();
        }
    }
}