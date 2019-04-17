using UnityEngine;
#if BICUTIL_APPLOVIN
using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;

namespace BicUtil.Ads
{
    public class AppLovinBannerController : MonoBehaviour, IAdsBanner
    {
        #region InstantData
        #endregion

        #region LifeCycle
        private void OnDestroy() {
            AdsManager.Instance.RemoveBanner(this);
        }
        #endregion

        #region Logic
        private object adsType;
        private Action<IAdsBanner> onLoadBannerAction;
        public void Load(string _unitId, object _adsType, Action<IAdsBanner> _onLoadBannerAction){
            adsType = _adsType;
            onLoadBannerAction = _onLoadBannerAction;
        }

        private void onLoaded(object sender, EventArgs e)
        {
            onLoadBannerAction(this);
        }
        #endregion

        #region IAdsBanner
        public void Destroy()
        {
            Destroy(this.gameObject);
        }

        public void Hide()
        {
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
                
            }else{
                
            }
        }

        public void Show()
        {
            
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