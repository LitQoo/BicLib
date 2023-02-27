using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BicUtil.Teacher{
    [Serializable]
    public class TeacherController : MonoBehaviour
    {
        [SerializeField]
        internal Sprite pointerOne;
        [SerializeField]
        internal Sprite pointerOneDown;
        [SerializeField]
        internal Sprite pointerTwo;
        [SerializeField]
        internal Sprite pointerZoomReady;
        [SerializeField]
        internal Sprite pointerZoomOut;
        [SerializeField]
        internal Sprite pointerZoomIn;

        [SerializeField]
        internal UnityEngine.UI.Image pointer;
        [SerializeField]
        internal UnityEngine.UI.Image dimmed;
        [SerializeField]
        internal UnityEngine.UI.Text title;
        [SerializeField]
        internal UnityEngine.UI.Text discription;
        [SerializeField]
        internal UnityEngine.UI.Image[] Spotlights;
        [SerializeField]
        internal TouchNotifier.TouchNotifier touchModule;
        [SerializeField]
        internal UnityEngine.UI.Button button;
        [SerializeField]
        internal UnityEngine.UI.Text buttonText;

        private BicUtil.Tween.Tween scenarioTween = null;
        
        internal BicUtil.Tween.Tween parentTween = null; 

        private Teacher teacher = null;



        private void Awake() {
            hideObjects();

            dimmed.rectTransform.sizeDelta = BicUtil.CameraScaler.CameraScaler.Instance.FullResolutionWithoutSafeArea;
            BicUtil.CameraScaler.CameraScaler.Instance.SubscribeChangedScreenSize(()=>{
                dimmed.rectTransform.sizeDelta = BicUtil.CameraScaler.CameraScaler.Instance.FullResolutionWithoutSafeArea;
            });
        }

        private void hideObjects(){
            this.pointer.gameObject.SetActive(false);
            this.dimmed.gameObject.SetActive(false);
            this.title.gameObject.SetActive(false);
            this.discription.gameObject.SetActive(false);
            this.button.gameObject.SetActive(false);
            
            foreach(var _spot in Spotlights){
                _spot.gameObject.SetActive(false);
            }
        }

        public BicUtil.Tween.Tween Play(Action<Teacher> _func){
            if(this.teacher == null){
                this.teacher = new Teacher(this); 
            }

            scenarioTween = BicTween.Sequance();
            parentTween = scenarioTween;
            scenarioTween.SubscribeStart(()=>{
                this.gameObject.SetActive(true);
                hideObjects();
            });

            _func(this.teacher);
            var _tween = scenarioTween;
            _tween.SubscribeComplete(()=>{
                this.gameObject.SetActive(false);
                hideObjects();
            });

            return _tween.Play();
        }

        public void NextStep(){
            var _child = scenarioTween.GetPlayingChild();
            if(_child != null){
                if(_child.Type == TweenType.WaitInput){
                _child.Data = "Next";
                }else{
                    Debug.LogError("Tween Type error, it's not TweenType.WaitInput");
                }
            }
        }

        internal void spawn(Action _func){
            if(parentTween != scenarioTween){
                throw new SystemException("not support spawn in spawn");
            }

            parentTween = BicTween.Spawn();
            _func();
            parentTween.AddTo(scenarioTween);
            parentTween = scenarioTween;
        }
    }

    // public interface ITeacher{
    //     void SetPointer(PointerType _type);
    //     void SetPosition(Vector2 _position);
    //     void SetPositionFollow(GameObject _target);
    //     void MovePointerTo(Vector2 _position, float _time);
    //     void MovePointerFollow(GameObject _target, float _time);
    //     void MovePointerFollow(GameObject _target, Vector2 _offset, float _time);
    //     void Delay(float _time);
    //     void SetDimmed(bool _isActive, Color _color, float _time);
    //     void SetPointerWithScaleUp(PointerType _type);
    //     void HidePointerWithScaleDown(PointerType _type);
    //     void SetPointerWithTween(PointerType _type, Func<GameObject, Tween.Tween> _func);
    //     void ClearDiscription();
    //     void ClearTitle();
    //     void SetTitle(string _title, Color _color);
    //     void SetDiscription(string _message, Color _color);
    //     void ClearTitleAndDiscription();
    //     void SetSpotLightWithScaleUp(int _index, Vector2 _position, Vector2 _size);
    //     void HideSpotLightWithScaleDown(int _index);
    //     void Spawn(Action _func);

    // }

    public enum TeachPointerType{
        None,        
        One,
        OneDown,
        OneUp,
        OneDownWithSimulate,
        OneUpWithSimulate,
        Two,
        ZoomIn,
        ZoomOut,
        ZoomReady
    }

    public class Teacher{
        private TeacherController controller;
        
        public Teacher(TeacherController _controller){
            this.controller = _controller;
        }


        public void Spawn(Action _scenario){
            controller.spawn(_scenario);
        }

        public void WaitClickedButton(string _buttonText){
            SetButton(_buttonText);
            BicTween.WaitInputData().AddTo(controller.parentTween);
            HideButton();
        }

        private Vector2 touchBeganPosition;
        public void SetPointerWithScaleUp(TeachPointerType _type){
            this.SetPointerWithTween(_type, _pointer=>{
                return BicTween.Scale(_pointer, Vector2.zero, Vector2.one, 0.2f).SubscribeStart(()=>{
                    _pointer.transform.localScale = Vector2.zero;
                }).SetEase(EaseType.OutBack);
            });
        }


        public void SetButton(string _title){
            BicTween.Delay(0f).SubscribeStart(()=>
            {
                controller.button.gameObject.SetActive(true);
                controller.buttonText.text = _title;
            }).AddTo(controller.parentTween);
        }

        public void HideButton(){
            BicTween.Delay(0f).SubscribeStart(()=>
            {
                controller.button.gameObject.SetActive(false);
            }).AddTo(controller.parentTween);
        }

        public void SetTitle(string _title, Color _color){
            BicTween.Delay(0f).SubscribeStart(()=>
            {
                controller.title.gameObject.SetActive(true);
                controller.title.text = _title;
                controller.title.color = _color;
            }).AddTo(controller.parentTween);
        }

        public void SetDiscription(string _message, Color _color){
            BicTween.Delay(0f).SubscribeStart(()=>
            {
                controller.discription.gameObject.SetActive(true);
                controller.discription.text = _message;
                controller.discription.color = _color;
            }).AddTo(controller.parentTween);
        }

        public void SetDiscriptionTypeWriter(string _message, Color _color){
            BicTween.TypeWriter(controller.discription, _message, 0.07f).SubscribeStart(()=>
            {
                controller.discription.gameObject.SetActive(true);
                controller.discription.text = "";
                controller.discription.color = _color;
            }).AddTo(controller.parentTween);
        }

        public void ClearTitleAndDiscription(){
            BicTween.Delay(0f).SubscribeStart(()=>
            {
                controller.title.gameObject.SetActive(false);
                controller.title.text = string.Empty;
                controller.discription.gameObject.SetActive(false);
                controller.discription.text = string.Empty;
            }).AddTo(controller.parentTween);
        }

        public void ClearTitle(){
            BicTween.Delay(0f).SubscribeStart(()=>
            {
                controller.title.gameObject.SetActive(false);
                controller.title.text = string.Empty;
            }).AddTo(controller.parentTween);
        }

        public void ClearDiscription(){

            BicTween.Delay(0f).SubscribeStart(()=>
            {
                controller.discription.gameObject.SetActive(false);
                controller.discription.text = string.Empty;
            }).AddTo(controller.parentTween);
        }

        public void HidePointerWithScaleDown(TeachPointerType _type){
            this.SetPointerWithTween(_type, _pointer=>{
                var _tween = BicTween.Scale(_pointer, Vector2.one, Vector2.zero, 0.2f);
                _tween.SubscribeStart(()=>{
                    _tween.OriginValue = _pointer.transform.localScale;
                    _tween.DiffValue = Vector2.zero - (Vector2)_tween.OriginValue;
                }).SetEase(EaseType.OutQuad).SubscribeComplete(()=>{
                    _pointer.transform.localScale = Vector2.one;
                    _pointer.gameObject.SetActive(false);
                });
                return _tween;
            });
        }

        public void SetPointerWithTween(TeachPointerType _type, Func<GameObject, Tween.Tween> _func){
            var _tween = _func(this.controller.pointer.gameObject);
            _tween.SubscribeStart(()=>{
                setPointer(_type);
            }).AddTo(controller.parentTween);
        }

        public void DoubleTapWithSimulate(){
            this.SetPointer(TeachPointerType.OneDownWithSimulate);
            this.Delay(0.05f);
            this.SetPointer(TeachPointerType.OneUpWithSimulate);
            this.Delay(0.05f);
            this.SetPointer(TeachPointerType.OneDownWithSimulate);
            this.Delay(0.05f);
            this.SetPointer(TeachPointerType.OneUpWithSimulate);
        }

        public void SingleTapWithSimulate(){
            this.SetPointer(TeachPointerType.OneDownWithSimulate);
            this.Delay(0.05f);
            this.SetPointer(TeachPointerType.OneUpWithSimulate);
        }

        public void SetPointer(TeachPointerType _type){
            BicTween.Delay(0f).SubscribeStart(()=>
            {
                setPointer(_type);
            }).AddTo(controller.parentTween);
        }

        private void setPointer(TeachPointerType _type)
        {
            controller.pointer.gameObject.SetActive(true);
            switch (_type)
            {
                case TeachPointerType.None: controller.pointer.gameObject.SetActive(false); break;
                case TeachPointerType.One:
                    controller.pointer.sprite = controller.pointerOne;
                    break;
                case TeachPointerType.OneUp:
                case TeachPointerType.OneUpWithSimulate:
                    controller.pointer.sprite = controller.pointerOne;
                    if (controller.touchModule != null &&  _type == TeachPointerType.OneUpWithSimulate)
                    {
                        controller.touchModule.OnTouchUp.Invoke(controller.pointer.transform.position, controller.pointer.transform.position, 0);
                    }

                    break;
                case TeachPointerType.OneDown:
                case TeachPointerType.OneDownWithSimulate:
                    controller.pointer.sprite = controller.pointerOneDown;
                    if (controller.touchModule != null &&  _type == TeachPointerType.OneDownWithSimulate)
                    {
                        touchBeganPosition = controller.pointer.transform.position;
                        controller.touchModule.OnTouchDown.Invoke(controller.pointer.transform.position, 0);
                    }
                    break;
                case TeachPointerType.Two: controller.pointer.sprite = controller.pointerTwo; break;
                case TeachPointerType.ZoomIn: controller.pointer.sprite = controller.pointerZoomIn; break;
                case TeachPointerType.ZoomOut: controller.pointer.sprite = controller.pointerZoomOut; break;
                case TeachPointerType.ZoomReady: controller.pointer.sprite = controller.pointerZoomReady; break;
            }
        }

        public void SetPosition(Vector2 _position){
            BicTween.Delay(0f).SubscribeStart(()=>{
                controller.pointer.gameObject.transform.position = _position;
            }).AddTo(controller.parentTween);
        }

        public void SetPositionFollow(GameObject _target){
            BicTween.Delay(0f).SubscribeStart(()=>{
                controller.pointer.gameObject.transform.position = _target.transform.position;
            }).AddTo(controller.parentTween);
        }

        public void ZoomWithSimulate(float _size, float _time){
            var _firstTouchSize = 30f*Vector2.one;
            BicTween.Value(0, _size, _time).SubscribeStart(()=>
            {
                if(_size > 0){
                    setPointer(TeachPointerType.ZoomIn);
                }else{
                    setPointer(TeachPointerType.ZoomOut);
                }
                touchBeganPosition = controller.pointer.transform.position;
            
                controller.touchModule.GetTouchReturnForced = true;
                controller.touchModule.SetTouchCountForced(2);
                controller.touchModule.SetStartPositionForced(0, touchBeganPosition + _firstTouchSize);
                controller.touchModule.OnTouchDown.Invoke(touchBeganPosition + _firstTouchSize, 0);
                controller.touchModule.OnTouchDown.Invoke(touchBeganPosition - _firstTouchSize, 1);
            }).SubscribeUpdate(_value=>{
                if(controller.touchModule != null){
                    controller.touchModule.SetTouchCountForced(2);
                    controller.touchModule.SetStartPositionForced(0, touchBeganPosition + _firstTouchSize);
                    controller.touchModule.OnTouchMove.Invoke(touchBeganPosition + _firstTouchSize, touchBeganPosition + _firstTouchSize + Vector2.one*_value.x, 0);
                    controller.touchModule.OnTouchMove.Invoke(touchBeganPosition - _firstTouchSize, touchBeganPosition - _firstTouchSize - Vector2.one*_value.x, 1);
                }
            }).SubscribeComplete(()=>{
                setPointer(TeachPointerType.ZoomReady);
                controller.touchModule.SetTouchCountForced(2);
                controller.touchModule.OnTouchUp.Invoke(touchBeganPosition + _firstTouchSize, touchBeganPosition + _firstTouchSize + Vector2.one * _size, 0);
                controller.touchModule.OnTouchUp.Invoke(touchBeganPosition - _firstTouchSize, touchBeganPosition - _firstTouchSize - Vector2.one * _size, 1);
                controller.touchModule.SetTouchCountForced(0);
                controller.touchModule.GetTouchReturnForced = false;
            }).AddTo(controller.parentTween);

            
        }

        public void MovePointerTo(Vector2 _position, float _time, bool _simulateTouch){
            var _tween = BicTween.MoveWorld(controller.pointer.gameObject, _position, _time);

            _tween.SubscribeStart(()=>{
                _tween.OriginValue = this.controller.pointer.transform.position;
                _tween.DiffValue = _position - (Vector2)_tween.OriginValue; 
            }).SubscribeUpdate(_position=>{
                if(controller.touchModule != null && _simulateTouch == true){
                    controller.touchModule.OnTouchMove.Invoke(touchBeganPosition, _position, 0);
                }
            }).AddTo(controller.parentTween);
        }

        public void MovePointerFollow(GameObject _target, float _time){
            
            var _tween = BicTween.MoveWorld(controller.pointer.gameObject, _target.transform.position, _time);
            
            _tween.SubscribeStart(()=>{
                _tween.OriginValue = this.controller.pointer.transform.position;
                _tween.DiffValue = _target.transform.position - (Vector3)_tween.OriginValue; 
            }).AddTo(controller.parentTween);
        }

        public void MovePointerFollow(GameObject _target, Vector2 _offset, float _time){
            var _tween = BicTween.MoveWorld(controller.pointer.gameObject, _target.transform.position, _time);

            _tween.SubscribeStart(()=>{
                _tween.OriginValue = this.controller.pointer.transform.position;
                _tween.DiffValue = _target.transform.position + (Vector3)_offset - (Vector3)_tween.OriginValue; 
            }).AddTo(controller.parentTween);
        }

        public void Delay(float _time){
            BicTween.Delay(_time).AddTo(controller.parentTween);
        }

        public void ClickButton(UnityEngine.UI.Button _button){
            throw new NotFiniteNumberException();
            //ExecuteEvents.Execute(button.gameObject, new BaseEventData(eventSystem), ExecuteEvents.submitHandler);
        }

        public void SetDimmed(bool _isActive, Color _color, float _time){
            var _tween = BicTween.Value((Vector4)controller.dimmed.color, (Vector4)_color, _time);
            _tween.SubscribeStart(()=>{
                if(_isActive == true){
                    if(controller.dimmed.gameObject.activeSelf == false){
                        controller.dimmed.color = Color.clear;
                    }

                    controller.dimmed.gameObject.SetActive(true);
                }

                _tween.OriginValue = (Vector4)controller.dimmed.color;
                _tween.DiffValue = (Vector4)_color - _tween.OriginValue;
            }).SubscribeUpdate(_value=>{
                controller.dimmed.color = _value;
            }).SubscribeComplete(()=>{
                controller.dimmed.gameObject.SetActive(_isActive);
            }).AddTo(controller.parentTween);
        }

        public void SetSpotlight(int _index, Vector2 _position, Vector2 _size){
            BicTween.Delay(0).SubscribeStart(()=>
            {
                setSpotlight(_index, _position, _size);
            }).AddTo(controller.parentTween);

        }

        public void SetSpotlightWithScaleUp(int _index, Vector2 _position, Vector2 _size){
            BicTween.Scale(controller.Spotlights[_index].gameObject, Vector2.zero, Vector2.one, 0.2f).SubscribeStart(()=>{
                controller.Spotlights[_index].transform.localScale = Vector2.zero;
                controller.Spotlights[_index].gameObject.SetActive(true);
                setSpotlight(_index, _position, _size);
            }).AddTo(controller.parentTween).SetEase(EaseType.OutBack);
        }

        public void HideSpotlightWithScaleDown(int _index){
            BicTween.Scale(controller.Spotlights[_index].gameObject, Vector2.one, Vector2.zero, 0.2f).SubscribeStart(()=>{
                controller.Spotlights[_index].transform.localScale = Vector2.one;
                controller.Spotlights[_index].gameObject.SetActive(true);
            }).AddTo(controller.parentTween).SetEase(EaseType.OutBack).SubscribeComplete(()=>{
                controller.Spotlights[_index].transform.localScale = Vector2.one;
                controller.Spotlights[_index].gameObject.SetActive(false);
            });
        }

        private void setSpotlight(int _index, Vector2 _position, Vector2 _size)
        {
            controller.Spotlights[_index].gameObject.SetActive(true);
            controller.Spotlights[_index].transform.position = _position;
            controller.Spotlights[_index].rectTransform.sizeDelta = _size;
        }

        public void MoveSpotlight(int _index, Vector2 _position, float _time){
            var _spot = controller.Spotlights[_index];
            var _tween = BicTween.MoveWorld(_spot.gameObject, _position, _time);

            _tween.SubscribeStart(()=>{
                _tween.OriginValue = _spot.transform.position;
                _tween.DiffValue = _position - (Vector2)_tween.OriginValue; 
            }).AddTo(controller.parentTween);
        }

        public void Action(Action _action){
            var _tween = BicTween.Delay(0f);
            _tween.SubscribeStart(_action).AddTo(controller.parentTween);
        }

    }
}