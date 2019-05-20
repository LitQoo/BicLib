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

        #region LifeCycle
        public AppLovinManager(string _sdkKey){
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => {
                Debug.Log("MaxSdk : " + sdkConfiguration.ConsentDialogState.ToString());
            };

            MaxSdk.SetSdkKey(_sdkKey);
            MaxSdk.InitializeSdk();

            InitializeRewardedAds();
            InitializeInterstitialAds();
        }
        #endregion

        #region InterstitalCallback
        public void InitializeInterstitialAds()
        {
            // Attach callback
            MaxSdkCallbacks.OnInterstitialLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.OnInterstitialLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent += InterstitialFailedToDisplayEvent;
            MaxSdkCallbacks.OnInterstitialHiddenEvent += OnInterstitialDismissedEvent;
        }

        private void OnInterstitialLoadedEvent(string adUnitId)
        {
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
        }

        private void OnInterstitialFailedEvent(string adUnitId, int errorCode)
        {
            var _adId = adUnitId;
            BicTween.Delay(3f).SubscribeComplete(()=>{
                MaxSdk.LoadInterstitial(_adId);
            });
        }

        private void InterstitialFailedToDisplayEvent(string adUnitId, int errorCode)
        {
            if(callback != null){
                callback(AdsResult.Failed);
            }

            // Interstitial ad failed to display. We recommend loading the next ad
            MaxSdk.LoadInterstitial(adUnitId);
        }

        private void OnInterstitialDismissedEvent(string adUnitId)
        {
            if(callback != null){
                callback(AdsResult.Finished);
            }

            // Interstitial ad is hidden. Pre-load the next ad
            MaxSdk.LoadInterstitial(adUnitId);
        }
        #endregion

        #region RewarededCallback
        public void InitializeRewardedAds()
        {
            // Attach callback
            MaxSdkCallbacks.OnRewardedAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.OnRewardedAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.OnRewardedAdLoadFailedEvent += OnRewardedAdFailedLoadEvent;
            MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent += RewardedAdFailedToDisplayEvent;
        }

        private void OnRewardedAdLoadedEvent(string adUnitId)
        {
            // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
        }

        private void OnRewardedAdDismissedEvent(string adUnitId)
        {
            if(callback != null){
                callback(AdsResult.Finished);
            }

            // Rewarded ad is hidden. Pre-load the next ad
            MaxSdk.LoadRewardedAd(adUnitId);
        }

        private void OnRewardedAdFailedLoadEvent(string adUnitId, int errorCode)
        {
            var _adId = adUnitId;
            // Rewarded ad failed to load. We recommend re-trying in 3 seconds.
            BicTween.Delay(3f).SubscribeComplete(()=>{
                MaxSdk.LoadRewardedAd(_adId);
            });
        }

        private void RewardedAdFailedToDisplayEvent(string adUnitId, int errorCode)
        {
            if(callback != null){
                callback(AdsResult.Failed);
            }

            // Rewarded ad failed to display. We recommend loading the next ad
            MaxSdk.LoadRewardedAd(adUnitId);
        }
        #endregion
        
        #region Logic
        private Action<AdsResult> callback;

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
            MaxSdk.LoadInterstitial(adsData[_adsType].PlatformId);
        }

        public bool IsReadyInterstitial(object _adsType)
        {
            return MaxSdk.IsInterstitialReady(adsData[_adsType].PlatformId);
        }

        public void ShowInterstitial(object _adsType, Action<AdsResult> _callback)
        {
            Debug.LogWarning("ShowInterstitial " + _adsType.ToString() + "/" + adsData[_adsType].PlatformId);
            callback = _callback;
            MaxSdk.ShowInterstitial(adsData[_adsType].PlatformId);   
        }

        public void LoadRewardBased(object _adsType)
        {
            MaxSdk.LoadRewardedAd(adsData[_adsType].PlatformId);
        }

        public bool IsReadyRewardBased(object _adsType)
        {
            return MaxSdk.IsRewardedAdReady(adsData[_adsType].PlatformId);
        }

        public void ShowRewardBased(object _adsType, Action<AdsResult> _callback)
        {
            callback = _callback;
            MaxSdk.ShowRewardedAd(adsData[_adsType].PlatformId);
        }

        public bool IsReadyBanner(object _adsType)
        {
            return true;
        }

        public IAdsBanner CreateBanner(object _adsType, Action<IAdsBanner> _onLoadBannerAction)
        {
            if(adsData.ContainsKey(_adsType) == true){
                var _banner = new AppLovinBannerController();
                _banner.Load(adsData[_adsType].PlatformId, _adsType);
                return _banner;
            }else{
                return null;
            }
        }
        #endregion
    }
}
#endif