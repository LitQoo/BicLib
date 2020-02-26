using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static BicUtil.UI.Button;

namespace BicUtil.UI{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class BicButton : MonoBehaviour, IPointerDownHandler, IPointerExitHandler
    {
        #region static
        public static Action<string> OnClicked = null;
        #endregion

        #region Setting
        [SerializeField]
        private string buttonName = "";
        [SerializeField]
        private Button button;
        [SerializeField]
        private float onClickScaleUpRate = 1.1f;
        [SerializeField]
        private float onClickScaleDownTime = 0.1f;
        [SerializeField]
        private float disableTouchTimeAfterTouch = 0.5f;
        #endregion

        #region LifeCycle
        private void Awake() {
            if(button == null){
                button = GetComponent<Button>();
            }
            button.onClick.AddListener(playScaleTween);    
            button.onClick.AddListener(disableTouch);    
            button.onClick.AddListener(callStaticCallback);
        }

        private void callStaticCallback()
        {
            OnClicked(string.IsNullOrEmpty(this.buttonName)?this.gameObject.name:this.buttonName);
        }

        private void disableTouch()
        {
            if(disableTouchTimeAfterTouch > 0){
                button.interactable = false;
                BicTween.Delay(this.disableTouchTimeAfterTouch).SubscribeComplete(()=>{
                    button.interactable = true;
                }).SetTargetObject(this.gameObject);
            }
        }
        #endregion

        #region Logic
        private TweenTracker scaleTracker = new TweenTracker();
        private void playScaleTween()
        {
            if(onClickScaleUpRate != 1f){
                scaleTracker.Cancel();
                this.transform.localScale = new Vector2(onClickScaleUpRate, onClickScaleUpRate);
                BicTween.Scale(this.gameObject, Vector2.one, onClickScaleDownTime).SetTracker(scaleTracker);   
            }
        }

        public void OnPointerDown(PointerEventData eventData){
            if(button.interactable == false){
                return;
            }

            if(onClickScaleUpRate != 1f){
                this.transform.localScale = new Vector2(onClickScaleUpRate, onClickScaleUpRate);
            }
        }

        public void OnPointerExit(PointerEventData eventData){
            if(button.interactable == false){
                return;
            }

            if(onClickScaleUpRate != 1f){
                this.transform.localScale = Vector2.one;
            }
        }

        private void OnDestroy() {
            scaleTracker.Cancel();    
            BicTween.Cancel(this.gameObject);
        }
        #endregion
    }

}