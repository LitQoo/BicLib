using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using BicUtil.Tween;
using System;

namespace BicUtil.Ads
{
    public class HouseInterstitialController : MonoBehaviour
    {
        #region LinkingObject
        [SerializeField]
        private HouseBannerController houseBanner;
        [SerializeField]
        private UnityEngine.UI.Text timeCountText;
        [SerializeField]
        private UnityEngine.UI.Button closeButton;
        #endregion

        #region Event
        public Action OnClose = null;
        #endregion

        #region InstantData
        private int time = 0;
        #endregion

        #region 
        public void Load(object _adsType, HouseAdsModel _model, int _time){
            houseBanner.Load(_adsType.ToString(), _model);
            closeButton.gameObject.SetActive(false);
            timeCountText.gameObject.SetActive(true);
            time = _time;
        }

        public void Show(){
            houseBanner.Show();
            this.gameObject.SetActive(true);
            timeCountText.text = time.ToString();
            BicTween.Interval(1f, time).SubscribeRepeat((_tween, _count)=>{
                timeCountText.text = (time - _count + 1).ToString();
            }).SubscribeComplete(()=>{
                closeButton.gameObject.SetActive(true);
                timeCountText.gameObject.SetActive(false);
            }).SetTargetObject(this.gameObject);
        }

        public void Close(){
            BicTween.Cancel(this.gameObject);

            if(OnClose != null){
                OnClose();
            }

            Destroy(this.gameObject);
        }

        public void MoveToStore(){
            houseBanner.MoveToStore();
            BicTween.Delay(0.5f).SetTargetObject(this.gameObject).SubscribeComplete(this.Close);
        }
        #endregion
    }
}