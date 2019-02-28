using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BicUtil.Ads
{
    public class AdsManager : BicUtil.SingletonBase.SingletonBase<AdsManager>, IAdsManager
    {

        #region InstantData
        Dictionary<object, AdsInfo> adsData = new Dictionary<object, AdsInfo>();
        Dictionary<object, IAdsPlatform> defaultAdsData = new Dictionary<object, IAdsPlatform>();
        List<IAdsPlatform> adsPlatforms = new List<IAdsPlatform>();
        List<object> passAdsList = new List<object>();
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

        public void SetAdsSetting(object _type, int _playTimeInterval){
            adsData[_type] = new AdsInfo(_type, _playTimeInterval);
        }

        public void SetDefaultAds(AdsType _type, IAdsPlatform _platform)
        {
            defaultAdsData[_type] = _platform;
        }

        int selectedInterstitialPlatform = -1;
        public bool IsReadyInterstitial(object _adsType)
        {
            if(passAdsList.Contains(_adsType) == true){
                Debug.Log("AdsManager IsReadyInterstitial it is passAds true");
                return true;
            }

            if(isPossiblePlayAds(_adsType) == false){
                Debug.Log("AdsManager IsReadyInterstitial isPossiblePlayAds false");
                return false;
            }

            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyInterstitial(_adsType) == true){
                    selectedInterstitialPlatform = i;
                    Debug.Log("AdsManager selectedInterstitialPlatform " + i.ToString());

                    return true;
                }
            }

            Debug.Log("AdsManager selectedInterstitialPlatform false");
            return false;
        }

        public bool IsReadyRewardBased(object _adsType)
        {
            if(passAdsList.Contains(_adsType) == true){
                return true;
            }

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
            if(passAdsList.Contains(_adsType) == true){

                Debug.Log("AdsManager ShowInterstitial passAdsList");
                _callback(AdsResult.Finished);
                return;
            }

            Action<AdsResult> _func = (AdsResult _adsResult)=>{
                if(_adsResult != AdsResult.Failed){ 
                    UpdateLastPlayedAdsTime(_adsType);
                } 

                selectedInterstitialPlatform = -1;
                _callback(_adsResult);
            };

            if(selectedInterstitialPlatform >= 0){

                Debug.Log("AdsManager ShowInterstitial selectedInterstitialPlatform" + selectedInterstitialPlatform.ToString());
                adsPlatforms[selectedInterstitialPlatform].ShowInterstitial(_adsType, _func);
                return;
            }

            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyInterstitial(_adsType) == true){
                    adsPlatforms[i].ShowInterstitial(_adsType, _func);
                    Debug.Log("AdsManager ShowInterstitial by order" + i.ToString());

                    return;
                }
            }

            if(defaultAdsData.ContainsKey(_adsType) == true){

                Debug.Log("AdsManager ShowInterstitial by default");
                defaultAdsData[_adsType].ShowInterstitial(_adsType, _func);
            }else{
                 Debug.Log("AdsManager ShowInterstitial fail");
                _callback(AdsResult.Failed);
            }
        }

        public void ShowRewardBased(object _adsType, Action<AdsResult> _callback)
        {
            if(passAdsList.Contains(_adsType) == true){
                _callback(AdsResult.Finished);
                return;
            }

            if(isPossiblePlayAds(_adsType) == false){
                _callback(AdsResult.Failed);
                return;
            }

            Action<AdsResult> _func = (AdsResult _adsResult)=>{
                if(_adsResult != AdsResult.Failed){ 
                    UpdateLastPlayedAdsTime(_adsType);
                } 

                _callback(_adsResult);
            };
            
            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyRewardBased(_adsType) == true){
                    adsPlatforms[i].ShowRewardBased(_adsType, _func);
                    return;
                }
            }


            if(defaultAdsData.ContainsKey(_adsType) == true){
                defaultAdsData[_adsType].ShowInterstitial(_adsType, _func);
            }else{
                _callback(AdsResult.Failed);
            }
        }

        public void AddAdsPlatform(IAdsPlatform _platform){
            adsPlatforms.Add(_platform);
        }


        private Dictionary<object, IAdsBanner> bannerList = new Dictionary<object, IAdsBanner>();
        public IAdsBanner CreateBanner(object _adsType, Action<IAdsBanner> _onLoadAction){
            if(passAdsList.Contains(_adsType) == true){

                Debug.Log("AdsManager CreateBanner passads");
                return new DummyBanner();
            }

            for(int i = 0; i < adsPlatforms.Count; i++)
            {
                if(adsPlatforms[i].IsReadyBanner(_adsType) == true){
                    var _banner = createBanner(_adsType, _onLoadAction, adsPlatforms[i]);
                    if(_banner != null){

                        Debug.Log("AdsManager CreateBanner by order" + i.ToString());
                        return _banner;
                    }else{
                        Debug.Log("AdsManager CreateBanner by order but is null " + i.ToString());
                    }
                }
            }

            if (defaultAdsData.ContainsKey(_adsType) == true){
                Debug.Log("AdsManager CreateBanner by default");
                var _banner = createBanner(_adsType, _onLoadAction, defaultAdsData[_adsType], true);
                if(_banner != null){
                    return _banner;
                }else{
                    Debug.Log("AdsManager CreateBanner by default but is null");
                }
            }

            return new DummyBanner();
        }

        private IAdsBanner createBanner(object _adsType, Action<IAdsBanner> _onLoadAction, IAdsPlatform _platform, bool isForced = false)
        {
            var _banner = _platform.CreateBanner(_adsType, _onLoadAction);
            if (_banner != null)
            {
                if (bannerList.ContainsKey(_adsType) == true)
                {
                    bannerList[_adsType].Destroy();
                }

                bannerList[_adsType] = _banner;
                return _banner;
            }
            else
            {
                return null;
            }
        }

        public IAdsBanner GetBanner(object _adsType){
            if(bannerList.ContainsKey(_adsType)){
                return bannerList[_adsType];
            }

            return new DummyBanner();
        }

        public void RemoveBanner(IAdsBanner _banner){
            if(bannerList.ContainsValue(_banner)){
                var _item = bannerList.First(_row => _row.Value == _banner);
                bannerList.Remove(_item.Key);
            }
        }

        public void SetPass(object _adsType){
            if(passAdsList.Contains(_adsType) == false){
                passAdsList.Add(_adsType);
            }

            GetBanner(_adsType).Destroy();
        }
    }

    public class DummyBanner : IAdsBanner
    {
        public bool IsReady(){
            return true;
        }

        public void Destroy()
        {
            AdsManager.Instance.RemoveBanner(this);
        }

        public void Hide()
        {
            
        }

        public void SetPosition(Transform _parent, Vector2 _position)
        {
            
        }

        public void Show()
        {
            
        }
    }
}