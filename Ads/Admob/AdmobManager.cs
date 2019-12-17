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

        public AdmobManager(){
            MobileAds.Initialize(_initState=>{

            });   
        }

        public AdmobManager(string _androidAppId, string _iosAppId){
            #if UNITY_IOS
            MobileAds.Initialize(_iosAppId);
            #endif

            #if UNITY_ANDROID
            MobileAds.Initialize(_androidAppId);
            #endif
        }

        public void SetAdsSettingIOSOnly(string _adsId, object[] _types){
            for(int i = 0; i < _types.Length; i++){
                SetAdsSettingIOSOnly(_adsId, _types[i]);
            }
        }

        public void SetAdsSettingIOSOnly(string _adsId, object _type){
            #if UNITY_IOS
            adsData[_type] = new AdsPlatformInfo(_adsId, _type);
            #endif
        }


        public void SetAdsSettingAndroidOnly(string _adsId, object[] _types){
            for(int i = 0; i < _types.Length; i++){
                SetAdsSettingAndroidOnly(_adsId, _types[i]);
            }
        }

        public void SetAdsSettingAndroidOnly(string _adsId, object _type){
            #if UNITY_ANDROID
            adsData[_type] = new AdsPlatformInfo(_adsId, _type);
            #endif
        }
        
        #endregion

        #region Interstitial
        public bool IsReadyInterstitial(object _adsType){

            InterstitialAd _interstitial = adsData[_adsType].Data as InterstitialAd;
            if(_interstitial != null && _interstitial.IsLoaded() == true){
                return true;
            }

            LoadInterstitial(_adsType);
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
        public void LoadRewardBased(object _adsType){
            loadRewardBased(_adsType, 1);
        }

        public void loadRewardBased(object _adsType, int _time){
            if(adsData[_adsType].Data == null){
                RewardedAd _rewardedAd = new RewardedAd(adsData[_adsType].PlatformId);
                AdRequest _request = new AdRequest.Builder().Build();
                int __time = _time;
                object __adsType = _adsType;

                _rewardedAd.OnAdFailedToLoad += (_sender, _args)=>{
                    adsData[_adsType].Data = null;
                    BicTween.Delay(1f).SubscribeComplete(()=>{
                        loadRewardBased(__adsType, __time * 2);
                    });
                };

                _rewardedAd.LoadAd(_request);
                adsData[_adsType].Data = _rewardedAd;
            }
        }

        private bool isSuccessRewarded = false;
        int reloadTime = 1;
        public void ShowRewardBased(object _adsType, Action<AdsResult> _callback){
            RewardedAd _rewardedAd = adsData[_adsType].Data as RewardedAd;
            object __adsType = _adsType;
            isSuccessRewarded = false;
            _rewardedAd.OnUserEarnedReward += (_sender, _args)=>{
                isSuccessRewarded = true;
            };

            _rewardedAd.OnAdClosed += (_sender, _args)=>{
                if(adsData[__adsType].Data != null){
                    adsData[__adsType].Data = null;
                    BicTween.Delay(0.5f).SubscribeComplete(()=>{
                        LoadRewardBased(__adsType);
                    });

                    if(_callback != null){
                        if(isSuccessRewarded == true){
                            _callback(AdsResult.Finished);
                        }else{
                            _callback(AdsResult.Skipped);
                        }

                        _callback = null;
                    }
                }
            };

            _rewardedAd.OnAdFailedToShow += (_sender, _args)=>{
                if(_callback != null){
                    _callback(AdsResult.Failed);
                    _callback = null;
                }
            };

            _rewardedAd.Show();
        }


        public bool IsReadyRewardBased(object _adsType){
            RewardedAd _rewardedAd = adsData[_adsType].Data as RewardedAd;
            if(_rewardedAd != null && _rewardedAd.IsLoaded() == true){
                return true;
            }
            
            LoadRewardBased(_adsType);
            return false;
        }
        #endregion

        #region Banner
        private BannerView bannerView;
        
        public bool IsReadyBanner(object _adsType){
            return true;
        }
        public IAdsBanner CreateBanner(object _adsType, Color _backColor, Action<IAdsBanner> _onLoadBannerAction)
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