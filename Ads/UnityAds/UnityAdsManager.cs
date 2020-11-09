#if BICUTIL_UNITYADS2
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

namespace BicUtil.Ads{
    public class UnityAdsManager : IAdsPlatform
    {
        public string TermsURL => "https://unity3d.com/legal/privacy-policy";
        public void SetUserConsent(bool _isEnabled){

        }

        #region InstantData
        Dictionary<object, AdsPlatformInfo> adsData = new Dictionary<object, AdsPlatformInfo>();
        #endregion
        
        #region Logic
        private void showAd(object _adsPlacement, Action<AdsResult> _callback)
        {
            var options = new ShowOptions { 
                resultCallback = _result=>{ 
                    _callback(convert(_result));
                }
            };

            Advertisement.Show(adsData[_adsPlacement].PlatformId, options);
        }

        private AdsResult convert(ShowResult _result){
            switch(_result){
                case ShowResult.Failed:
                    return AdsResult.Failed;
                case ShowResult.Finished:
                    return AdsResult.Finished;
                case ShowResult.Skipped:
                    return AdsResult.Skipped;
            }

            return AdsResult.Failed;
        }

        public void SetAdsSetting(string _unityAdsId, object _adsPlacement){
            
            adsData[_adsPlacement] = new AdsPlatformInfo(_unityAdsId, _adsPlacement);
        }

        public void SetPlatformIos(string _id){
            #if UNITY_IOS
            Advertisement.Initialize(_id, false);
            #endif
        }

        public void SetPlatformAndroid(string _id){
            #if UNITY_ANDROID
            Advertisement.Initialize(_id, false);
            #endif
        }

        public void SetAdsSettingAndroidOnly(string _unityAdsId, object _adsPlacement)
        {
            #if UNITY_ANDROID
            adsData[_adsPlacement] = new AdsPlatformInfo(_unityAdsId, _adsPlacement);
            #endif
        }

        public void SetAdsSettingIOSOnly(string _unityAdsId, object _adsPlacement)
        {
            #if UNITY_IOS
            adsData[_adsPlacement] = new AdsPlatformInfo(_unityAdsId, _adsPlacement);
            #endif
        }

        public void LoadInterstitial(object _adsPlacement)
        {
            
        }

        public bool IsReadyInterstitial(object _adsPlacement)
        {
            return Advertisement.IsReady(adsData[_adsPlacement].PlatformId);
        }

        public void ShowInterstitial(object _adsPlacement, Action<AdsResult> _callback)
        {
            showAd(_adsPlacement, _callback);
        }

        public void LoadRewardBased(object _adsPlacement)
        {
            
        }

        public bool IsReadyRewardBased(object _adsPlacement)
        {
            return Advertisement.IsReady(adsData[_adsPlacement].PlatformId);
        }

        public void ShowRewardBased(object _adsPlacement, Action<AdsResult> _callback)
        {
            showAd(_adsPlacement, _callback);
        }

        public bool IsReadyBanner(object _adsPlacement)
        {
            return false;
        }

        public IAdsBanner CreateBanner(object _adsPlacement, Color _backColor, Action<IAdsBanner> _onLoadBannerAction)
        {
            return null;
        }
        #endregion
    }
}
#endif