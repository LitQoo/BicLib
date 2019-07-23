
using System;

namespace BicUtil.Ads{

    public class DummyManager : IAdsPlatform
    {
        public bool IsReadyInterstitialReturn = false;
        public bool IsReadyRewardBasedReturn = false;
        public AdsResult ShowInterstitialReturn = AdsResult.Finished;
        public AdsResult ShowRewardBasedReturn = AdsResult.Finished;

        public DummyManager()
        {
        }


        public bool IsReadyBanner(object _adsPlacement){
            return false;
        }

        public IAdsBanner CreateBanner(object _adsPlacement, Action<IAdsBanner> _onLoadBannerAction)
        {
            return null;
        }

        public bool IsReadyInterstitial(object _adsPlacement)
        {
            return IsReadyInterstitialReturn;
        }

        public bool IsReadyRewardBased(object _adsPlacement)
        {
            return IsReadyRewardBasedReturn;
        }

        public void LoadInterstitial(object _adsPlacement)
        {
            
        }

        public void LoadRewardBased(object _adsPlacement)
        {
            
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
            _callback(ShowInterstitialReturn);
        }

        public void ShowRewardBased(object _adsPlacement, Action<AdsResult> _callback)
        {
            _callback(ShowRewardBasedReturn);
        }
    }
}