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
        internal TouchNotifier.TouchNotifier touchModule;

        internal BicUtil.Tween.Tween scenarioTween = null;

        private Teacher teacher = null;



        private void Awake() {
            this.pointer.gameObject.SetActive(false);
        }

        public BicUtil.Tween.Tween Make(Action<ITeacher> _func){
            if(this.teacher == null){
                this.teacher = new Teacher(this); 
            }

            scenarioTween = BicTween.Sequance();
            scenarioTween.SubscribeStart(()=>{
                this.gameObject.SetActive(true);
            });

            _func(this.teacher);
            var _tween = scenarioTween;
            scenarioTween = null;
            _tween.SubscribeComplete(()=>{
                this.gameObject.SetActive(false);
                this.pointer.gameObject.SetActive(false);
            });

            return _tween;
        }
    }

    public interface ITeacher{
        void SetPointer(PointerType _type);
        void SetPosition(Vector2 _position);
        void SetPositionFollow(GameObject _target);
        void MovePointerTo(Vector2 _position, float _time);
        void MovePointerFollow(GameObject _target, float _time);
        void MovePointerFollow(GameObject _target, Vector2 _offset, float _time);
        void Delay(float _time);
    }

    public enum PointerType{
        None,        
        One,
        OneDown,
        OneUp,
        Two,
        ZoomIn,
        ZoomOut,
        ZoomReady
    }

    public class Teacher : ITeacher{
        private TeacherController controller;
        
        public Teacher(TeacherController _controller){
            this.controller = _controller;
        }

        Vector2 downPosition;
        public void SetPointer(PointerType _type){
            BicTween.Delay(0f).SubscribeStart(()=>{
                controller.pointer.gameObject.SetActive(true);
                switch(_type){
                    case PointerType.None:controller.pointer.gameObject.SetActive(false); break;
                    case 
                        PointerType.One:controller.pointer.sprite = controller.pointerOne;
                    break;
                    case 
                        PointerType.OneUp:controller.pointer.sprite = controller.pointerOne;
                        if(controller.touchModule != null){
                            Debug.Log("up");
                            controller.touchModule.OnTouchUp.Invoke(controller.pointer.transform.position, controller.pointer.transform.position, 0);
                        } 

                    break;
                    case PointerType.OneDown:
                        controller.pointer.sprite = controller.pointerOneDown; 
                        if(controller.touchModule != null){
                            Debug.Log("down");
                            downPosition = controller.pointer.transform.position;
                            controller.touchModule.OnTouchDown.Invoke(controller.pointer.transform.position, 0);
                        }
                    break;
                    case PointerType.Two: controller.pointer.sprite = controller.pointerTwo; break;
                    case PointerType.ZoomIn: controller.pointer.sprite = controller.pointerZoomIn;break;
                    case PointerType.ZoomOut:controller.pointer.sprite = controller.pointerZoomOut;break;
                    case PointerType.ZoomReady:controller.pointer.sprite = controller.pointerZoomReady; break;
                }
            }).AddTo(controller.scenarioTween);
        }

        public void SetPosition(Vector2 _position){
            BicTween.Delay(0f).SubscribeStart(()=>{
                controller.pointer.gameObject.transform.position = _position;
            }).AddTo(controller.scenarioTween);
        }

        public void SetPositionFollow(GameObject _target){
            BicTween.Delay(0f).SubscribeStart(()=>{
                controller.pointer.gameObject.transform.position = _target.transform.position;
            }).AddTo(controller.scenarioTween);
        }

        public void MovePointerTo(Vector2 _position, float _time){
            var _tween = BicTween.MoveWorld(controller.pointer.gameObject, _position, _time);

            _tween.SubscribeStart(()=>{
                _tween.OriginValue = this.controller.pointer.transform.position;
                _tween.DiffValue = _position - (Vector2)_tween.OriginValue; 
            }).SubscribeUpdate(_position=>{
                if(controller.touchModule != null){
                    Debug.Log("move");
                    controller.touchModule.OnTouchMove.Invoke(downPosition, _position, 0);
                }
            }).AddTo(controller.scenarioTween);
        }

        public void MovePointerFollow(GameObject _target, float _time){
            
            var _tween = BicTween.MoveWorld(controller.pointer.gameObject, _target.transform.position, _time);
            
            _tween.SubscribeStart(()=>{
                _tween.OriginValue = this.controller.pointer.transform.position;
                _tween.DiffValue = _target.transform.position - (Vector3)_tween.OriginValue; 
            }).AddTo(controller.scenarioTween);
        }

        public void MovePointerFollow(GameObject _target, Vector2 _offset, float _time){
            var _tween = BicTween.MoveWorld(controller.pointer.gameObject, _target.transform.position, _time);

            _tween.SubscribeStart(()=>{
                _tween.OriginValue = this.controller.pointer.transform.position;
                _tween.DiffValue = _target.transform.position + (Vector3)_offset - (Vector3)_tween.OriginValue; 
            }).AddTo(controller.scenarioTween);
        }

        public void Delay(float _time){
            BicTween.Delay(_time).AddTo(controller.scenarioTween);
        }

        public void ClickButton(UnityEngine.UI.Button _button){
            throw new NotFiniteNumberException();
            //ExecuteEvents.Execute(button.gameObject, new BaseEventData(eventSystem), ExecuteEvents.submitHandler);
        }
    }
}