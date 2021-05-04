using System;
using UnityEngine;

namespace BicUtil.TouchNotifier
{
    public class MultiTouchController : MonoBehaviour
    {
        #region DI
        [SerializeField]
        private RectTransform targetTrasnform;
        [SerializeField]
        private TouchNotifier touchNotifier;
        #endregion

        #region Instant
        public bool IsMultiTouch = false;
        public float Scale = 1f;
        private Vector2 lastBoardPosition = Vector2.zero;
        private float distanceBetweenTouches = 0f;
        private float lastBoardScale = 1f;
        private Vector2 multiTouchMidPosition = Vector2.zero;
        #endregion

        #region Event
        public Action<Vector2> onSingleTouchDown;
        public Action<Vector2, Vector2> onSingleTouchMove;
        public Action<Vector2, Vector2> onSingleTouchUp;
        #endregion

        #region LifeCycle
        private void Awake() {
            touchNotifier.OnTouchUp.AddListener(onTouchUp);
            touchNotifier.OnTouchDown.AddListener(onTouchDown);
            touchNotifier.OnTouchMove.AddListener(onTouchMove);
            Init();
        }

        private void OnDestroy() {
            touchNotifier.OnTouchUp.RemoveListener(onTouchUp);
            touchNotifier.OnTouchDown.RemoveListener(onTouchDown);
            touchNotifier.OnTouchMove.RemoveListener(onTouchMove);
        }
        #endregion

        #region Touch
        private void onTouchDown(Vector2 _currentPosition, int _touchIndex)
        {
            if(this.gameObject.activeInHierarchy == false){
                return;
            }

            if(this.touchNotifier.TouchCount > 1){
                if(_touchIndex == 1)
                {
                    onMultiTouchDown(_currentPosition);
                }

                return;
            }


            if(_touchIndex == 0 && onSingleTouchDown != null){
                this.onSingleTouchDown(_currentPosition);
            }
        }

        private void onTouchMove(Vector2 _startPosition, Vector2 _currentPosition, int _touchIndex)
        {
            if(this.gameObject.activeInHierarchy == false){
                return;
            }

            if(this.touchNotifier.TouchCount > 1){
                if(_touchIndex == 1)
                {
                    onMultiTouchMove(_startPosition, _currentPosition);
                }
                return;
            }

            if(_touchIndex == 0 && onSingleTouchMove != null){
                this.onSingleTouchMove(_startPosition, _currentPosition);
            }
        }

        private void onTouchUp(Vector2 _startPosition, Vector2 _currentPosition, int _touchIndex)
        {
            if(this.gameObject.activeInHierarchy == false){
                return;
            }

            if(_touchIndex == 0 && onSingleTouchUp != null){
                onSingleTouchUp(_startPosition, _currentPosition);
            }

            if(IsMultiTouch == true && touchNotifier.TouchCount <= 0){
                IsMultiTouch = false;
            }
        }
        #endregion

        #region MultiTouch

        private void onMultiTouchDown(Vector2 _currentPosition)
        {
            IsMultiTouch = true;
            lastBoardPosition = targetTrasnform.localPosition;
            lastBoardScale = Scale;
            var _touch0Position = touchNotifier.GetTouch(0);
            distanceBetweenTouches = Vector2.Distance(_currentPosition, _touch0Position);
            multiTouchMidPosition = (Vector2.Lerp(_currentPosition, _touch0Position, 0.5f) - (Vector2)targetTrasnform.localPosition) / Scale;
        }

        private void onMultiTouchMove(Vector2 _startPosition, Vector2 _currentPosition)
        {
            var _touch0Position = touchNotifier.GetTouch(0);
            var _distance = Vector2.Distance(_currentPosition, _touch0Position);
            Scale = lastBoardScale * _distance / distanceBetweenTouches;
            targetTrasnform.localScale = new Vector2(Scale, Scale);
            var _offset = _startPosition - _currentPosition;
            targetTrasnform.localPosition = lastBoardPosition - _offset + (lastBoardScale - Scale) * multiTouchMidPosition;
            
        }

        public void Init(){
            Scale = targetTrasnform.localScale.x;
            lastBoardPosition = Vector2.zero;
            distanceBetweenTouches = 0f;
            lastBoardScale = 1f;
            multiTouchMidPosition = Vector2.zero;
        }
        #endregion
    }
}