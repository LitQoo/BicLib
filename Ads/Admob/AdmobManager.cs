#if BICUTIL_ADMOB
using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;
using GoogleMobileAds.Api;
using UnityEngine;

namespace BicUtil.Ads{
    public class AdmobManager : IAdsPlatform
    {
        #region InstantData
        private Dictionary<object, AdsPlatformInfo> adsData = new Dictionary<object, AdsPlatformInfo>();
        #endregion

        #region Logic
        private RewardBasedVideoAd rewardBasedVideo;
        public AdmobManager(string _androidAppId, string _iosAppId){
            #if UNITY_IOS
            MobileAds.Initialize(_iosAppId);
            #endif

            #if UNITY_ANDROID
            MobileAds.Initialize(_androidAppId);
            #endif

            rewardBasedVideo = RewardBasedVideoAd.Instance;
            rewardBasedVideo.OnAdClosed += onRewardBasedAdClosed;
            rewardBasedVideo.OnAdFailedToLoad += reloadRewardBased;

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
        
        #endregion

        #region Interstitial
        public bool IsReadyInterstitial(object _adsType){
            LoadInterstitial(_adsType);

            InterstitialAd _interstitial = adsData[_adsType].Data as InterstitialAd;
            if(_interstitial != null && _interstitial.IsLoaded() == true){
                return true;
            }

            return false;
        }

        public void ShowInterstitial(object _adsType, Action<AdsResult> _callback){
            InterstitialAd _interstitial = adsData[_adsType].Data as InterstitialAd;
            object __adsType = _adsType;
            _interstitial.OnAdClosed += (_sender, _args)=>{
                if(adsData[__adsType].Data != null){
                    _interstitial.Destroy();
                    adsData[__adsType].Data = null;
                    BicTween.Delay(0.5f).SubscribeComplete(()=>{
                        LoadInterstitial(__adsType);
                    });

                    _callback(AdsResult.Finished);
                }
            };

            _interstitial.Show();

        }

        public void LoadInterstitial(object _adsType){
            loadInterstitial(_adsType, 1);
        }

        public void loadInterstitial(object _adsType, int _time){
            if(adsData[_adsType].Data == null){
                InterstitialAd _interstitial = new InterstitialAd(adsData[_adsType].PlatformId);
                AdRequest _request = new AdRequest.Builder().Build();
                int __time = _time;
                object __adsType = _adsType;
                _interstitial.OnAdFailedToLoad += (_sender, _args)=>{
                    _interstitial.Destroy();
                    adsData[_adsType].Data = null;
                    BicTween.Delay(1f).SubscribeComplete(()=>{
                        loadInterstitial(__adsType, __time * 2);
                    });
                };

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
            }

            reloadTime = 1;
            loadRewardBased(lastPlayedAdType, 0);
        }

        public void LoadRewardBased(object _adsType){
            if(lastPlayedAdType == null || lastPlayedAdType != _adsType){
                loadRewardBased(_adsType, 0);
            }
        }

        object lastPlayedAdType = null;
        int reloadTime = 1;
        public void ShowRewardBased(object _adsType, Action<AdsResult> _callback){
            lastPlayedAdType = _adsType;
            adsData[_adsType].Data = _callback;
            rewardBasedVideo.Show();
        }

        private void reloadRewardBased(object sender, AdFailedToLoadEventArgs e)
        {
            loadRewardBased(lastPlayedAdType, reloadTime * 2);
        }

        private void loadRewardBased(object _adsType, int _time){
            object __adsType = _adsType;
            lastPlayedAdType = __adsType;
            
            if(_time == 0){
                loadRewardBased(__adsType);
            }else{
                BicTween.Delay(_time).SubscribeComplete(()=>{
                    loadRewardBased(__adsType);
                });
            }
        }

        private void loadRewardBased(object _adsType){
            AdRequest request = new AdRequest.Builder().Build();
            this.rewardBasedVideo.LoadAd(request, adsData[_adsType].PlatformId);
        }

        public bool IsReadyRewardBased(object _adsType){
            LoadRewardBased(_adsType);
            return this.rewardBasedVideo.IsLoaded();
        }
        #endregion

        #region Banner
        private BannerView bannerView;
        
        public bool IsReadyBanner(object _adsType){
            return true;
        }
        public IAdsBanner CreateBanner(object _adsType, Action<IAdsBanner> _onLoadBannerAction)
        {
            if(adsData.ContainsKey(_adsType) == true){
                var _banner = MonoBehaviour.Instantiate(Resources.Load<AdmobBannerController>("AdmobBanner"));
                _banner.Load(adsData[_adsType].PlatformId, _adsType, _onLoadBannerAction);
                return _banner;
            }else{
                return null;
            }
        }

        #endregion
    }
}
#endif