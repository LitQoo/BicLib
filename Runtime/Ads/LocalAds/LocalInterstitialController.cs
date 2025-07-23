using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;
using UnityEngine;

namespace BicUtil.Ads
{
    public class LocalInterstitialController : MonoBehaviour
    {
        #region LinkingObject
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
        public void Load(int _time){
            if(closeButton != null){
                closeButton.gameObject.SetActive(false);
            }

            if(timeCountText != null){
                timeCountText.gameObject.SetActive(true);
            }

            time = _time;
        }

        public void Show(){
            this.gameObject.SetActive(true);

            if(timeCountText != null){
                timeCountText.text = time.ToString();
            }

            BicTween.Interval(1f, time).SubscribeRepeat((_tween, _count)=>{
                if(timeCountText != null){
                    timeCountText.text = (time - _count + 1).ToString();
                }
            }).SubscribeComplete(()=>{
                if(closeButton != null){
                    closeButton.gameObject.SetActive(true);
                }

                if(timeCountText != null){
                    timeCountText.gameObject.SetActive(false);
                }
            }).SetTargetObject(this.gameObject);
        }

        public void Close(){
            BicTween.Cancel(this.gameObject);

            if(OnClose != null){
                OnClose();
            }

            Destroy(this.gameObject);
        }
        #endregion
    }
}