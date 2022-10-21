#if BICUTIL_APPLOVIN
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BicDB;
using BicDB.Container;
using BicDB.Storage;
using BicUtil.Json;
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
            
            if(MaxSdk.IsInitialized() == false){
                MaxSdk.SetSdkKey(_sdkKey);
                MaxSdk.InitializeSdk();
            }

            initializeRewardedAds();
            InitializeInterstitialAds();
        }
        #endregion

        #region InterstitalCallback
        public void InitializeInterstitialAds()
        {
            // Attach callback
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo _adInfo)
        {
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo _errorInfo)
        {
            var _adId = adUnitId;
            BicTween.Delay(3f).SubscribeComplete(()=>{
                MaxSdk.LoadInterstitial(_adId);
            });
        }

        private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo _errorInfo, MaxSdkBase.AdInfo _adInfo)
        {
            if(callback != null){
                callback(AdsResult.Failed);
            }

            // Interstitial ad failed to display. We recommend loading the next ad
            MaxSdk.LoadInterstitial(adUnitId);
        }

        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo _adInfo)
        {
            if(callback != null){
                callback(AdsResult.Finished);
            }

            // Interstitial ad is hidden. Pre-load the next ad
            MaxSdk.LoadInterstitial(adUnitId);
        }
        #endregion

        #region RewarededCallback
        private bool isSuccessRewarded = false;
        private void initializeRewardedAds()
        {
            // Attach callback
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedLoadEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += RewardedAdFailedToDisplayEvent;
        }

        private void OnRewardedAdReceivedRewardEvent(string arg1, MaxSdkBase.Reward arg2, MaxSdkBase.AdInfo arg3)
        {
            isSuccessRewarded = true;
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo _adInfo)
        {
            // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
        }

        private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo _adInfo)
        {
            if(callback != null){
                if(isSuccessRewarded == true){
                    callback(AdsResult.Finished);
                }else{
                    callback(AdsResult.Skipped);
                }
            }

            // Rewarded ad is hidden. Pre-load the next ad
            MaxSdk.LoadRewardedAd(adUnitId);

        }

        private void OnRewardedAdFailedLoadEvent(string adUnitId, MaxSdkBase.ErrorInfo _errorInfo)
        {
            isSuccessRewarded = false;
            var _adId = adUnitId;
            // Rewarded ad failed to load. We recommend re-trying in 3 seconds.
            BicTween.Delay(3f).SubscribeComplete(()=>{
                MaxSdk.LoadRewardedAd(_adId);
            });
        }

        private void RewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo _errorInfo, MaxSdkBase.AdInfo _adInfo)
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

        public void SetAdsSetting(string _unityAdsId, object _adsPlacement){
            
            adsData[_adsPlacement] = new AdsPlatformInfo(_unityAdsId, _adsPlacement);
        }

        public void SetPlatformIos(string _id){

        }

        public void SetPlatformAndroid(string _id){

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
            MaxSdk.LoadInterstitial(adsData[_adsPlacement].PlatformId);
        }

        public bool IsReadyInterstitial(object _adsPlacement)
        {
            return MaxSdk.IsInterstitialReady(adsData[_adsPlacement].PlatformId);
        }

        public void ShowInterstitial(object _adsPlacement, Action<AdsResult> _callback)
        {
            callback = _callback;
            MaxSdk.ShowInterstitial(adsData[_adsPlacement].PlatformId);   
        }

        public void LoadRewardBased(object _adsPlacement)
        {
            MaxSdk.LoadRewardedAd(adsData[_adsPlacement].PlatformId);
        }

        public bool IsReadyRewardBased(object _adsPlacement)
        {
            return MaxSdk.IsRewardedAdReady(adsData[_adsPlacement].PlatformId);
        }

        public void ShowRewardBased(object _adsPlacement, Action<AdsResult> _callback)
        {
            isSuccessRewarded = false;
            callback = _callback;
            MaxSdk.ShowRewardedAd(adsData[_adsPlacement].PlatformId);
        }

        public bool IsReadyBanner(object _adsPlacement)
        {
            return true;
        }

        public IAdsBanner CreateBanner(object _adsPlacement, Color _backColor, Action<IAdsBanner> _onLoadBannerAction)
        {
            if(adsData.ContainsKey(_adsPlacement) == true){
                var _banner = new AppLovinBannerController();
                _banner.Load(adsData[_adsPlacement].PlatformId, _adsPlacement, _backColor);
                return _banner;
            }else{
                return null;
            }
        }


        public string TermsURL => "https://www.applovin.com/terms/";
        public void SetUserConsent(bool _isEnabled)
        {
            
        }
        #endregion
    }


    public class MaxABTestStorage : ITableStorage {
        public enum ResultCode
        {
            Success = 0,
            FailedConvertJson = 1
        }
        #region static
        static private ITableStorage instance = null;
        static public ITableStorage GetInstance(){
            if (instance == null) {
                instance = new MaxABTestStorage();
            }

            return instance;
        }
        #endregion

        #region IStorage
        public string StorageType{get{ return "MaxABTestStorage"; }}

        public void Save<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
            if (_callback != null) {
                _callback(new Result((int)ResultCode.Success, string.Empty));
            }
        }

        public void Push<T>(ITableContainer<T> _table, Action<Result> _callback) where T : IRecordContainer, new()
        {
            Save(_table, _callback);
        }

        public void Pull<T>(ITableContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new ()
        {
            loadByMaxSdk(_table, _callback, _parameter);
        }

        public void Load<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
            _table.Clear();
            loadByMaxSdk(_table, _callback, _parameter);
        }

        private void loadByMaxSdk<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new()
        {
            string _sdkKey = string.Empty;
            string _abTestKey = string.Empty;
            string _defultData = string.Empty;
            Func<string, string> _valueSetter = null;
            if (_parameter != null)
            {
                MaxABTestStorageParameter _param = _parameter as MaxABTestStorageParameter;
                _sdkKey = _param.SdkKey;
                _abTestKey = _param.ABTestKey;
                _defultData = _param.DefaultValue;
                _valueSetter = _param.ValueSetter;
            }

            if (MaxSdk.IsInitialized() == true)
            {
                Debug.LogWarning("[pixaw] max init already");
                loadData(_table, _callback, _abTestKey, _defultData, _valueSetter);
            }
            else
            {   
                Debug.LogWarning("[pixaw] max init start");
                MaxSdkCallbacks.OnSdkInitializedEvent += _config=>{
                    Debug.LogWarning("[pixaw] OnSdkInitializedEvent " + _config.ConsentDialogState.ToString());
                    
                    if(isLoaded == true){
                        return;
                    }

                    isLoaded = true;
                    loadData(_table, _callback, _abTestKey, _defultData, _valueSetter);
                };

                MaxSdkCallbacks.OnVariablesUpdatedEvent += ()=>{
                    Debug.LogWarning("[pixaw] OnVariablesUpdatedEvent " + MaxSdk.VariableService.GetString(_abTestKey, "not"));
                };

                MaxSdk.SetSdkKey(_sdkKey);
                MaxSdk.InitializeSdk();
            }

        }
        
        private bool isLoaded = false;
        private static void loadData<T>(ITableContainer<T> _table, Action<Result> _callback, string _abTestKey, string _defultData, Func<string, string> _valueSetter) where T : IRecordContainer, new()
        {
            throw new NotImplementedException();
            // string _data = MaxSdk.VariableService.GetString(_abTestKey, _defultData);
            // var _result = new Result((int)ResultCode.Success, string.Empty);
            // int _counter = 0;


            // try{
            //     if(_data == _defultData){
            //         //load by file if exist
            //         var _dataFromFile = FileStorage.ReadByPath(FileStorage.GetPath("maxab_"+_table.Name), "abtestkey");
            //         if(string.IsNullOrEmpty(_dataFromFile) == false){
            //             _data = _dataFromFile;
            //         }
            //     }else if(string.IsNullOrEmpty(_data) == false){
            //         //save to file
            //         FileStorage.Write(_data, "maxab_"+_table.Name, "abtestkey");
            //     }
            // }catch{

            // }

            // string _jsonData = _valueSetter(_data);

            // if (!string.IsNullOrEmpty(_jsonData))
            // {
            //     try
            //     {
            //         JsonConvertor.GetInstance().BuildTableContainer(_table, ref _jsonData, ref _counter);
            //     }
            //     catch (Exception e)
            //     {
            //         _result.Code = (int)ResultCode.FailedConvertJson;
            //         _result.Message = "FailedConvertJson " + e.Message + "/" + e.ToString();
            //     }
            // }

            // if (_callback != null)
            // {
            //     _callback(_result);
            // }
        }

        public Task<Result> LoadAsync<T>(ITableContainer<T> _table, object _parameter) where T : IRecordContainer, new()
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class MaxABTestStorageParameter{
        public string SdkKey;
        public string ABTestKey;
        public string DefaultValue;
        public Func<string, string> ValueSetter;

        public MaxABTestStorageParameter(string _sdkKey, string _abTestKey, string _defaultValue, Func<string, string> _valueSetter){
            SdkKey = _sdkKey;
            ABTestKey = _abTestKey;
            DefaultValue = _defaultValue;
            ValueSetter = _valueSetter;
        }
    }
}
#endif