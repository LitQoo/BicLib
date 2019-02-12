#if BICUTIL_ADMOB
using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

namespace BicUtil.AdsManager{
    public class AdmobManager : IAdsPlatform
    {
        #region InstantData
        Dictionary<object, AdsPlatformInfo> adsData = new Dictionary<object, AdsPlatformInfo>();
        #endregion

        #region Logic
        private RewardBasedVideoAd rewardBasedVideo;
        bool isInit = false;
        public AdmobManager(){
            if(adsData.Count == 0){
                return;
            }
            
            if(isInit == true){
                return; 
            }

            isInit = true;

            rewardBasedVideo = RewardBasedVideoAd.Instance;
            rewardBasedVideo.OnAdClosed += onRewardBasedAdClosed;

        }

        public void SetAdsSettingIOSOnly(string _adsId, object _type){
            #if UNITY_IOS
            adsData[_type] = new AdsPlatformInfo(_adsId, _type);
            #endif
        }

        public void SetAdsSettingAndroidOnly(string _adsId, object _type){
            #if UNITY_ANDROID
            adsData[_type] = new AdsPlatformInfo(_adsId, _type);
            #endif
        }

        public void SetPlatformIos(string _id){
            #if UNITY_IOS
            MobileAds.Initialize(_id);
            #endif
        }

        public void SetPlatformAndroid(string _id){
            #if UNITY_ANDROID
            MobileAds.Initialize(_id);
            #endif
        }
        
        #endregion

        #region Interstitial
        public bool IsReadyInterstitial(object _adsType){
            InterstitialAd _interstitial = adsData[_adsType].Data as InterstitialAd;
            if(_interstitial != null && _interstitial.IsLoaded() == true){
                return true;
            }

            return false;
        }

        public void ShowInterstitial(object _adsType, Action<AdsResult> _callback){
            InterstitialAd _interstitial = adsData[_adsType].Data as InterstitialAd;
            
            _interstitial.OnAdFailedToLoad += (_sender, _args)=>{
                if(adsData[_adsType].Data != null){
                    _interstitial.Destroy();
                    adsData[_adsType].Data = null;
                    _callback(AdsResult.Failed);
                }
            };

            _interstitial.OnAdClosed += (_sender, _args)=>{
                if(adsData[_adsType].Data != null){
                    _interstitial.Destroy();
                    adsData[_adsType].Data = null;
                    updateLastPlayedAdsTimeAll();
                    _callback(AdsResult.Failed);
                }
            };

            _interstitial.Show();

            LoadInterstitial(_adsType);
        }

        public void LoadInterstitial(object _adsType){
            if(adsData[_adsType].Data == null){
                InterstitialAd _interstitial = new InterstitialAd(adsData[_adsType].Id);
                AdRequest _request = new AdRequest.Builder().Build();
                _interstitial.LoadAd(_request);
                adsData[_adsType].Data = _interstitial;
            }
        }
        #endregion

        #region RewardBased
        private void onRewardBasedAdClosed(object sender, EventArgs args){
            if(lastPlayedAdType != null){
                var _callback = adsData[lastPlayedAdType].Data as Action<AdsResult>;
                _callback(AdsResult.Finished);
                lastPlayedAdType = null;
            }
        }

        public void LoadRewardBased(object _adsType){
            AdRequest request = new AdRequest.Builder().Build();
            this.rewardBasedVideo.LoadAd(request, adsData[_adsType].Id);
        }

        object lastPlayedAdType = null;
        public void ShowRewardBased(object _adsType, Action<AdsResult> _callback){
            lastPlayedAdType = _adsType;
            adsData[_adsType].Data = _callback;
            rewardBasedVideo.Show();
        }

        public bool IsReadyRewardBased(object _adsType){
            if(this.rewardBasedVideo.IsLoaded() == true){
                return true;
            }

            return false;
        }
        #endregion
    }
}
#endif