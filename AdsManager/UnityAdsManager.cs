#if BICUTIL_UNITYADS2
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

namespace BicUtil.AdsManager{
    public class UnityAdsManager : IAdsPlatform
    {
        #region InstantData
        Dictionary<object, AdsPlatformInfo> adsData = new Dictionary<object, AdsPlatformInfo>();
        #endregion
        
        #region Logic
        private void showAd(object _adsType, Action<AdsResult> _callback)
        {
            var options = new ShowOptions { 
                resultCallback = _result=>{ 
                    _callback(convert(_result));
                }
            };

            Advertisement.Show(adsData[_adsType].Id, options);
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

        public void SetAdsSetting(string _adsId, object _type){
            
            adsData[_type] = new AdsPlatformInfo(_adsId, _type);
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

        public void SetAdsSettingAndroidOnly(string _adsId, object _type)
        {
            #if UNITY_ANDROID
            adsData[_type] = new AdsPlatformInfo(_adsId, _type);
            #endif
        }

        public void SetAdsSettingIOSOnly(string _adsId, object _type)
        {
            #if UNITY_IOS
            adsData[_type] = new AdsPlatformInfo(_adsId, _type);
            #endif
        }

        public void LoadInterstitial(object _adsType)
        {
            
        }

        public bool IsReadyInterstitial(object _adsType)
        {
            return Advertisement.IsReady(adsData[_adsType].Id);
        }

        public void ShowInterstitial(object _adsType, Action<AdsResult> _callback)
        {
            showAd(_adsType, _callback);
        }

        public void LoadRewardBased(object _adsType)
        {
            
        }

        public bool IsReadyRewardBased(object _adsType)
        {
            return Advertisement.IsReady(adsData[_adsType].Id);
        }

        public void ShowRewardBased(object _adsType, Action<AdsResult> _callback)
        {
            showAd(_adsType, _callback);
        }

        public IAdsBanner CreateBanner(object _adsType)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
#endif