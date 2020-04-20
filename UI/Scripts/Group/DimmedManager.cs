using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;
using UnityEngine;
using BicUtil.UIFlow;
using System;

namespace BicUtil.UI{
    public class DimmedManager : MonoBehaviour
    {
        #region Static
        static public DimmedManager Instance{
            get;
            private set;
        }
        #endregion

        #region DI
        [SerializeField]
        private UnityEngine.UI.Image image;
        [SerializeField]
        private UnityEngine.UI.Text message;
        #endregion

        #region InstantData
        [SerializeField]
        private int count = 0;
        private int lastDimmedFrame = 0;

        private int dimmedCount{
            get{
                return count;
            }

            set{
                count = Mathf.Max(value, 0);
                image.gameObject.SetActive(count > 0);
                lastDimmedFrame = Time.frameCount;
                if(count <= 0){
                    image.color = Color.clear;
                    message.text = "";
                }
            }
        }

        public int LastDimmedFrame{
            get{
                return lastDimmedFrame;
            }
        }

        public bool IsEnabled{
            get{
                return dimmedCount > 0;
            }
        }
        
        TweenTracker tweenTracker = new TweenTracker();
        #endregion

        #region Event
        private void Awake(){
            Instance = this;
        }

        private void OnEnable() {
            Instance = this;
        }
        #endregion

        #region Logic
        public void Enable(){
            Enable(Color.clear, "");
        }

        public void Enable(Color _color, string _text = ""){
            if(tweenTracker.IsComplete == true){
                this.image.color = _color; 
            }

            this.message.text = _text;
            dimmedCount++;

            #if UNITY_EDITOR
            //this.image.color = new Color(1f, 0f, 0f, 0.3f);
            Debug.Log("DimmedManager.Enable " + dimmedCount.ToString());
            #endif
        }

        public void Disable(){
            dimmedCount--;

            #if UNITY_EDITOR
            if(dimmedCount <= 0){
                Debug.Log("DimmedManager.Disable 0 / OK");    
            }else{
                Debug.Log("DimmedManager.Disable " + dimmedCount.ToString());
            }
            #endif
        }

        public TweenModel PlayAlphaTween(Color _color, float _fromAlpha, float _toAlpha, float _time){
            return BicTween.Value(_fromAlpha, _toAlpha, _time).SubscribeUpdate(_value=>{
                this.image.color = new Color(_color.r, _color.g, _color.b, _value.x);
            }).SetTracker(tweenTracker);
        }

        public TweenModel PlayTransitionTween(Action _transitionAction, float _time = 2f){
            var _multi = BicTween.Multi();
            dimmedCount++;
            this.message.text = "";
            this.image.color = Color.clear;
            _multi.Sequance(()=>{
                PlayAlphaTween(Color.black, 0f, 1f, _time / 2f).AddTo(_multi).SubscribeComplete(()=>{
                    _transitionAction();
                });

                PlayAlphaTween(Color.black, 1f, 0f, _time / 2f).AddTo(_multi);
            });

            return _multi.Play().SetTracker(tweenTracker).SubscribeComplete(()=>{
                dimmedCount--;
            });
            
        }
        #endregion
    }
}
