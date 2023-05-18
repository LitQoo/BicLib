using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using BicUtil.EventNotifyer;

namespace BicUtil.TouchNotifier
{
	public class TouchNotifier : MonoBehaviour {
		#region Event
		public const string BUTTON_ESC = "esc";
		
		[System.Serializable]
		public class BackKeyEvent : UnityEvent<Vector2> {}
		[System.Serializable]
		public class TouchEvent : UnityEvent<Vector2, int> {}
		[System.Serializable]
		public class TouchEventWithStartingPosition : UnityEvent<Vector2, Vector2, int>{}

		public BackKeyEvent OnTouchBackKey = new BackKeyEvent();
		public TouchEvent OnTouchDown = new TouchEvent();
		public TouchEventWithStartingPosition OnTouchMove = new TouchEventWithStartingPosition();
		public TouchEventWithStartingPosition OnTouchUp = new TouchEventWithStartingPosition();
		const int TOUCH_SIZE = 40;
        private Vector2[] startTouchPosition = new Vector2[TOUCH_SIZE]{Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero};
		private bool[] isTouchIn = new bool[TOUCH_SIZE]{false, false, false, false, false,false, false, false, false, false, false, false, false, false, false,false, false, false, false, false,false, false, false, false, false,false, false, false, false, false, false, false, false, false, false,false, false, false, false, false};
        public bool EnableUpdate{get;set;} = true;
		private void Update ()
        {
            if(EnableUpdate == false){
                return;
            }
            
            #if UNITY_EDITOR
            if(UnityEditor.EditorApplication.isRemoteConnected == true){
                touchProcess();
            }else{
                checkTouch(Input.GetMouseButton(0), 0);
                checkTouch(Input.GetMouseButton(1), 1);
                twoTouchScaleWithControlKey();
                twoTouchMoveWithShiftKey();
                updateCursor();
            }
            #elif UNITY_WEBGL || UNITY_STANDALONE || UNITY_FACEBOOK
            checkTouch(Input.GetMouseButton(0), 0);
			checkTouch(Input.GetMouseButton(1), 1);
			twoTouchScaleWithControlKey();
			twoTouchMoveWithShiftKey();
            #else
            touchProcess();
            #endif

            escProcess();
        }

        private void OnDestroy() {
			EventNotifyer.EventNotifyer.ClearSubscription(this);
		}
		#endregion

		#region Logic
		public bool IsIn(RectTransform _rectTransform, Vector2 _worldPosition){
			return UnityEngine.RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, _worldPosition);
		}

        public Vector2 GetStartPosition(int _touchIndex){
            return this.startTouchPosition[_touchIndex];
        }

        public bool GetTouchReturnForced{get;set;}=false;

        public void SetStartPositionForced(int _index, Vector2 _position){
            this.startTouchPosition[_index] = _position;
        }

		public Vector2 GetTouch(int _touchIndex){
            if(GetTouchReturnForced == true){
                return this.startTouchPosition[_touchIndex];
            }

			#if UNITY_EDITOR
                bool _useGetTouch = false;

                if(UnityEditor.EditorApplication.isRemoteConnected == true){
                    _useGetTouch = true;
                }
            #elif UNITY_WEBGL || UNITY_STANDALONE || UNITY_FACEBOOK
                bool _useGetTouch = false;
            #else
                bool _useGetTouch = true;
            #endif

            if(_useGetTouch == false){
                if(isTouchIn[1] == true){
                    return Camera.main.ScreenToWorldPoint (Input.mousePosition) - new Vector3(100, 100);
                }else{
                    Vector2 _position = Camera.main.ScreenToWorldPoint (Input.mousePosition);
                    return _position;
                }
            }else{
                try{
                    for(int i = 0; i < Input.touchCount; i++){
                        var _touch = Input.GetTouch(i);
                        if(_touch.fingerId == _touchIndex){
                            return Camera.main.ScreenToWorldPoint (_touch.position);
                        }
                    }

                    return startTouchPosition[_touchIndex];
                }catch{
                    return startTouchPosition[_touchIndex];
                }
            }
		}

		public bool IsEnableTouch(int _touchIndex){
			return isTouchIn[_touchIndex];
		}

        private void escProcess()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                OnTouchBackKey.Invoke(Vector2.zero);
                EventNotifyer.EventNotifyer.Notify(this, BUTTON_ESC);
            }
        }

		public int TouchCount{
			get{
			#if UNITY_EDITOR 
                if(UnityEditor.EditorApplication.isRemoteConnected == true){
                    return (isTouchIn[0]?1:0) + (isTouchIn[1]?1:0) + (isTouchIn[2]?1:0) + (isTouchIn[3]?1:0) + (isTouchIn[4]?1:0) + (isTouchIn[5]?1:0) + (isTouchIn[6]?1:0) + (isTouchIn[7]?1:0) + (isTouchIn[8]?1:0) + (isTouchIn[9]?1:0) + (isTouchIn[10]?1:0);
                }else{
                    return (isTouchIn[0]?1:0) + (isTouchIn[1]?2:0) + (isTouchIn[2]?2:0) + (isTouchIn[3]?1:0) + (isTouchIn[4]?1:0);
                }
            #elif UNITY_WEBGL || UNITY_STANDALONE || UNITY_FACEBOOK
				return (isTouchIn[0]?1:0) + (isTouchIn[1]?2:0) + (isTouchIn[2]?2:0) + (isTouchIn[3]?1:0) + (isTouchIn[4]?1:0);
			#else
                return (isTouchIn[0]?1:0) + (isTouchIn[1]?1:0) + (isTouchIn[2]?1:0) + (isTouchIn[3]?1:0) + (isTouchIn[4]?1:0) + (isTouchIn[5]?1:0) + (isTouchIn[6]?1:0) + (isTouchIn[7]?1:0) + (isTouchIn[8]?1:0) + (isTouchIn[9]?1:0) + (isTouchIn[10]?1:0);
			#endif
			}
		}
		#endregion

		#region Mobile 
        Action touchUpNotify = null;
        private void touchProcess()
        {
			int _touchCount = Input.touchCount;
            
            if(TOUCH_SIZE < _touchCount){
                return;
            }

            if(_touchCount <= 0){
                return;
            }
            
            _touchCount = Mathf.Min(_touchCount, 5);

            Touch _touch = default(Touch);
            touchUpNotify = null;
            int _errorCheck = 0; 
            try{
            for (int i = 0; i < _touchCount; i++)
            {
                _errorCheck = 1;
                _touch = Input.GetTouch(i);
                _errorCheck = 2;
                var _index = _touch.fingerId;
                _errorCheck = 3;
                if(_index >= TOUCH_SIZE){
                    continue;
                }
                
                _errorCheck = 4;
                if (_touch.phase == TouchPhase.Began)
                {
                    _errorCheck = 5;
                    notifyOnTouchDown(_touch.fingerId, _touch);
                }
                else if (_touch.phase == TouchPhase.Ended && isTouchIn[_index] == true)
                {
                    _errorCheck = 6;
                    isTouchIn[_index] = false;
                    int _i = _index;
                    _errorCheck = 7;
                    touchUpNotify += ()=>notifyOnTouchUp(_i, _touch);
                }
                else if (isTouchIn[_index] == true)
                {
                    _errorCheck = 8;
                    notifyOnTouchMove(_index, _touch);
                }
            }

            _errorCheck = 9;
            if(touchUpNotify != null){
                _errorCheck = 10;
                touchUpNotify();
            }

            }catch(System.Exception _e){
                Debug.Log("touchProcess error " + _touch.fingerId.ToString() + ", touch Count " + _touchCount.ToString() + ", touchinsize "+ isTouchIn.Length.ToString() + ", errorCheck = " + _errorCheck);
                throw _e;
            }
        }

        private void notifyOnTouchMove(int _index, Touch _touch)
        {
            Vector2 _position = Camera.main.ScreenToWorldPoint(_touch.position);
            if (_position != startTouchPosition[_index])
            {
                OnTouchMove.Invoke(startTouchPosition[_index], _position, _index);
            }
        }

        private void notifyOnTouchUp(int _index, Touch _touch)
        {
            Vector2 _position = Camera.main.ScreenToWorldPoint(_touch.position);
            OnTouchUp.Invoke(startTouchPosition[_index], _position, _index);
        }

        private void notifyOnTouchDown(int _index, Touch _touch)
        {
            var _line = 1;
            try{
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(_index) == false)
            {
                _line = 2;
                isTouchIn[_index] = true;
                _line = 3;
                startTouchPosition[_index] = Camera.main.ScreenToWorldPoint(_touch.position);
                _line = 4;
                OnTouchDown.Invoke(startTouchPosition[_index], _index);
            }
            _line = 5;
            }catch(System.Exception _e){
                Debug.Log("notifyOnTouchDown fingerid = " + _index + ", line = " + _line.ToString() + ", touchnisize " + isTouchIn.Length.ToString() + ", startTouchPosition size "+ startTouchPosition.Length.ToString());
                throw _e;
            }
        }

        public void SetTouchCountForced(int _count){
            for(int i = 0; i < isTouchIn.Length; i++){
                this.isTouchIn[i] = i < _count;
            }
        }
		#endregion

		#region StandAlone
		private void checkTouch(bool _touchResponse, int _touchIndex){
			

            if (_touchResponse == true && isTouchIn[_touchIndex] == false)
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() == false)
                {
					isTouchIn[_touchIndex] = true;
                    startTouchPosition[_touchIndex] = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    OnTouchDown.Invoke(startTouchPosition[_touchIndex], _touchIndex);

                }
            }
            else if (isTouchIn[_touchIndex] == true && _touchResponse == false)
            {
                Vector2 _position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                isTouchIn[_touchIndex] = false;
                OnTouchUp.Invoke(startTouchPosition[_touchIndex], _position, _touchIndex);
            }
            else if (isTouchIn[_touchIndex] == true)
            {
                Vector2 _position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (startTouchPosition[_touchIndex] != _position)
                {
                    OnTouchMove.Invoke(startTouchPosition[_touchIndex], _position, _touchIndex);
                }
            }
		}

        private void twoTouchScaleWithControlKey()
        {
            if (isTouchIn[0] == true && Input.GetKey(KeyCode.LeftControl) == true && isTouchIn[3] == false)
            {
                isTouchIn[3] = true;
                startTouchPosition[3] = startTouchPosition[0] - new Vector2(100, 100);
                OnTouchDown.Invoke(startTouchPosition[3], 1);
            }
            else if ((isTouchIn[0] == false || Input.GetKey(KeyCode.LeftControl) == false) && isTouchIn[3] == true)
            {
				isTouchIn[3] = false;
                OnTouchUp.Invoke(startTouchPosition[3], startTouchPosition[3], 1);
            }
            else if (isTouchIn[3] == true)
            {
                OnTouchMove.Invoke(startTouchPosition[3], startTouchPosition[3], 1);
            }
        }

        private void twoTouchMoveWithShiftKey()
        {
            if (isTouchIn[0] == true && Input.GetKey(KeyCode.LeftShift) == true && isTouchIn[4] == false)
            {
                isTouchIn[4] = true;
                startTouchPosition[4] = startTouchPosition[0] - new Vector2(50, 50);
                OnTouchDown.Invoke(startTouchPosition[4], 1);
            }
            else if ((isTouchIn[0] == false || Input.GetKey(KeyCode.LeftShift) == false) && isTouchIn[4] == true)
            {
                isTouchIn[4] = false;
                var _position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - new Vector2(50, 50);
                OnTouchUp.Invoke(startTouchPosition[4], _position, 1);
            }
            else if (isTouchIn[4] == true)
            {
                var _position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - new Vector2(50, 50);
                OnTouchMove.Invoke(startTouchPosition[4], _position, 1);
            }
        }
		#endregion

        #region Editor Touch Cursor
        [SerializeField]
        private Sprite cursorNormal;
        [SerializeField]
        private Sprite cursorDown;
        [SerializeField]
        private string cursorSortingLayer = "Top";
        [SerializeField]
        private float cursorScale = 1f;

        private SpriteRenderer cursor = null;

        private void createCursor(){
            if(cursor == null){
                var _gameobject = new GameObject();
                _gameobject.name = "cursor";
                _gameobject.transform.SetParent(this.transform);
                cursor = _gameobject.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
                cursor.sprite = cursorNormal;
                cursor.transform.localScale = Vector2.one * cursorScale;
                cursor.sortingLayerName = cursorSortingLayer;
                cursor.sortingOrder = 999999;
                cursor.gameObject.SetActive(true);
            }
        }

        private void updateCursor(){
            if(cursorNormal == null){
                return;
            }

            createCursor();

            this.cursor.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if(Input.GetMouseButtonDown(0) == true){
                this.cursor.sprite = cursorDown;
            }else if(Input.GetMouseButtonUp(0) == true){
                this.cursor.sprite = cursorNormal;
            }
        }
        #endregion
	}
}