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

		public BackKeyEvent OnTouchBackKey;
		public TouchEvent OnTouchDown;
		public TouchEventWithStartingPosition OnTouchMove;
		public TouchEventWithStartingPosition OnTouchUp;
		private Vector2[] startTouchPosition = new Vector2[10]{Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero};

		private bool[] isTouchIn = new bool[10]{false, false, false, false, false,false, false, false, false, false};

		private void Update ()
        {
#if UNITY_EDITOR || UNITY_WEBGL || UNITY_STANDALONE || UNITY_FACEBOOK

            if (Input.GetMouseButtonDown(0))
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() == false)
                {
                    startTouchPosition[0] = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    OnTouchDown.Invoke(startTouchPosition[0], 0);

                    isTouchIn[0] = true;
                }
            }
            else if (Input.GetMouseButtonUp(0) && isTouchIn[0] == true)
            {
                Vector2 _position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                OnTouchUp.Invoke(startTouchPosition[0], _position, 0);
                isTouchIn[0] = false;
            }
            else if (isTouchIn[0] == true)
            {
                Vector2 _position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (startTouchPosition[0] != _position)
                {
                    OnTouchMove.Invoke(startTouchPosition[0], _position, 0);
                }
            }

            twoTouchScaleWithControlKey();
            twoTouchMoveWithShiftKey();

#else
			if(Input.touchCount > 0){
				for(int i = 0; i < Input.touchCount; i++){
					var _touch = Input.GetTouch(i);
					if(_touch.phase == TouchPhase.Began){
						if(UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject (i) == false){
							startTouchPosition[i] = Camera.main.ScreenToWorldPoint (_touch.position);
							OnTouchDown.Invoke (startTouchPosition[i], i);
							isTouchIn[i] = true;
						}
					} else if(_touch.phase == TouchPhase.Ended && isTouchIn[i] == true){
						Vector2 _position = Camera.main.ScreenToWorldPoint (_touch.position);
						OnTouchUp.Invoke(startTouchPosition[i], _position, i);
						isTouchIn[i] = false;
					} else if(isTouchIn[i] == true){
						Vector2 _position = Camera.main.ScreenToWorldPoint (_touch.position);
						if(_position != startTouchPosition[i]){
							OnTouchMove.Invoke(startTouchPosition[i], _position, i);
						}
					}
				}
			}
#endif

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                OnTouchBackKey.Invoke(Vector2.zero);
                EventNotifyer.EventNotifyer.Notify(this, BUTTON_ESC);
            }
        }

        private void twoTouchScaleWithControlKey()
        {
            if (isTouchIn[0] == true && Input.GetKey(KeyCode.LeftControl) == true && isTouchIn[1] == false)
            {
                isTouchIn[1] = true;
                startTouchPosition[1] = startTouchPosition[0] - new Vector2(100, 100);
                OnTouchDown.Invoke(startTouchPosition[1], 1);
            }
            else if ((isTouchIn[0] == false || Input.GetKey(KeyCode.LeftControl) == false) && isTouchIn[1] == true)
            {
                isTouchIn[1] = false;
                OnTouchUp.Invoke(startTouchPosition[1], startTouchPosition[1], 1);
            }
            else if (isTouchIn[1] == true)
            {
                OnTouchMove.Invoke(startTouchPosition[1], startTouchPosition[1], 1);
            }
        }

        private void twoTouchMoveWithShiftKey()
        {
            if (isTouchIn[0] == true && Input.GetKey(KeyCode.LeftShift) == true && isTouchIn[2] == false)
            {
                isTouchIn[2] = true;
                startTouchPosition[2] = startTouchPosition[0] - new Vector2(50, 50);
                OnTouchDown.Invoke(startTouchPosition[2], 1);
            }
            else if ((isTouchIn[0] == false || Input.GetKey(KeyCode.LeftShift) == false) && isTouchIn[2] == true)
            {
                isTouchIn[2] = false;
                var _position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - new Vector2(50, 50);
                OnTouchUp.Invoke(startTouchPosition[2], _position, 1);
            }
            else if (isTouchIn[2] == true)
            {
                var _position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - new Vector2(50, 50);
                OnTouchMove.Invoke(startTouchPosition[2], _position, 1);
            }
        }

        private void OnDestroy() {
			EventNotifyer.EventNotifyer.ClearSubscription(this);
		}

		public bool IsIn(RectTransform _rectTransform, Vector2 _worldPosition){
			return UnityEngine.RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, _worldPosition);
		}

		public Vector2 GetTouch(int _touchIndex){
			#if UNITY_EDITOR || UNITY_WEBGL || UNITY_STANDALONE || UNITY_FACEBOOK
			Vector2 _position = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			return _position;
			#else
			try{
				var _touch = Input.GetTouch(_touchIndex);
				return Camera.main.ScreenToWorldPoint (_touch.position);
			}catch{
				return startTouchPosition[_touchIndex];
			}
			#endif
		}

		public bool IsEnableTouch(int _touchIndex){
			return isTouchIn[_touchIndex];
		}

		public int TouchCount{
			get{
				return Input.touchCount;
			}
		}
		#endregion
	}
}