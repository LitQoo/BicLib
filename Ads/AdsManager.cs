using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Container;
using BicDB.Storage;
using BicDB.Variable;
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

        public TableContainer<AdsStat> AnalyticsTable = new TableContainer<AdsStat>("AdsAnalytics");
        #endregion
        private bool isInit = false;
        public override void Initialize()
        {
            if(isInit == true){
                return;
            }

            isInit = true;
            AnalyticsTable.SetStorage(FileStorage.GetInstance());
            AnalyticsTable.Load(null, new FileStorageParameter("analytics"));
            Debug.Log("init");
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

        public void SetDefaultAds(object _type, IAdsPlatform _platform)
        {
            defaultAdsData[_type] = _platform;
        }

        int selectedInterstitialPlatform = -1;
        public bool IsReadyInterstitial(object _adsType)
        {
            if(passAdsList.Contains(_adsType) == true){
                return true;
            }

            if(isPossiblePlayAds(_adsType) == false){
                return false;
            }

            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyInterstitial(_adsType) == true){
                    selectedInterstitialPlatform = i;
                    
                    return true;
                }
            }

            return false;
        }

        int selectedRewardBasedPlatform = -1;
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
                    selectedRewardBasedPlatform = i;
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
                _callback(AdsResult.Finished);
                return;
            }

            Action<AdsResult> _func = (AdsResult _adsResult)=>{
                if(_adsResult != AdsResult.Failed){ 
                    UpdateLastPlayedAdsTime(_adsType);
                } 

                selectedInterstitialPlatform = -1;
                _callback(_adsResult);

                increaseCount(AdsStat.INTERSTITIAL, _adsResult);
            };

            if(selectedInterstitialPlatform >= 0){
                adsPlatforms[selectedInterstitialPlatform].ShowInterstitial(_adsType, _func);
                return;
            }

            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyInterstitial(_adsType) == true){
                    adsPlatforms[i].ShowInterstitial(_adsType, _func);
                    
                    return;
                }
            }

            if(defaultAdsData.ContainsKey(_adsType) == true){
                defaultAdsData[_adsType].ShowInterstitial(_adsType, _func);
            }else{
                 _callback(AdsResult.Failed);
                 increaseCount(AdsStat.INTERSTITIAL, AdsResult.Failed);
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

                selectedRewardBasedPlatform = -1;
                _callback(_adsResult);

                increaseCount(AdsStat.REWARD, _adsResult);
            };


            if(selectedRewardBasedPlatform >= 0){
                adsPlatforms[selectedRewardBasedPlatform].ShowRewardBased(_adsType, _func);
                return;
            }
            
            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyRewardBased(_adsType) == true){
                    adsPlatforms[i].ShowRewardBased(_adsType, _func);
                    return;
                }
            }

            if(defaultAdsData.ContainsKey(_adsType) == true){
                defaultAdsData[_adsType].ShowRewardBased(_adsType, _func);
            }else{
                _callback(AdsResult.Failed);
                increaseCount(AdsStat.REWARD, AdsResult.Failed);
            }
        }

        public void AddAdsPlatform(IAdsPlatform _platform){
            adsPlatforms.Add(_platform);
        }


        private Dictionary<object, IAdsBanner> bannerList = new Dictionary<object, IAdsBanner>();
        public IAdsBanner CreateBanner(object _adsType, Action<IAdsBanner> _onLoadAction){
            if(passAdsList.Contains(_adsType) == true){
                increaseCount(AdsStat.BANNER, AdsResult.Skipped);
                return new DummyBanner();
            }

            for(int i = 0; i < adsPlatforms.Count; i++)
            {
                if(adsPlatforms[i].IsReadyBanner(_adsType) == true){
                    var _banner = createBanner(_adsType, _onLoadAction, adsPlatforms[i]);
                    if(_banner != null){

                        increaseCount(AdsStat.BANNER, AdsResult.Finished);
                        return _banner;
                    }
                }
            }

            if (defaultAdsData.ContainsKey(_adsType) == true){
                var _banner = createBanner(_adsType, _onLoadAction, defaultAdsData[_adsType], true);
                if(_banner != null){
                    increaseCount(AdsStat.BANNER, AdsResult.Finished);
                    return _banner;
                }
            }


            increaseCount(AdsStat.BANNER, AdsResult.Failed);
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

        private AdsStat getStat(string _name){
            var _stat = AnalyticsTable.FirstOrDefault(_row=>_row.Name.AsString == _name);
            if(_stat == null){
                _stat = new AdsStat(_name);
                AnalyticsTable.Add(_stat);
            }

            return _stat;
        }

        private void increaseCount(string _name, AdsResult _result){
            var _stat = getStat(_name);

            switch(_result){
                case AdsResult.Cancel:
                _stat.CancelCount.AsInt++;
                break;
                case AdsResult.Failed:
                _stat.FailCount.AsInt++;
                break;
                case AdsResult.Finished:
                _stat.FinishCount.AsInt++;
                break;
                case AdsResult.Skipped:
                _stat.SkipCount.AsInt++;
                break;
                default:
                _stat.OtherCount.AsInt++;
                break;
            }

            this.AnalyticsTable.Save();
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

    public class AdsStat : RecordContainer{
        #region Static
        public const string REWARD = "Reward";
        public const string INTERSTITIAL = "Intersitital";
        public const string BANNER = "Banner";
        #endregion

        #region Field
        public StringVariable Name = new StringVariable();
        public IntVariable FinishCount = new IntVariable();
        public IntVariable FailCount = new IntVariable();
        public IntVariable SkipCount = new IntVariable();
        public IntVariable CancelCount = new IntVariable();
        public IntVariable OtherCount = new IntVariable();
        #endregion

        #region LifeCycle
        public AdsStat(){
            AddManagedColumn("Name", this.Name);
            AddManagedColumn("Finish", this.FinishCount);
            AddManagedColumn("Fail", this.FailCount);
            AddManagedColumn("Skip", this.SkipCount);
            AddManagedColumn("Cancel", this.CancelCount);
            AddManagedColumn("Other", this.OtherCount);
        }

        public AdsStat(string Name) : base(){
            this.Name.AsString = Name;
        }
        #endregion
    }
}