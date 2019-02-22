using UnityEngine;
#if BICUTIL_ADMOB
using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;
using GoogleMobileAds.Api;

namespace BicUtil.Ads
{
    public class AdmobBannerController : MonoBehaviour, IAdsBanner
    {
        #region InstantData
        private BannerView bannerView;
        #endregion

        #region LifeCycle
        private void OnDestroy() {
            AdsManager.Instance.RemoveBanner(this);
            bannerView.Destroy();
        }
        #endregion

        #region Logic
        private object adsType;
        private Action<IAdsBanner> onLoadBannerAction;
        public void Load(string _unitId, object _adsType, Action<IAdsBanner> _onLoadBannerAction){
            adsType = _adsType;
            onLoadBannerAction = _onLoadBannerAction;
            bannerView = new BannerView(_unitId, AdSize.SmartBanner, AdPosition.Top);
            bannerView.OnAdFailedToLoad += reloadBanner;
            bannerView.OnAdLoaded += onLoaded;
            AdRequest request = new AdRequest.Builder().Build();
            BicTween.Delay(0.1f).SubscribeComplete(()=>{
                bannerView.LoadAd(request);
            });
        }

        private void onLoaded(object sender, EventArgs e)
        {
            onLoadBannerAction(this);
        }

        private void reloadBanner(object sender, AdFailedToLoadEventArgs e)
        {
            AdsManager.Instance.RemoveBanner(this);
            AdsManager.Instance.CreateBanner(adsType, onLoadBannerAction);
            Destroy(this.gameObject);
        }
        #endregion

        #region IAdsBanner
        public void Destroy()
        {
            AdsManager.Instance.RemoveBanner(this);
            Destroy(this.gameObject);
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