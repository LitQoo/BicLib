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
        private Dictionary<object, AdsInfo> adsData = new Dictionary<object, AdsInfo>();
        private Dictionary<object, IAdsPlatform> defaultAdsData = new Dictionary<object, IAdsPlatform>();
        private List<IAdsPlatform> adsPlatforms = new List<IAdsPlatform>();
        public TableContainer<AdsStat> AnalyticsTable = new TableContainer<AdsStat>("AdsAnalytics");
        private bool isInit = false;

        public bool IsShowingInterstital {get; private set;}
        public bool IsShowingRewardBased {get; private set;}
        public bool IsShowingAds{get{return IsShowingInterstital || IsShowingRewardBased;}}
        #endregion

        #region Event
        private Action<object, AdsType, AdsResult> OnAfterPlayedAdsCallback;
        private Action<object, AdsType> OnBeforePlayAdsCallback;
        
        public void SubscribeAfterPlayedAds(Action<object, AdsType, AdsResult> _callback){
            OnAfterPlayedAdsCallback += _callback;
        }

        public void SubscribeBeforePlayAds(Action<object, AdsType> _callback){
            OnBeforePlayAdsCallback += _callback;
        }

        private Dictionary<object, Func<bool>> isReadyInterstitialFunc = new Dictionary<object, Func<bool>>();
        public void CustomReadyInterstitial(object _id, Func<bool> _callback){
            if(isReadyInterstitialFunc.ContainsKey(_id) == false){
                isReadyInterstitialFunc.Add(_id, _callback);
            }else{
                isReadyInterstitialFunc[_id] = _callback;
            }
        }
        #endregion

        public override void Initialize()
        {
            if(isInit == true){
                return;
            }

            isInit = true;
            AnalyticsTable.SetStorage(FileStorage.GetInstance());
            AnalyticsTable.Load(null, new FileStorageParameter("analytics"));
        }

        #region Time
        private long getTimestamp(){
            return System.DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
        }

        private bool isPossiblePlayAds(object _adsPlacement){
            if(getTimestamp() - adsData[_adsPlacement].LastPlayedAdsTime > adsData[_adsPlacement].TimeInterval.AsInt){
                return true;
            }else{
                return false;
            }
        }

        public void UpdateLastPlayedAdsTime(object _adsPlacement){
            adsData[_adsPlacement].LastPlayedAdsTime = getTimestamp();
        }

        public void UpdateLastPlayedAdsTimeAll(long _offsetSec = 0){
            foreach (var _item in adsData)
            {   
                _item.Value.LastPlayedAdsTime = getTimestamp() + _offsetSec;
            }
        }
        #endregion

        public void SetAdsSetting(object _adsPlacement, int _playTimeInterval){
            adsData[_adsPlacement] = new AdsInfo(_adsPlacement, new IntVariable(_playTimeInterval));
        }

        public void SetAdsSetting(object _adsPlacement, IVariable _playTimeInterval){
            adsData[_adsPlacement] = new AdsInfo(_adsPlacement, _playTimeInterval);
        }

        public void SetDefaultAds(object _adsPlacement, IAdsPlatform _platform)
        {
            defaultAdsData[_adsPlacement] = _platform;
        }

        int selectedInterstitialPlatform = -1;
        public bool IsReadyInterstitial(object _adsPlacement)
        {
            if(isReadyInterstitialFunc.ContainsKey(_adsPlacement) == true){
                if(isReadyInterstitialFunc[_adsPlacement]() == false){
                    return false;
                }
            }

            if(isPossiblePlayAds(_adsPlacement) == false){
                return false;
            }

            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyInterstitial(_adsPlacement) == true){
                    selectedInterstitialPlatform = i;
                    
                    return true;
                }
            }

            return false;
        }

        int selectedRewardBasedPlatform = -1;
        public bool IsReadyRewardBased(object _adsPlacement)
        {
            if(isPossiblePlayAds(_adsPlacement) == false){
                return false;
            }

            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyRewardBased(_adsPlacement) == true ){
                    selectedRewardBasedPlatform = i;
                    return true;
                }
            }

            return false;
        }

        public void LoadInterstitial(object _adsPlacement)
        {
            for(int i = 0; i < adsPlatforms.Count; i++){
                adsPlatforms[i].LoadInterstitial(_adsPlacement);
            }
        }

        public void LoadRewardBased(object _adsPlacement)
        {
            for(int i = 0; i < adsPlatforms.Count; i++){
                adsPlatforms[i].LoadRewardBased(_adsPlacement);
            }
        }

        public void ShowInterstitial(object _adsPlacement, Action<AdsResult> _callback)
        {
            IsShowingInterstital = true;

            Action<AdsResult> _func = (AdsResult _adsResult)=>{
                IsShowingInterstital = false;

                if(OnAfterPlayedAdsCallback != null){
                    OnAfterPlayedAdsCallback(_adsPlacement, AdsType.Interstital, _adsResult);
                }

                selectedInterstitialPlatform = -1;
                _callback(_adsResult);

                increaseCount(AdsStat.INTERSTITIAL, _adsResult);
            };

            if(selectedInterstitialPlatform >= 0){
                if(OnBeforePlayAdsCallback != null){
                    OnBeforePlayAdsCallback(_adsPlacement, AdsType.Interstital);
                }
                
                UpdateLastPlayedAdsTime(_adsPlacement);

                adsPlatforms[selectedInterstitialPlatform].ShowInterstitial(_adsPlacement, _func);
                return;
            }

            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyInterstitial(_adsPlacement) == true){
                    if(OnBeforePlayAdsCallback != null){
                        OnBeforePlayAdsCallback(_adsPlacement, AdsType.Interstital);
                    }

                    UpdateLastPlayedAdsTime(_adsPlacement);
                    
                    adsPlatforms[i].ShowInterstitial(_adsPlacement, _func);
                    return;
                }
            }

            if(defaultAdsData.ContainsKey(_adsPlacement) == true){
                if(OnBeforePlayAdsCallback != null){
                    OnBeforePlayAdsCallback(_adsPlacement, AdsType.Interstital);
                }

                UpdateLastPlayedAdsTime(_adsPlacement);
                
                defaultAdsData[_adsPlacement].ShowInterstitial(_adsPlacement, _func);
            }else{
                IsShowingInterstital = false;
                _callback(AdsResult.Failed);
                increaseCount(AdsStat.INTERSTITIAL, AdsResult.Failed);
            }
        }
        

        public void ShowRewardBased(object _adsPlacement, Action<AdsResult> _callback)
        {
            IsShowingRewardBased = true;

            if(isPossiblePlayAds(_adsPlacement) == false){
                IsShowingRewardBased = false;
                _callback(AdsResult.Failed);
                return;
            }

            Action<AdsResult> _func = (AdsResult _adsResult)=>{
                IsShowingRewardBased = false;
                
                if(_adsResult != AdsResult.Failed){ 
                    UpdateLastPlayedAdsTime(_adsPlacement);
                } 

                if(OnAfterPlayedAdsCallback != null){
                    OnAfterPlayedAdsCallback(_adsPlacement, AdsType.RewardBase, _adsResult);
                }

                selectedRewardBasedPlatform = -1;
                _callback(_adsResult);

                increaseCount(AdsStat.REWARD, _adsResult);
            };


            if(selectedRewardBasedPlatform >= 0){
                if(OnBeforePlayAdsCallback != null){
                    OnBeforePlayAdsCallback(_adsPlacement, AdsType.RewardBase);
                }

                adsPlatforms[selectedRewardBasedPlatform].ShowRewardBased(_adsPlacement, _func);
                return;
            }
            
            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyRewardBased(_adsPlacement) == true){
                    if(OnBeforePlayAdsCallback != null){
                        OnBeforePlayAdsCallback(_adsPlacement, AdsType.RewardBase);
                    }

                    adsPlatforms[i].ShowRewardBased(_adsPlacement, _func);
                    return;
                }
            }

            if(defaultAdsData.ContainsKey(_adsPlacement) == true){
                if(OnBeforePlayAdsCallback != null){
                    OnBeforePlayAdsCallback(_adsPlacement, AdsType.RewardBase);
                }

                defaultAdsData[_adsPlacement].ShowRewardBased(_adsPlacement, _func);
            }else{
                IsShowingRewardBased = false;

                _callback(AdsResult.Failed);
                increaseCount(AdsStat.REWARD, AdsResult.Failed);
            }
        }

        public void AddAdsPlatform(IAdsPlatform _platform){
            adsPlatforms.Add(_platform);
        }


        private Dictionary<object, IAdsBanner> bannerList = new Dictionary<object, IAdsBanner>();
        public IAdsBanner CreateBanner(object _adsPlacement, Color _backColor, Action<IAdsBanner> _onLoadAction){

            for(int i = 0; i < adsPlatforms.Count; i++)
            {
                if(adsPlatforms[i].IsReadyBanner(_adsPlacement) == true){
                    var _banner = createBanner(_adsPlacement, _onLoadAction, adsPlatforms[i], _backColor);
                    if(_banner != null){

                        increaseCount(AdsStat.BANNER, AdsResult.Finished);
                        return _banner;
                    }
                }
            }

            if (defaultAdsData.ContainsKey(_adsPlacement) == true){
                var _banner = createBanner(_adsPlacement, _onLoadAction, defaultAdsData[_adsPlacement], _backColor, true);
                if(_banner != null){
                    increaseCount(AdsStat.BANNER, AdsResult.Finished);
                    return _banner;
                }
            }


            increaseCount(AdsStat.BANNER, AdsResult.Failed);
            return new DummyBanner();
        }

        private IAdsBanner createBanner(object _adsPlacement, Action<IAdsBanner> _onLoadAction, IAdsPlatform _platform, Color _backColor, bool isForced = false)
        {
            var _banner = _platform.CreateBanner(_adsPlacement, _backColor, _onLoadAction);
            if (_banner != null)
            {
                if (bannerList.ContainsKey(_adsPlacement) == true)
                {
                    bannerList[_adsPlacement].Destroy();
                }

                bannerList[_adsPlacement] = _banner;
                return _banner;
            }
            else
            {
                return null;
            }
        }

        public IAdsBanner GetBanner(object _adsPlacement){
            if(bannerList.ContainsKey(_adsPlacement)){
                return bannerList[_adsPlacement];
            }

            return new DummyBanner();
        }

        public void RemoveBanner(IAdsBanner _banner){
            if(bannerList.ContainsValue(_banner)){
                var _item = bannerList.First(_row => _row.Value == _banner);
                bannerList.Remove(_item.Key);
            }
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

            //this.AnalyticsTable.Save();
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