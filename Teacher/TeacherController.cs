using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.Tween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BicUtil.Teacher
{
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
        internal UnityEngine.UI.Button dimmedButton;
        [SerializeField]
        internal UnityEngine.UI.Text title;
        [SerializeField]
        internal UnityEngine.UI.Text discription;
        [SerializeField]
        internal UnityEngine.UI.Image[] spotlights;
        [SerializeField]
        internal DottedLineController[] dottedLines;
        [SerializeField]
        internal TouchNotifier.TouchNotifier touchModule;
        [SerializeField]
        internal UnityEngine.UI.Button button;
        [SerializeField]
        internal UnityEngine.UI.Text buttonText;

        internal BicUtil.Tween.Tween scenarioTween = null;    
        internal BicUtil.Tween.Tween parentTween = null; 
        internal BicUtil.Tween.Tween waitTween = null;    

        private Teacher teacher = null;



        private void Awake() {
            hideObjects();
        }

        private void hideObjects(){
            this.pointer.gameObject.SetActive(false);
            this.dimmed.gameObject.SetActive(false);
            this.dimmedButton.enabled = false;
            this.title.gameObject.SetActive(false);
            this.discription.gameObject.SetActive(false);
            this.button.gameObject.SetActive(false);
            
            foreach(var _spot in spotlights){
                _spot.gameObject.SetActive(false);
            }

            foreach(var _line in dottedLines){
                _line.gameObject.SetActive(false);
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
            var _child = this.waitTween;
            if(_child != null){
                if(_child.Type == TweenType.WaitInput){
                _child.Data = "Next";
                }else{
                    Debug.LogError("Tween Type error, it's not TweenType.WaitInput");
                }
            }
        }

        internal void spawn(Action _func, SpawnType _spawnType = SpawnType.WaitAll){
            // if(parentTween != scenarioTween){
            //     throw new SystemException("not support spawn in spawn");
            // }

            var _newParentTween = BicTween.Spawn(_spawnType);
            var _backup = parentTween;
            parentTween = _newParentTween;
            _func();
            _newParentTween.AddTo(_backup);
            parentTween = _backup;
        }

        internal void sequance(Action _func){
            // if(parentTween != scenarioTween){
            //     throw new SystemException("not support spawn in spawn");
            // }

            var _newParentTween = BicTween.Sequance();
            var _backup = parentTween;
            parentTween = _newParentTween;
            _func();
            _newParentTween.AddTo(_backup);
            parentTween = _backup;
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
}