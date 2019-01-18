#if BICUTIL_UNITYADS
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

namespace BicUtil.AdsManager{
    public class UnityAdsManager : BicUtil.SingletonBase.SingletonBase<UnityAdsManager>, IAdsManager
    {
        #region InstantData
        Dictionary<object, AdsInfo> adsData = new Dictionary<object, AdsInfo>();
        #endregion

        #region Time
        private long getTimestamp(){
            return System.DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
        }

        private bool isPossiblePlayAds(object _type){
            if(getTimestamp() - adsData[_type].LastPlayedAdsTime > adsData[_type].TimeInterval){
                return true;
            }else{
                return false;
            }
        }

        public void UpdateLastPlayedAdsTime(object _type){
            adsData[_type].LastPlayedAdsTime = getTimestamp();
        }

        public void UpdateLastPlayedAdsTimeAll(){
            foreach (var _item in adsData)
            {   
                _item.Value.LastPlayedAdsTime = getTimestamp();
            }
        }
        #endregion

        #region Logic
        
        public override void Initialize(){
            UpdateLastPlayedAdsTimeAll();

        }
        
        public bool IsReady(object _adsType){
            return Advertisement.IsReady(adsData[_adsType].Id) && isPossiblePlayAds(_adsType);
        }

        public void ShowAd(object _adsType, Action<AdsResult> _callback)
        {
            var options = new ShowOptions { resultCallback = _result=>{ UpdateLastPlayedAdsTimeAll(); _callback(convert(_result));} };
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

        public void SetAdsSetting(string _adsId, object _type, int _playTimeInterval){
            
            adsData[_type] = new AdsInfo(_adsId, _type, _playTimeInterval);
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
        #endregion
    }
}
#endif