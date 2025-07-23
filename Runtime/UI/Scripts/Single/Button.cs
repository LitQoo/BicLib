using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BicUtil.UI{
    public class Button : UnityEngine.UI.Button, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler, IUIColorSupport
    {
        #region Color
        public bool UseCustomColor = false;
        public void SetUIColor(UIColorType _type, Color _color){
            if(this.UseCustomColor == true){
                return;
            }

            switch(_type){
                case UIColorType.Point:
                this.image.color = _color;
                break;
            }
        }
        #endregion
        
        #region Setting
        //[SerializeField]
        private float onClickScaleUpRate = 1.05f;
        //[SerializeField]
        private float disableTouchTimeAfterTouch = 0.5f;
        #endregion

        #region PointerTouch
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
            if(this.interactable == false){
                return;
            }

            if(onClickScaleUpRate != 1f){
                this.transform.localScale = new Vector2(onClickScaleUpRate, onClickScaleUpRate);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData){
            if(this.interactable == false){
                return;
            }

            if(onClickScaleUpRate != 1f){
                this.transform.localScale = Vector2.one;
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData){
            if(this.interactable == false){
                return;
            }

            if(onClickScaleUpRate != 1f){
                this.transform.localScale = Vector2.one;
            }
        }
        #endregion

        #region Cycle
        protected override void Awake() {
            base.Awake();

            if(disableTouchTimeAfterTouch > 0){
                this.onClick.AddListener(disableTouch);
            }    
        }
        #endregion

        #region DisableAfterTouch
        private void disableTouch()
        {
            if(disableTouchTimeAfterTouch > 0){
                this.interactable = false;
                BicTween.Delay(this.disableTouchTimeAfterTouch).SubscribeComplete(()=>{
                    this.interactable = true;
                }).SetTargetObject(this.gameObject);
            }
        }

        protected override void OnDestroy() {
            try{
                BicTween.Cancel(this.gameObject);
            }catch{

            }

            base.OnDestroy();
        }
        #endregion
    }


    public interface IUIColorSupport{
        void SetUIColor(UIColorType _type, Color _color);
    }

    public enum UIColorType
    {
        Background,
        Point,
        TextOnBackground,
        TextOnPoint
    }
}
