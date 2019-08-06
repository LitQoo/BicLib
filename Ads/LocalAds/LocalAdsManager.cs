using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.SingletonBase;
using System;
using System.Linq;
using BicUtil.Tween;

namespace BicUtil.Ads
{
    public class LocalAdsManager : SingletonBase<LocalAdsManager>, IAdsPlatform
    {
        #region InstantData
        private List<LocalAdsInfo> adsData = new List<LocalAdsInfo>();
        public float BannerReloadTime = 60;
        #endregion

        #region LifeCycle
        public override void Initialize()
        {
        }
        #endregion

        #region set 
        public void SetAdsSetting(string _id, object _adsPlacement, string _prefabPath, int _wieght, Func<bool> _isReady){
            var _ads = new LocalAdsInfo(_id, _adsPlacement, _prefabPath, _wieght, _isReady);
            adsData.Add(_ads);
        }
        #endregion

        #region IAdsPlatform
        public bool IsReadyBanner(object _adsPlacement){
            var _ads = adsData.FirstOrDefault(_row=>_row.AdsPlacement.ToString() == _adsPlacement.ToString() && _row.IsReady() == true);
            selectedAds[_adsPlacement] = _ads;
            return _ads != null;
        }

        public IAdsBanner CreateBanner(object _adsPlacement, Color _backColor, Action<IAdsBanner> _onLoadBannerAction)
        {
            var _ads = selectedAds[_adsPlacement];
            
            if(_ads == null){
                _ads = adsData.FirstOrDefault(_row=>_row.AdsPlacement.ToString() == _adsPlacement.ToString());
            }

            if(_ads != null){
                var _banner = MonoBehaviour.Instantiate(Resources.Load<LocalBannerController>(_ads.PrefabPath));
                _onLoadBannerAction(_banner);

                var _nextAdsPlacement = _adsPlacement;
                var _nextOnLoadBannerAction = _onLoadBannerAction;
                BicTween.Delay(BannerReloadTime).SetTargetObject(_banner.gameObject).SubscribeComplete(()=>{
                    _banner.Destroy();
                    AdsManager.Instance.CreateBanner(_nextAdsPlacement, _backColor, _nextOnLoadBannerAction);
                });
                return _banner;
            }

            return null;
        }

        private Dictionary<object, LocalAdsInfo> selectedAds = new Dictionary<object, LocalAdsInfo>();        
        
        public bool IsReadyInterstitial(object _adsPlacement)
        {
            selectedAds[_adsPlacement] = GetAds(_adsPlacement);
            
            return selectedAds[_adsPlacement] != null;
        }

        public bool IsReadyRewardBased(object _adsPlacement)
        {
            selectedAds[_adsPlacement] = GetAds(_adsPlacement);

            return selectedAds[_adsPlacement] != null;
        }

        public void LoadInterstitial(object _adsPlacement)
        {
        }

        public void LoadRewardBased(object _adsPlacement)
        {
        }

        public void ShowInterstitial(string _id, Action<AdsResult> _callback, int _time){
            var _ads = adsData.FirstOrDefault(_row=>_row.Id == _id);
            if(_ads != null){
                showInterstitialByPrefab(_ads, _callback, _time);
            }else{
                _callback(AdsResult.Failed);
            }
        }

        public void ShowInterstitial(object _adsPlacement, Action<AdsResult> _callback)
        {
            showInterstitial(_adsPlacement, _callback, 5);
        }

        public void ShowRewardBased(object _adsPlacement, Action<AdsResult> _callback)
        {
            showInterstitial(_adsPlacement, _callback, 20);
        }

        private void showInterstitial(object _adsPlacement, Action<AdsResult> _callback, int _time){
            LocalAdsInfo _ads = null;
            
            if(selectedAds.ContainsKey(_adsPlacement) == true){
                _ads = selectedAds[_adsPlacement];
                selectedAds[_adsPlacement] = null;
            }
            
            if(_ads == null){
                _ads = GetAds(_adsPlacement);
            }

            if(_ads != null){
                showInterstitialByPrefab(_ads, _callback, _time);

            }else{
                _callback(AdsResult.Failed);
            }
        }

        private void showInterstitialByPrefab(LocalAdsInfo _ads, Action<AdsResult> _callback, int _time){
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
        }

        public LocalAdsInfo GetAds(object _adsPlacement){
           var _adsList = getAdsList(_adsPlacement);

           if(_adsList.Count() <= 0){
               return null;
           }

           int _totalWeight = _adsList.Sum(_row => _row.ViewWeight);
           int _selectWeight = UnityEngine.Random.Range(0, _totalWeight);
           int _sumWeight = 0;

           foreach(var _ads in _adsList){
               _sumWeight += _ads.ViewWeight;
               if (_sumWeight >= _selectWeight){
                   return _ads;
               }
           }

           return _adsList.ElementAt(0);
        }

        private List<LocalAdsInfo> getAdsList(object _adsPlacement)
        {
            return adsData.Where(_row => _row.IsReady() == true && (_row.AdsPlacement.ToString() == _adsPlacement.ToString())).ToList();
        }
        #endregion
    }

    

    public class LocalAdsInfo{
        public string Id;
        public object AdsPlacement;
        public string PrefabPath;
        public int ViewWeight;
        private Func<bool> isReady;

        public LocalAdsInfo(string _id, object _adsPlacement, string _prefabPath, int _weight, Func<bool> _isReady){
            this.Id = _id;
            this.AdsPlacement = _adsPlacement;
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