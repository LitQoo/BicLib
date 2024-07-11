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
        private IAdsPlatform lastAdsPlatforms = null;
        //public TableContainer<AdsStat> AnalyticsTable = new TableContainer<AdsStat>("AdsAnalytics");
        private bool isInit = false;

        public bool IsShowingInterstital {get; private set;}
        public bool IsShowingRewardBased {get; private set;}
        public bool IsShowingAds{get{return IsShowingInterstital || IsShowingRewardBased;}}
        public float admitRewardTime = 14f;
        #endregion

        #region Event
        private Action<object, AdsType, AdsResult> OnAfterPlayedAdsCallback;
        private Action<object, AdsType> OnBeforePlayAdsCallback;
        private Action<object, AdsType, string> OnNotReadyAdsCallback;

        #if UNITY_EDITOR
        public void CallOnBeforePlayAdsCallback(object _adsPlacement, AdsType _adsType){
            if(OnBeforePlayAdsCallback != null){
                OnBeforePlayAdsCallback(_adsPlacement, _adsType);
            }
        }

        public void CallOnAfterPlayAdsCallback(object _adsPlacement, AdsType _adsType, AdsResult _result){
            if(OnAfterPlayedAdsCallback != null){
                OnAfterPlayedAdsCallback(_adsPlacement, _adsType, _result);
            }
        }
        #endif
        
        public void SubscribeAfterPlayedAds(Action<object, AdsType, AdsResult> _callback){
            OnAfterPlayedAdsCallback += _callback;
        }

        public void SubscribeBeforePlayAds(Action<object, AdsType> _callback){
            OnBeforePlayAdsCallback += _callback;
        }

        public void SubscribeNotReadyAds(Action<object, AdsType, string> _callback){
            OnNotReadyAdsCallback += _callback;
        }

        private Dictionary<object, Func<bool>> isReadyInterstitialFunc = new Dictionary<object, Func<bool>>();
        public void CustomReadyInterstitial(object _id, Func<bool> _callback){
            if(isReadyInterstitialFunc.ContainsKey(_id) == false){
                isReadyInterstitialFunc.Add(_id, _callback);
            }else{
                isReadyInterstitialFunc[_id] = _callback;
            }
        }

        public void SetAdmitRewardTime(float _time){
            this.admitRewardTime = _time;
        }

        internal void AddAdsPlatform()
        {
            throw new NotImplementedException();
        }
        #endregion

        public override void Initialize()
        {
            // AnalyticsTable.SetStorage(FileStorage.GetInstance());
            // AnalyticsTable.Load(null, new FileStorageParameter("analytics"));
            this.Initialize(null);
        }


        public void Initialize(Action _callback)
        {
            if(isInit == true){
                Debug.Log("[AdsManager] already init");
                return;
            }
            
            int _count = 0;
            int _targetCount = adsPlatforms.Count;

            if(_targetCount < 0){
                Debug.Log("[AdsManager] platforms count is zero");
                return;
            }

            Debug.Log("[AdsManager] Init platforms " + _targetCount);

            foreach(var _ads in adsPlatforms){
                _ads.Initialize(()=>{
                    _count++;
                    if(_count == _targetCount){
                        if(_callback != null){
                            _callback();
                            _callback = null;
                        }
                    }
                });
            }

            isInit = true;
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
                    if(OnNotReadyAdsCallback != null){
                        OnNotReadyAdsCallback(_adsPlacement, AdsType.Interstital, "Custom");
                    }

                    DebugForEditor.Log("[AdsManager] IsReadyInterstitial false by Custom");
                    return false;
                }
            }

            if(isPossiblePlayAds(_adsPlacement) == false){
                if(OnNotReadyAdsCallback != null){
                    OnNotReadyAdsCallback(_adsPlacement, AdsType.Interstital, "Time");
                }

                DebugForEditor.Log("[AdsManager] IsReadyInterstitial false by TimeInterval");
                return false;
            }

            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyInterstitial(_adsPlacement) == true){
                    selectedInterstitialPlatform = i;
                    

                    DebugForEditor.Log("[AdsManager] check IsReadyInterstitial true by " + adsPlatforms[i].GetType().ToString());
                    return true;
                }else{
                    DebugForEditor.Log("[AdsManager] check IsReadyInterstitial false by " + adsPlatforms[i].GetType().ToString());
                }
            }

            if(lastAdsPlatforms != null){
                selectedInterstitialPlatform = int.MaxValue;
                DebugForEditor.Log("[AdsManager] IsReadyInterstitial true by lastAdsPlatforms");
                return true;
            }

            if(OnNotReadyAdsCallback != null){
                OnNotReadyAdsCallback(_adsPlacement, AdsType.Interstital, "NoFill");
            }


            DebugForEditor.Log("[AdsManager] IsReadyInterstitial false");

            return false;
        }

        int selectedRewardBasedPlatform = -1;
        public bool IsReadyRewardBased(object _adsPlacement)
        {
            if(isPossiblePlayAds(_adsPlacement) == false){
                if(OnNotReadyAdsCallback != null){
                    OnNotReadyAdsCallback(_adsPlacement, AdsType.RewardBase, "Time");
                }
                return false;
            }

            for(int i = 0; i < adsPlatforms.Count; i++){
                if(adsPlatforms[i].IsReadyRewardBased(_adsPlacement) == true ){
                    selectedRewardBasedPlatform = i;
                    return true;
                }
            }

            if(lastAdsPlatforms != null){
                selectedRewardBasedPlatform = int.MaxValue;
                return true;
            }

            if(OnNotReadyAdsCallback != null){
                OnNotReadyAdsCallback(_adsPlacement, AdsType.RewardBase, "NoFill");
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

        public void ShowInterstitialIfReady(object _adsPlacement, Action<AdsResult> _nextAction){
            if(IsReadyInterstitial(_adsPlacement) == true){
                ShowInterstitial(_adsPlacement, _nextAction);
            }else{
                _nextAction(AdsResult.Failed);
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

                
                if(_adsResult != AdsResult.Failed){ 
                    UpdateLastPlayedAdsTime(_adsPlacement);
                } 

                selectedInterstitialPlatform = -1;
                _callback(_adsResult);

                increaseCount(AdsStat.INTERSTITIAL, _adsResult);
            };

            if(selectedInterstitialPlatform >= 0 && selectedInterstitialPlatform < adsPlatforms.Count){
                if(OnBeforePlayAdsCallback != null){
                    OnBeforePlayAdsCallback(_adsPlacement, AdsType.Interstital);
                }

                adsPlatforms[selectedInterstitialPlatform].ShowInterstitial(_adsPlacement, _func);
                return;
            }

            if(selectedInterstitialPlatform < 0){
                for(int i = 0; i < adsPlatforms.Count; i++){
                    if(adsPlatforms[i].IsReadyInterstitial(_adsPlacement) == true){
                        if(OnBeforePlayAdsCallback != null){
                            OnBeforePlayAdsCallback(_adsPlacement, AdsType.Interstital);
                        }
                        
                        adsPlatforms[i].ShowInterstitial(_adsPlacement, _func);
                        return;
                    }
                }
            }

            if(defaultAdsData.ContainsKey(_adsPlacement) == true && lastAdsPlatforms != null){
                if(OnBeforePlayAdsCallback != null){
                    OnBeforePlayAdsCallback(_adsPlacement, AdsType.Interstital);
                }
                
                defaultAdsData[_adsPlacement].ShowInterstitial(_adsPlacement, _func);
            }else if(lastAdsPlatforms != null){
                if(OnBeforePlayAdsCallback != null){
                    OnBeforePlayAdsCallback(_adsPlacement, AdsType.Interstital);
                }

                lastAdsPlatforms.ShowInterstitial(_adsPlacement, _func);
            }else{
                IsShowingInterstital = false;
                _callback(AdsResult.Failed);
                increaseCount(AdsStat.INTERSTITIAL, AdsResult.Failed);
            }
        }
        
        public void ShowRewardBasedIfReady(object _adsPlacement, Action<AdsResult> _nextAction){
            if(IsReadyRewardBased(_adsPlacement) == true){
                ShowRewardBased(_adsPlacement, _nextAction);
            }else{
                _nextAction(AdsResult.Failed);
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

            var _startTime = getTimestamp();
            Action<AdsResult> _func = (AdsResult _adsResult)=>{
                IsShowingRewardBased = false;
                var _result = _adsResult;
 
                if(getTimestamp() - _startTime >= admitRewardTime){
                    _result = AdsResult.Finished;
                }
                
                if(_result != AdsResult.Failed){ 
                    UpdateLastPlayedAdsTime(_adsPlacement);
                } 

                if(OnAfterPlayedAdsCallback != null){
                    OnAfterPlayedAdsCallback(_adsPlacement, AdsType.RewardBase, _result);
                }

                selectedRewardBasedPlatform = -1;
                _callback(_result);

                increaseCount(AdsStat.REWARD, _result);
            };


            if(selectedRewardBasedPlatform >= 0 && selectedRewardBasedPlatform < adsPlatforms.Count){
                if(OnBeforePlayAdsCallback != null){
                    OnBeforePlayAdsCallback(_adsPlacement, AdsType.RewardBase);
                }

                adsPlatforms[selectedRewardBasedPlatform].ShowRewardBased(_adsPlacement, _func);
                return;
            }
            
            if(selectedRewardBasedPlatform < 0){
                for(int i = 0; i < adsPlatforms.Count; i++){
                    if(adsPlatforms[i].IsReadyRewardBased(_adsPlacement) == true){
                        if(OnBeforePlayAdsCallback != null){
                            OnBeforePlayAdsCallback(_adsPlacement, AdsType.RewardBase);
                        }

                        adsPlatforms[i].ShowRewardBased(_adsPlacement, _func);
                        return;
                    }
                }
            }

            if(defaultAdsData.ContainsKey(_adsPlacement) == true && lastAdsPlatforms == null){
                if(OnBeforePlayAdsCallback != null){
                    OnBeforePlayAdsCallback(_adsPlacement, AdsType.RewardBase);
                }

                defaultAdsData[_adsPlacement].ShowRewardBased(_adsPlacement, _func);
            }else if(lastAdsPlatforms != null){
                if(OnBeforePlayAdsCallback != null){
                    OnBeforePlayAdsCallback(_adsPlacement, AdsType.RewardBase);
                }

                lastAdsPlatforms.ShowRewardBased(_adsPlacement, _func);
            }else{
                IsShowingRewardBased = false;

                _callback(AdsResult.Failed);
                increaseCount(AdsStat.REWARD, AdsResult.Failed);
            }
        }

        public void AddAdsPlatform(IAdsPlatform _platform){
            adsPlatforms.Add(_platform);
        }

        public void AddAdsPlatformFirst(IAdsPlatform _platform){
            adsPlatforms.Insert(0, _platform);
        }

        public void SetLastAdsPlatform(IAdsPlatform _platform){
            lastAdsPlatforms = _platform;
        }

        public void RemoveAdsPlatform(IAdsPlatform _platform){
            adsPlatforms.Remove(_platform);
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

            if(lastAdsPlatforms != null){
                var _banner = createBanner(lastAdsPlatforms, _onLoadAction, lastAdsPlatforms, _backColor, true);
                
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

        // private AdsStat getStat(string _name){
        //     var _stat = AnalyticsTable.FirstOrDefault(_row=>_row.Name.AsString == _name);
        //     if(_stat == null){
        //         _stat = new AdsStat(_name);
        //         AnalyticsTable.Add(_stat);
        //     }

        //     return _stat;
        // }

        private void increaseCount(string _name, AdsResult _result){
            // var _stat = getStat(_name);

            // switch(_result){
            //     case AdsResult.Cancel:
            //     _stat.CancelCount.AsInt++;
            //     break;
            //     case AdsResult.Failed:
            //     _stat.FailCount.AsInt++;
            //     break;
            //     case AdsResult.Finished:
            //     _stat.FinishCount.AsInt++;
            //     break;
            //     case AdsResult.Skipped:
            //     _stat.SkipCount.AsInt++;
            //     break;
            //     default:
            //     _stat.OtherCount.AsInt++;
            //     break;
            // }

            //this.AnalyticsTable.Save();
        }

        public List<string> GetTerms(){
            var _result = new List<string>();
            for(int i = 0; i < adsPlatforms.Count; i++){
                _result.Add(adsPlatforms[i].TermsURL);
            }

            return _result;
        }

        public void SetUserConsent(bool _isEnabled){
            for(int i = 0; i < adsPlatforms.Count; i++){
                adsPlatforms[i].SetUserConsent(_isEnabled);
            }
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