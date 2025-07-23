using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;
using UnityEngine;
using UnityEngine.Analytics;

namespace BicUtil.Ads
{
    public class LocalBannerController : MonoBehaviour, IAdsBanner
    {
        public void Destroy()
        {
            Destroy(this.gameObject);
        }
        
        private void OnDestroy() {
            AdsManager.Instance.RemoveBanner(this);
            BicTween.Cancel(this.gameObject);
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        public bool IsReady()
        {
            return true;
        }

        public void SetPosition(Transform _parent, Vector2 _position)
        {
            this.transform.SetParent(_parent);
            this.transform.localPosition = _position;
            this.transform.localScale = new Vector2(1f, 1f);
        }

        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        public void MoveToStore(string _iosId, string _androidId, string _campainName){
            UnityEngine.Analytics.Analytics.CustomEvent("clickHouseAds", new Dictionary<string, object>
            {
                { "type", "houseAds"  },
                { "id", _androidId},
                { "campaign", _campainName}
            });

            #if UNITY_EDITOR
            Application.OpenURL("https://play.google.com/store/apps/details?id="+_androidId);
            #elif UNITY_IOS
            Application.OpenURL ("itms-apps://itunes.apple.com/app/id" + _iosId);
            #elif UNITY_ANDROID
            Application.OpenURL("market://details?id="+_androidId+"&referrer=utm_source%3Dbigjam%26utm_campaign%3D"+_campainName);
            #else
            Application.OpenURL("https://play.google.com/store/apps/details?id="+_androidId);
            #endif
        }
    }
}