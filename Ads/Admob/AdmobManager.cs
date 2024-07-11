#if BICUTIL_ADMOB
using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace BicUtil.Ads{
    public class AdmobManager : IAdsPlatform
    {
        public string TermsURL => "https://policies.google.com/privacy/update";

        #region InstantData
        private Dictionary<object, AdsPlatformInfo> adsData = new Dictionary<object, AdsPlatformInfo>();
        #endregion

        #region Logic

        public AdmobManager(bool _initialize){
            if(_initialize == true){
                MobileAds.Initialize(_initState=>{

                });   
            }
        }

        public void Initialize(Action _action){
            Debug.Log("[Admob] start Initialize");
            UpdateConsent(_result=>{
                Debug.Log("[Admob] UpdateConsent result: "+_result.ToString());

                MobileAds.Initialize(_initState=>{
                    Debug.Log("[Admob] Initialize result: "+_initState.ToString());
                    _action();
                });
            });
        }

        public void UpdateConsent(Action<ConsentResult> _callback){
            Debug.Log("[Admob] start UpdateConsent");
            var debugSettings = new ConsentDebugSettings
            {
                DebugGeography = DebugGeography.EEA,
                TestDeviceHashedIds =
                new List<string>
                {
                    "A9178F1E-C3E8-407D-9A54-844E7CDF7FA8",
                    "366fac27-a1ef-4c95-b5eb-e313cf98d35c",
                    "9104b117-10b9-46ac-835f-a89cf6ef4f37",
                    "83049E61DFD375AB64940E8710FE94A1"
                }
            };

            ConsentRequestParameters request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
                ConsentDebugSettings = debugSettings,
            };

            ConsentInformation.Update(request, _error=>OnConsentInfoUpdated(_error, _callback));
        }

        void OnConsentInfoUpdated(FormError consentError, Action<ConsentResult> _callback)
        {

            Debug.Log("[Admob] start OnConsentInfoUpdated");
            if (consentError != null)
            {
                // Handle the error.
                UnityEngine.Debug.LogError("[Admob] consentError1 " + consentError);
                _callback(ConsentResult.ConsentUpdateError);
                return;
            }

            // If the error is null, the consent information state was updated.
            // You are now ready to check if a form is available.
            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
            {
                if (formError != null)
                {
                    // Consent gathering failed.
                    UnityEngine.Debug.LogError("[Admob] consentError2 " + consentError);
                    _callback(ConsentResult.ConsentShowFormError);
                    return;
                }

                // Consent has been gathered.
                if (ConsentInformation.CanRequestAds())
                {
                   _callback(ConsentResult.Success);
                }else{
                    _callback(ConsentResult.ConsentRequestError);
                }
            });
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


        public void SetAdsSettingEditorOnly(string _adsId, object[] _types){
            for(int i = 0; i < _types.Length; i++){
                SetAdsSettingEditorOnly(_adsId, _types[i]);
            }
        }

        public void SetAdsSettingEditorOnly(string _adsId, object _type){
            #if UNITY_EDITOR
            adsData[_type] = new AdsPlatformInfo(_adsId, _type);
            #endif
        }
        
        #endregion

        #region Interstitial
        public bool IsReadyInterstitial(object _adsType){

            InterstitialAd _interstitial = adsData[_adsType].Data as InterstitialAd;
            if(_interstitial != null && _interstitial.CanShowAd() == true){
                return true;
            }

            LoadInterstitial(_adsType);
            return false;
        }

        public void ShowInterstitial(object _adsType, Action<AdsResult> _callback){
            InterstitialAd _interstitial = adsData[_adsType].Data as InterstitialAd;
            object __adsType = _adsType;
            Action _reload = ()=>{
                if(adsData[__adsType].Data != null){
                    _interstitial.Destroy();
                    adsData[__adsType].Data = null;

                    BicTween.RunOnMainThread(()=>{
                        BicTween.Delay(0.5f).SubscribeComplete(()=>{
                            LoadInterstitial(__adsType);
                        });

                        _callback(AdsResult.Finished);
                    });
                }
            };

            _interstitial.OnAdFullScreenContentClosed += _reload;
            _interstitial.OnAdFullScreenContentFailed += (AdError)=>{
                _reload();
            };

            _interstitial.Show();

        }

        public void LoadInterstitial(object _adsType){
            loadInterstitial(_adsType, 1);
        }

        public void loadInterstitial(object _adsType, float _time){
            if(adsData[_adsType].Data == null)
            {
                float __time = _time;
                object __adsType = _adsType;
                var _request = buildRequest();
                InterstitialAd.Load(adsData[_adsType].PlatformId, _request, (_ad, _error)=>{
                    adsData[__adsType].Data = _ad;

                    if(_error != null || _ad == null){
                        if(_ad != null){
                            _ad.Destroy();
                        }

                        adsData[__adsType].Data = null;

                        BicTween.RunOnMainThread(() =>
                        {
                            BicTween.Delay(__time).SubscribeComplete(() =>
                            {
                                loadInterstitial(__adsType, Mathf.Min(__time * 2, 300f));
                            });
                        });
                    }
                });
            }
        }
        #endregion

        #region RewardBased
        private Dictionary<string, RewardedAd> rewardedAdLoader = new Dictionary<string, RewardedAd>();

        public void LoadRewardBased(object _adsType){
            var _adsId = adsData[_adsType].PlatformId;
            if(rewardedAdLoader.ContainsKey(_adsId) == false ){
                rewardedAdLoader.Add(_adsId, null);
            }

            loadRewardBased(_adsId, 1);
        }

        public void loadRewardBased(string _adsId, float _time){
            if(rewardedAdLoader.ContainsKey(_adsId) == false || rewardedAdLoader[_adsId] == null){
                AdRequest _request = buildRequest();
                
                float __time = _time;
                string __adsId = _adsId;

                RewardedAd.Load(__adsId, _request, (_ad,_error)=>{
                    rewardedAdLoader[_adsId] = _ad;

                    if(_error != null || _ad == null){
                        if(_ad != null){
                            _ad.Destroy();
                        }

                        rewardedAdLoader[_adsId] = null;

                        BicTween.RunOnMainThread(()=>{
                            BicTween.Delay(__time).SubscribeComplete(()=>{
                                loadRewardBased(__adsId, Mathf.Min(__time * 2, 300f));
                            });
                        });
                    }
                });
            }
        }

        private bool isSuccessRewarded = false;
        int reloadTime = 1;
        public void ShowRewardBased(object _adsType, Action<AdsResult> _callback){
            var _adsId = adsData[_adsType].PlatformId;
            RewardedAd _rewardedAd = rewardedAdLoader[_adsId];
            object __adsType = _adsType;
            isSuccessRewarded = false;

            _rewardedAd.OnAdImpressionRecorded += ()=>{
                isSuccessRewarded = true;
            };

            _rewardedAd.OnAdFullScreenContentClosed += ()=>{
                _rewardedAd.Destroy();
                rewardedAdLoader[_adsId] = null;
                LoadRewardBased(__adsType);

                 if(_callback != null){
                    var __callback = _callback;
                    _callback = null;
                    if(isSuccessRewarded == true){
                        BicTween.RunOnMainThread(()=>__callback(AdsResult.Finished));
                    }else{
                        BicTween.RunOnMainThread(()=>__callback(AdsResult.Skipped));
                    }
                }
            };

            _rewardedAd.OnAdFullScreenContentFailed += (_error)=>{
                _rewardedAd.Destroy();
                rewardedAdLoader[_adsId] = null;
                LoadRewardBased(__adsType);

                if(_callback != null){
                    var __callback = _callback;
                    _callback = null;
                    BicTween.RunOnMainThread(()=>__callback(AdsResult.Failed));
                }
            };

            _rewardedAd.Show(_reward=>{
            });
        }


        public bool IsReadyRewardBased(object _adsType){
            var _adsId = adsData[_adsType].PlatformId;
            if(rewardedAdLoader.ContainsKey(_adsId) == true){
                RewardedAd _rewardedAd = rewardedAdLoader[_adsId];
                if(_rewardedAd != null && _rewardedAd.CanShowAd() == true){
                    return true;
                }
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
                _banner.Load(adsData[_adsType].PlatformId, _adsType, _onLoadBannerAction, buildRequest());
                return _banner;
            }else{
                return null;
            }
        }

        #endregion

        #region UserConsent
        private bool isUserConsent = true;
        public void SetUserConsent(bool _isEnabled){
            isUserConsent = _isEnabled;
        }

        private AdRequest buildRequest()
        {
            var _request = new AdRequest();
            return _request;
        }
        #endregion
    }
}
#endif