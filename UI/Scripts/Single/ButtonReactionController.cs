using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BicUtil.UI{
    public class ButtonReactionController : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
    {
        #region Setting
        [SerializeField]
        UnityEngine.UI.Button targetButton;
        [SerializeField][Range(0.5f, 1.5f)]
        private float onClickScaleUpRate = 1.05f;
        [SerializeField][Range(0f, 1f)]
        private float disableTouchTimeAfterTouch = 0.5f;
        #endregion

        #region PointerTouch
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
            if(targetButton.interactable == false){
                return;
            }

            if(onClickScaleUpRate != 1f){
                this.transform.localScale = new Vector2(onClickScaleUpRate, onClickScaleUpRate);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData){
            if(targetButton.interactable == false){
                return;
            }

            if(onClickScaleUpRate != 1f){
                this.transform.localScale = Vector2.one;
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData){
            if(targetButton.interactable == false){
                return;
            }

            if(onClickScaleUpRate != 1f){
                this.transform.localScale = Vector2.one;
            }
        }
        #endregion

        #region Cycle
        protected void Awake() {
            if(disableTouchTimeAfterTouch > 0){
                targetButton.onClick.AddListener(disableTouch);
            }    
        }
        #endregion

        #region DisableAfterTouch
        TweenTracker tracker = new TweenTracker();
        private void disableTouch()
        {
            if(disableTouchTimeAfterTouch > 0){
                targetButton.interactable = false;
                BicTween.Delay(this.disableTouchTimeAfterTouch).SubscribeComplete(()=>{
                    targetButton.interactable = true;
                }).SetTracker(tracker);
            }
        }

        protected void OnDestroy() {
            try{
                tracker.Cancel();
            }catch{

            }
        }
        #endregion
    }
}