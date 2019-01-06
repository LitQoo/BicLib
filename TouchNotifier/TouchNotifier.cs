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
		private Vector2[] startTouchPosition = new Vector2[5];

		private bool[] isTouchIn = new bool[5]{false, false, false, false, false};

		private void Update () {
			#if UNITY_EDITOR
			if (Input.GetMouseButtonDown (0)) {
				if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject () == false) {
					startTouchPosition[0] = Camera.main.ScreenToWorldPoint (Input.mousePosition);
					OnTouchDown.Invoke (startTouchPosition[0], 0);

					isTouchIn[0] = true;
				}
			} else if (Input.GetMouseButtonUp (0) && isTouchIn[0] == true) {
				Vector2 _position = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				OnTouchUp.Invoke (startTouchPosition[0], _position, 0);
				isTouchIn[0] = false;
			} else if (isTouchIn[0] == true) {
				Vector2 _position = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				OnTouchMove.Invoke (startTouchPosition[0],_position, 0);
			}

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
						OnTouchMove.Invoke(startTouchPosition[i], _position, i);
					}
				}
			}
			#endif

			if(Input.GetKeyUp(KeyCode.Escape))
			{
				OnTouchBackKey.Invoke(Vector2.zero);
				EventNotifyer.EventNotifyer.Notify(this, BUTTON_ESC);
			}
		}

		private void OnDestroy() {
			EventNotifyer.EventNotifyer.ClearSubscription(this);
		}
		#endregion
	}
}