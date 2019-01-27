
using System;

namespace BicUtil.AdsManager{

    public class DummyManager : IAdsManager
    {
        static private IAdsManager instance = null;
        static public IAdsManager Instance{
            get{
                if(instance == null){
                    instance = new DummyManager();
                }

                return instance;
            }
        } 

        public bool IsReadyInterstitialReturn = false;
        public bool IsReadyRewardBasedReturn = false;
        public AdsResult ShowInterstitialReturn = AdsResult.Finished;
        public AdsResult ShowRewardBasedReturn = AdsResult.Finished;

        public void Initialize()
        {
            
        }

        public bool IsReadyInterstitial(object _adsType)
        {
            return IsReadyInterstitialReturn;
        }

        public bool IsReadyRewardBased(object _adsType)
        {
            return IsReadyRewardBasedReturn;
        }

        public void LoadInterstitial(object _adsType)
        {
            
        }

        public void LoadRewardBased(object _adsType)
        {
            
        }

        public void SetAdsSettingAndroidOnly(string _adsId, object _type, int _playTimeInterval)
        {
            
        }

        public void SetAdsSettingIOSOnly(string _adsId, object _type, int _playTimeInterval)
        {
            
        }

        public void SetPlatformAndroid(string _androidId)
        {
            
        }

        public void SetPlatformIos(string _iosId)
        {
            
        }

        public void ShowInterstitial(object _adsType, Action<AdsResult> _callback)
        {
            _callback(ShowInterstitialReturn);
        }

        public void ShowRewardBased(object _adsType, Action<AdsResult> _callback)
        {
            _callback(ShowRewardBasedReturn);
        }
    }
}