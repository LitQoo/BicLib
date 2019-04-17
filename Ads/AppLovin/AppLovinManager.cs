#if BICUTIL_APPLOVIN
using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;
using UnityEngine;

namespace BicUtil.Ads{
    public class AppLovinManager : IAdsPlatform
    {
        #region InstantData
        Dictionary<object, AdsPlatformInfo> adsData = new Dictionary<object, AdsPlatformInfo>();
        #endregion

        #region Callback
        public void InitializeRewardedAds()
        {
            // Attach callback
            MaxSdkCallbacks.OnRewardedAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.OnRewardedAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.OnRewardedAdLoadFailedEvent += OnRewardedAdFailedEvent;
            MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent += RewardedAdFailedToDisplayEvent;

            // Load the first RewardedAd
            LoadRewardedAd();
        }

        string rewardedAdUnitId = "";
        private void LoadRewardedAd()
        {
            MaxSdk.LoadRewardedAd(rewardedAdUnitId);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId)
        {
            // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
        }

        private void OnRewardedAdDismissedEvent(string adUnitId)
        {
            // Rewarded ad is hidden. Pre-load the next ad
            LoadRewardedAd();
        }

        private void OnRewardedAdFailedEvent(string adUnitId, int errorCode)
        {
            // Rewarded ad failed to load. We recommend re-trying in 3 seconds.
            BicTween.Delay(3f).SubscribeComplete(LoadRewardedAd);
        }

        private void RewardedAdFailedToDisplayEvent(string adUnitId, int errorCode)
        {
            // Rewarded ad failed to display. We recommend loading the next ad
            LoadRewardedAd();
        }
        #endregion
        
        #region Logic


        public void SetAdsSetting(string _unityAdsId, object _type){
            
            adsData[_type] = new AdsPlatformInfo(_unityAdsId, _type);
        }

        public void SetPlatformIos(string _id){

        }

        public void SetPlatformAndroid(string _id){

        }

        public void SetAdsSettingAndroidOnly(string _unityAdsId, object _type)
        {
            #if UNITY_ANDROID
            adsData[_type] = new AdsPlatformInfo(_unityAdsId, _type);
            #endif
        }

        public void SetAdsSettingIOSOnly(string _unityAdsId, object _type)
        {
            #if UNITY_IOS
            adsData[_type] = new AdsPlatformInfo(_unityAdsId, _type);
            #endif
        }

        public void LoadInterstitial(object _adsType)
        {
            
        }

        public bool IsReadyInterstitial(object _adsType)
        {
            return MaxSdk.IsRewardedAdReady(adsData[_adsType].PlatformId);
        }

        public void ShowInterstitial(object _adsType, Action<AdsResult> _callback)
        {
            
            MaxSdk.ShowInterstitial(adsData[_adsType].PlatformId);
            
        }

        public void LoadRewardBased(object _adsType)
        {
            
        }

        public bool IsReadyRewardBased(object _adsType)
        {
            return MaxSdk.IsRewardedAdReady(adsData[_adsType].PlatformId);
        }

        public void ShowRewardBased(object _adsType, Action<AdsResult> _callback)
        {
            MaxSdk.ShowRewardedAd(adsData[_adsType].PlatformId);
        }

        public bool IsReadyBanner(object _adsType)
        {
            return false;
        }

        string bannerAdUnitId = "";
        public IAdsBanner CreateBanner(object _adsType, Action<IAdsBanner> _onLoadBannerAction)
        {
            MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
            return null;
        }
        #endregion
    }
}
#endif