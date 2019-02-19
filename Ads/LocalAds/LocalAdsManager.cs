using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.SingletonBase;
using System;
using System.Linq;

namespace BicUtil.Ads
{
    public class LocalAdsManager : SingletonBase<LocalAdsManager>, IAdsPlatform
    {
        #region InstantData
        private List<LocalAdsInfo> adsData = new List<LocalAdsInfo>();
        #endregion

        #region LifeCycle
        public override void Initialize()
        {
            
        }
        #endregion

        #region set 
        public void SetAdsSetting(object _adsType, string _prefabPath, int _wieght, int _time, Func<bool> _isReady){
            var _ads = new LocalAdsInfo(_adsType, _prefabPath, _wieght, _time, _isReady);
            adsData.Add(_ads);
        }
        #endregion

        #region IAdsPlatform
        public IAdsBanner CreateBanner(object _adsType, Action<IAdsBanner> _onLoadBannerAction)
        {
            var _ads = adsData.FirstOrDefault(_row=>_row.AdsType.ToString() == _adsType.ToString());
            if(_ads != null){
                var _banner = MonoBehaviour.Instantiate(Resources.Load<LocalBannerController>(_ads.PrefabPath));
                _onLoadBannerAction(_banner);
                return _banner;
            }

            return null;
        }

        public bool IsReadyInterstitial(object _adsType)
        {
            return isReady(_adsType);
        }

        public bool IsReadyRewardBased(object _adsType)
        {
            return isReady(_adsType);
        }

        public void LoadInterstitial(object _adsType)
        {
        }

        public void LoadRewardBased(object _adsType)
        {
        }

        public void ShowInterstitial(object _adsType, Action<AdsResult> _callback)
        {
            showInterstitial(_adsType, _callback, 5);
        }

        public void ShowRewardBased(object _adsType, Action<AdsResult> _callback)
        {
            showInterstitial(_adsType, _callback, 20);
        }
        
        private bool isReady(object _adsType){
            var _count = getAdsList(_adsType).Count();

            if(_count > 0){
                return true;
            }else{
                return false;
            }
        }

        private void showInterstitial(object _adsType, Action<AdsResult> _callback, int _time){
            var _ads = GetAds(_adsType);

            if(_ads != null){
                var _interstitial = MonoBehaviour.Instantiate(Resources.Load<LocalInterstitialController>(_ads.PrefabPath));
                _interstitial.OnClose += ()=>{
                    _callback(AdsResult.Finished);
                };

                var _canvasList = Resources.FindObjectsOfTypeAll(typeof(Canvas));
                var _canvas = (_canvasList[0] as Canvas);
                _interstitial.transform.SetParent(_canvas.transform);
                _interstitial.transform.localScale = new Vector2(1f, 1f);
                _interstitial.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 0f);
                _interstitial.GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);
                _interstitial.Load(_time);
                _interstitial.Show();

            }else{
                _callback(AdsResult.Failed);
            }
        }

        public LocalAdsInfo GetAds(object _adsType){
           var _adsList = getAdsList(_adsType);

           if(_adsList.Count() <= 0){
               return null;
           }

           int _totalWeight = _adsList.Sum(_row => _row.ViewWeight);
           int _selectWeight = UnityEngine.Random.Range(0, _totalWeight);
           int _sumWeight = 0;

           foreach(var _ads in _adsList){
               _sumWeight += _ads.ViewWeight;
               if (_sumWeight >= _selectWeight) {
                   return _ads;
               }
           }

           return _adsList.ElementAt(0);
        }

        private IEnumerable<LocalAdsInfo> getAdsList(object _adsType)
        {
            return adsData.Where(_row => _row.IsReady() == true && (_row.AdsType.ToString() == _adsType.ToString()));
        }
        #endregion
    }

    

    public class LocalAdsInfo{
        public object AdsType;
        public int Time;
        public string PrefabPath;
        public int ViewWeight;
        private Func<bool> isReady;

        public LocalAdsInfo(object _type, string _prefabPath, int _weight, int _time, Func<bool> _isReady){
            this.AdsType = _type;
            this.Time = _time;
            this.PrefabPath = _prefabPath;
            this.ViewWeight = _weight;
            this.isReady = _isReady;
        }

        public bool IsReady(){
            if(this.isReady == null){
                return true;
            }else{
                return this.isReady();
            }
        }
    }
}