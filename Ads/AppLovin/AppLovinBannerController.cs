using UnityEngine;
#if BICUTIL_APPLOVIN
using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;

namespace BicUtil.Ads
{
    public class AppLovinBannerController : IAdsBanner
    {
        #region InstantData
        #endregion

        #region Logic
        private object adsPlacement;
        private string unitId;
        public void Load(string _unitId, object _adsPlacement){
            unitId = _unitId;
            adsPlacement = _adsPlacement;
            MaxSdk.CreateBanner(_unitId, MaxSdkBase.BannerPosition.TopCenter);
        }
        #endregion

        #region IAdsBanner
        public void Destroy()
        {
            AdsManager.Instance.RemoveBanner(this);
            MaxSdk.HideBanner(unitId);
        }

        public void Hide()
        {
            MaxSdk.HideBanner(unitId);
        }

        public bool IsReady()
        {
            return true;
        }

        public void SetPosition(Transform _parent, Vector2 _position)
        {
            
        }

        public void Show()
        {
            MaxSdk.ShowBanner(unitId);
        }
        #endregion
    }
}
#else
namespace BicUtil.Ads
{
    public class AppLovinBannerController : MonoBehaviour{

    }
}
#endif