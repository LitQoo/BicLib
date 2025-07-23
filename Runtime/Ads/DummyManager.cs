
using System;
using BicDB.Container;
using BicDB.Core;
using BicDB.Variable;
using UnityEngine;

namespace BicUtil.Ads{

    public class DummySetting : RecordContainer{
        public BoolVariable IsTestingSDK = new(true);
        public BoolVariable UseSettingValueOnTestingSDK = new(false);
        public BoolVariable IsReadyInterstitialReturn = new(false);
        public BoolVariable IsReadyRewardBasedReturn = new(false);
        public EnumVariable<AdsResult> ShowInterstitialReturn = new(AdsResult.Finished);
        public EnumVariable<AdsResult> ShowRewardBasedReturn = new(AdsResult.Finished);

        public DummySetting(){
            AddManagedColumn("IsTestingSDK", IsTestingSDK);
            AddManagedColumn("UseSettingValueOnTestingSDK", UseSettingValueOnTestingSDK);
            AddManagedColumn("IsReadyInterstitialReturn", IsReadyInterstitialReturn);
            AddManagedColumn("IsReadyRewardBasedReturn", IsReadyRewardBasedReturn);
            AddManagedColumn("ShowInterstitialReturn", ShowInterstitialReturn);
            AddManagedColumn("ShowRewardBasedReturn", ShowRewardBasedReturn);

            #if UNITY_EDITOR
            TableService.Inspector.Track("DummyAdsSetting", this);
            #endif
        }
    }
    public class DummyManager : IAdsPlatform
    {
        public string TermsURL => "";

        public DummySetting Setting = new DummySetting();

        private IAdsPlatform testingSDK = null;

        public DummyManager(IAdsPlatform _testingSDK)
        {
            testingSDK = _testingSDK;
            if(testingSDK == null){
                Setting.IsTestingSDK.AsBool = false;
            }
        }


        public bool IsReadyBanner(object _adsPlacement){
            return false;
        }

        public IAdsBanner CreateBanner(object _adsPlacement, Color _backColor, Action<IAdsBanner> _onLoadBannerAction)
        {
            return null;
        }

        public bool IsReadyInterstitial(object _adsPlacement)
        {
            if(Setting.IsTestingSDK.AsBool == true && Setting.UseSettingValueOnTestingSDK.AsBool == false){
                return testingSDK.IsReadyInterstitial(_adsPlacement);
            }else{
                return Setting.IsReadyInterstitialReturn.AsBool;
            }
        }

        public bool IsReadyRewardBased(object _adsPlacement)
        {
            if(Setting.IsTestingSDK.AsBool == true && Setting.UseSettingValueOnTestingSDK.AsBool == false){
                return testingSDK.IsReadyRewardBased(_adsPlacement);
            }else{
                return Setting.IsReadyRewardBasedReturn.AsBool;
            }
        }

        public void LoadInterstitial(object _adsPlacement)
        {
            if(Setting.IsTestingSDK.AsBool == true && Setting.UseSettingValueOnTestingSDK.AsBool == false){
                testingSDK.LoadInterstitial(_adsPlacement);
            }
        }

        public void LoadRewardBased(object _adsPlacement)
        {
            if(Setting.IsTestingSDK.AsBool == true && Setting.UseSettingValueOnTestingSDK.AsBool == false){
                testingSDK.LoadRewardBased(_adsPlacement);
            }
        }

        public void SetAdsSettingAndroidOnly(string _adsId, object _adsPlacement)
        {
            
        }

        public void SetAdsSettingIOSOnly(string _adsId, object _adsPlacement)
        {
            
        }

        public void SetPlatformAndroid(string _androidId)
        {
            
        }

        public void SetPlatformIos(string _iosId)
        {
            
        }

        public void ShowInterstitial(object _adsPlacement, Action<AdsResult> _callback)
        {
            if(testingSDK != null && Setting.IsTestingSDK.AsBool == true){
                this.testingSDK.LoadInterstitial(_adsPlacement);
                this.testingSDK.ShowInterstitial(_adsPlacement, _result=>{
                    if(Setting.UseSettingValueOnTestingSDK.AsBool == true){
                        _callback(Setting.ShowInterstitialReturn.AsEnum);
                    }else{
                        _callback(_result);
                    }
                });
            }else{
                _callback(Setting.ShowInterstitialReturn.AsEnum);
            }
        }

        public void ShowRewardBased(object _adsPlacement, Action<AdsResult> _callback)
        {
            if(testingSDK != null && Setting.IsTestingSDK.AsBool == true){
                this.testingSDK.LoadRewardBased(_adsPlacement);
                this.testingSDK.ShowRewardBased(_adsPlacement, _result=>{
                    if(Setting.UseSettingValueOnTestingSDK.AsBool == true){
                        _callback(Setting.ShowRewardBasedReturn.AsEnum);
                    }else{
                        _callback(_result);
                    }
                });
            }else{
                _callback(Setting.ShowRewardBasedReturn.AsEnum);
            }
        }

        public void SetUserConsent(bool _isEnabled){
         
        }

        public void Initialize(Action _callback)
        {
            if(Setting.IsTestingSDK.AsBool == true){
                this.testingSDK.Initialize(_callback);
            }else{
                _callback();
            }
        }
    }
}