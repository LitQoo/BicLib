using UnityEngine;
#if BICUTIL_ADMOB
using System;
using BicUtil.Tween;
using GoogleMobileAds.Api;

namespace BicUtil.Ads
{
    public class AdmobBannerController : MonoBehaviour, IAdsBanner
    {   
        private static float reloadTime = 30f; 
        #region InstantData
        private BannerView bannerView;
        #endregion

        #region LifeCycle
        private void OnDestroy() {
            BicTween.Cancel(this.gameObject);
            AdsManager.Instance.RemoveBanner(this);
            if(bannerView != null){
                bannerView.Destroy();
                bannerView = null;
            }
        }
        #endregion

        #region Logic
        private object adsPlacement;
        private Action<IAdsBanner> onLoadBannerAction;
        public void Load(string _unitId, object _adsPlacement, Action<IAdsBanner> _onLoadBannerAction, AdRequest _request){
            adsPlacement = _adsPlacement;
            onLoadBannerAction = _onLoadBannerAction;
            bannerView = new BannerView(_unitId, AdSize.Banner, AdPosition.Top);
            bannerView.OnBannerAdLoadFailed += _error=>reloadBanner();
            bannerView.OnBannerAdLoaded += onLoaded;
            BicTween.Delay(0.1f).SubscribeComplete(()=>{
                bannerView.LoadAd(_request);
            }).SetTargetObject(this.gameObject);
        }

        private void onLoaded()
        {
            reloadTime = 30f;
            BicTween.RunOnMainThread(()=>onLoadBannerAction(this));
        }

        private void reloadBanner(object sender, AdFailedToLoadEventArgs e)
        {
            BicTween.RunOnMainThread(reloadBanner);
        }


        private void reloadBanner(){
            reloadTime = Mathf.Min(reloadTime * 2f, 600f);
            BicTween.Delay(reloadTime).SubscribeComplete(()=>{
                var _log = "reload time is " + reloadTime.ToString();
                try{
                    if(adsPlacement == null){
                        _log += "/adsPlacement is null";
                    }

                    if(AdsManager.Instance == null){
                        _log += "/AdsManager.Instance is null";
                    }

                    if(this.gameObject == null){
                        _log += "/this.gameobejct is null";
                    }

                    AdsManager.Instance.CreateBanner(adsPlacement, Color.black, onLoadBannerAction);
                    _log += "/2";
                    Destroy(this.gameObject);
                    _log += "/3";
                }catch(System.Exception _e){
                    Debug.LogError("reload banner error " + _log);
                    BicUtil.Analytics.Analytics.LogException(_e);
                }
            }).SetTargetObject(this.gameObject);
        }
        #endregion

        #region IAdsBanner
        public void Destroy()
        {
            BicTween.Cancel(this.gameObject);
            Destroy(this.gameObject);
            if(bannerView != null){
                bannerView.Destroy();
                bannerView = null;
            }
        }

        public void Hide()
        {
            bannerView.Hide();
        }

        public bool IsReady()
        {
            return true;
        }

        public void SetPosition(Transform _parent, Vector2 _position)
        {
            this.transform.SetParent(_parent);
            this.transform.localPosition = _position;

            if(_parent.localPosition.y > 0){
                bannerView.SetPosition(AdPosition.Top);
            }else{
                bannerView.SetPosition(AdPosition.Bottom);
            }
        }

        public void Show()
        {
            bannerView.Show();
        }
        #endregion
    }
}
#else
namespace BicUtil.Ads
{
    public class AdmobBannerController : MonoBehaviour{

    }
}
#endif