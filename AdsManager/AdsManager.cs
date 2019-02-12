using System;
using System.Collections.Generic;

namespace BicUtil.AdsManager
{
    public class AdsManager : BicUtil.SingletonBase.SingletonBase<AdsManager>, IAdsManager
    {

        #region InstantData
        Dictionary<object, AdsInfo> adsData = new Dictionary<object, AdsInfo>();
        List<IAdsPlatform> adsPlatforms = new List<IAdsPlatform>();
        #endregion
        
        public override void Initialize()
        {
        }

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

        public void SetAdsSetting(string _adsId, object _type, int _playTimeInterval){
            adsData[_type] = new AdsInfo(_adsId, _type, _playTimeInterval);
        }

        public bool IsReadyInterstitial(object _adsType)
        {
            if(isPossiblePlayAds(_adsType) == false){
                return false;
            }

            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyInterstitial(_adsType) == true){
                    return true;
                }
            }

            return false;
        }

        public bool IsReadyRewardBased(object _adsType)
        {
            if(isPossiblePlayAds(_adsType) == false){
                return false;
            }

            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyRewardBased(_adsType) == true ){
                    return true;
                }
            }

            return false;
        }

        public void LoadInterstitial(object _adsType)
        {
            for(int i = 0; i < adsPlatforms.Count; i++){
                adsPlatforms[i].LoadInterstitial(_adsType);
            }
        }

        public void LoadRewardBased(object _adsType)
        {
            for(int i = 0; i < adsPlatforms.Count; i++){
                adsPlatforms[i].LoadRewardBased(_adsType);
            }
        }

        public void ShowInterstitial(object _adsType, Action<AdsResult> _callback)
        {
            if(isPossiblePlayAds(_adsType) == false){
                _callback(AdsResult.Failed);
            }

            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyInterstitial(_adsType) == true){
                    adsPlatforms[i].ShowInterstitial(_adsType, _adsResult=>{
                        if(_adsResult != AdsResult.Failed){ 
                            UpdateLastPlayedAdsTime(_adsType);
                        } 

                        _callback(_adsResult);
                    });

                    return;
                }
            }

            _callback(AdsResult.Failed);
        }

        public void ShowRewardBased(object _adsType, Action<AdsResult> _callback)
        {
            if(isPossiblePlayAds(_adsType) == false){
                _callback(AdsResult.Failed);
            }
            
            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyRewardBased(_adsType) == true){
                    adsPlatforms[i].ShowRewardBased(_adsType, _adsResult=>{
                        if(_adsResult != AdsResult.Failed){ 
                            UpdateLastPlayedAdsTime(_adsType);
                        } 

                        _callback(_adsResult);
                    });

                    return;
                }
            }

            _callback(AdsResult.Failed);
        }

        public void AddAdsPlatform(IAdsPlatform _platform){
            adsPlatforms.Add(_platform);
        }

        public void ShowBanner(object _adsType){

        }

        public void HideBanner(object _adsType){

        }
    }
}