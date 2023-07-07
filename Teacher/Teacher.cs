using System;
using BicUtil.Tween;
using UnityEngine;

namespace BicUtil.Teacher
{
    public class Teacher{
        private TeacherController controller;
        
        public Teacher(TeacherController _controller){
            this.controller = _controller;
        }


        public void Spawn(Action _scenario, SpawnType _spawnType = SpawnType.WaitAll){
            controller.spawn(_scenario, _spawnType);
        }

        public void Sequance(Action _scenario){
            controller.sequance(_scenario);
        }

        public void WaitClickedButton(string _buttonText){
            SetButton(_buttonText);
            var _tween = BicTween.WaitInputData().AddTo(controller.parentTween);
            _tween.SubscribeStart(()=>controller.waitTween = _tween);
            HideButton();
        }

        public void WaitClickedDimmed(){
            Sequance(()=>{
                EnableDimmedButton();
                var _tween = BicTween.WaitInputData().AddTo(controller.parentTween);
                _tween.SubscribeStart(()=>controller.waitTween = _tween);
                DisableDimmedButton();
            });
        }

        public void Wait(Func<bool> _updateFunc){
            var _tween = BicTween.WaitInputData().AddTo(controller.parentTween);
            _tween.SubscribeStart(()=>controller.waitTween = _tween);
            _tween.SubscribeUpdate(_update=>{
                if(_updateFunc() == true){
                    _tween.Data = "Next";
                }
            });
        }

        private Vector2 touchBeganPosition;
        public void SetPointerWithScaleUp(TeachPointerType _type){
            this.SetPointerWithTween(_type, _pointer=>{
                return BicTween.Scale(_pointer, Vector2.zero, Vector2.one, 0.2f).SubscribeStart(()=>{
                    _pointer.transform.localScale = Vector2.zero;
                }).SetEase(EaseType.OutBack);
            });
        }

        public void EnableDimmedButton(){
            BicTween.Delay(0f).SubscribeStart(()=>
            {
                controller.dimmedButton.enabled = true;
            }).AddTo(controller.parentTween);
        }

        public void DisableDimmedButton(){
            BicTween.Delay(0f).SubscribeStart(()=>
            {
                controller.dimmedButton.enabled = false;
            }).AddTo(controller.parentTween);
        }
        
        public void SetButtonTextColorImmediately(Color _color){
            controller.buttonText.color = _color;
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

        public void SetDiscriptionScaleUp(string _message, Color _color){
            
            BicTween.Scale(controller.discription.gameObject, Vector2.one * 0.5f, Vector2.one, 0.5f).SubscribeStart(()=>{
                controller.discription.gameObject.SetActive(true);
                controller.discription.text = _message;
                controller.discription.color = _color;
                controller.discription.transform.localScale = Vector2.one * 0.5f;
            }).SetEase(EaseType.OutBack).AddTo(controller.parentTween);
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

        public void AddTween(Tween.Tween _tween){
            _tween.AddTo(controller.parentTween);
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
                controller.touchModule.EnableUpdate = false;
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
                controller.touchModule.EnableUpdate = true;
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

        public void HighlightSpotlight(int _index, int _repeat){
            var _seq = BicTween.Sequance();
            var _spot = controller.spotlights[_index].gameObject;
            BicTween.Scale(_spot, Vector2.one, Vector2.one * 1.1f, 0.5f).AddTo(_seq);
            BicTween.Scale(_spot, Vector2.one * 1.1f, Vector2.one, 0.5f).AddTo(_seq);
            _seq.SetRepeat(_repeat);
            _seq.AddTo(controller.parentTween);
        }

        public void SetSpotlightWithScaleUp(int _index, Vector2 _position, Vector2 _size){
            var _spot = controller.spotlights[_index].gameObject;
            BicTween.Scale(_spot, Vector2.zero, Vector2.one, 0.2f).SubscribeStart(()=>{
                _spot.transform.localScale = Vector2.zero;
                _spot.gameObject.SetActive(true);
                setSpotlight(_index, _position, _size);
            }).AddTo(controller.parentTween).SetEase(EaseType.OutBack);
        }

        public void HideSpotlightWithScaleDown(int _index){
            var _spotObject = controller.spotlights[_index];
            var _tween = BicTween.Scale(_spotObject.gameObject, Vector2.one, Vector2.zero, 0.2f);
            
            _tween.SubscribeStart(()=>{
                _tween.OriginValue = _spotObject.transform.localScale;
                _tween.DiffValue = -_spotObject.transform.localScale;
                _spotObject.gameObject.SetActive(true);
            }).AddTo(controller.parentTween).SetEase(EaseType.OutBack).SubscribeComplete(()=>{
                _spotObject.transform.localScale = Vector2.one;
                _spotObject.gameObject.SetActive(false);
            });
        }

        private void setSpotlight(int _index, Vector2 _position, Vector2 _size)
        {
            var _spotObject = controller.spotlights[_index];
            _spotObject.gameObject.SetActive(true);
            _spotObject.transform.position = _position;
            _spotObject.rectTransform.sizeDelta = _size;
        }

        public void MoveSpotlight(int _index, Vector2 _position, float _time){
            var _spot = controller.spotlights[_index];
            var _tween = BicTween.MoveWorld(_spot.gameObject, _position, _time);

            _tween.SubscribeStart(()=>{
                _tween.OriginValue = _spot.transform.position;
                _tween.DiffValue = _position - (Vector2)_tween.OriginValue; 
            }).AddTo(controller.parentTween);
        }

        public void SetDottedLine(int _index, Vector2 _positionStart, Vector2 _positionEnd, Color _color, float _width, float _space, float _speed = 0f, float _startOffset = 0f, float _endOffset = 0f){
            var _line = controller.dottedLines[_index];
            BicTween.Delay(0).SubscribeStart(()=>{
                _line.SetColor(_color);
                _line.SetPosition(new Vector3[]{_positionStart, _positionEnd}, _startOffset, _endOffset, false);
                _line.SetDotSize(_width, false); 
                _line.SetSpeed(_speed);
                _line.SetSpacing(_space);
                _line.gameObject.SetActive(true);
            }).AddTo(controller.parentTween);
        }

        public void HideDottedLine(int _index){
            BicTween.Delay(0).SubscribeStart(()=>
            {
                var _line = controller.dottedLines[_index];
                _line.gameObject.SetActive(false);
            }).AddTo(controller.parentTween);
        }

        public void Action(Action _action){
            var _tween = BicTween.Delay(0f);
            _tween.SubscribeStart(_action).AddTo(controller.parentTween);
        }

    }

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
}