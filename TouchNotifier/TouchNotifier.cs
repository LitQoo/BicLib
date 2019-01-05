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
		public class TouchEvent : UnityEvent<Vector2> {}
		[System.Serializable]
		public class TouchEventWithStartingPosition : UnityEvent<Vector2, Vector2>{}

		public TouchEvent OnTouchDown;
		public TouchEvent OnTouchMove;
		public TouchEvent OnTouchUp;
		public TouchEvent OnTouchBackKey;
		
		public TouchEventWithStartingPosition OnTouchMoveWithStartingPosition;
		public TouchEventWithStartingPosition OnTouchUpWithStartingPosition;
		private Vector2[] startTouchPosition = new Vector2[5];

		private bool isTouchIn = false;

		private void Update () {
			#if UNITY_EDITOR
			if (Input.GetMouseButtonDown (0)) {
				if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject () == false) {
					startTouchPosition[0] = Camera.main.ScreenToWorldPoint (Input.mousePosition);
					OnTouchDown.Invoke (startTouchPosition[0]);
					
					isTouchIn = true;
				}
			} else if (Input.GetMouseButtonUp (0) && isTouchIn == true) {
				Vector2 _position = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				OnTouchUp.Invoke (_position);
				OnTouchUpWithStartingPosition.Invoke (startTouchPosition[0], _position);

				isTouchIn = false;
			} else if (isTouchIn == true) {
				Vector2 _position = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				OnTouchMove.Invoke (_position);
				OnTouchMoveWithStartingPosition.Invoke(startTouchPosition[0], _position);
			}

			#else
			if(Input.touchCount > 0){
				for(int i = 0; i < Input.touchCount; i++){
					if(Input.GetTouch(i).phase == TouchPhase.Began){
						if(UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject (i) == false){
							startTouchPosition[i] = Camera.main.ScreenToWorldPoint (Input.GetTouch(i).position);
							OnTouchDown.Invoke (startTouchPosition[i]);
							isTouchIn = true;
						}
					} else if(Input.GetTouch(i).phase == TouchPhase.Ended && isTouchIn == true){
						Vector2 _position = Camera.main.ScreenToWorldPoint (Input.GetTouch(i).position);
						OnTouchUp.Invoke (_position);
						OnTouchUpWithStartingPosition.Invoke(startTouchPosition[i], _position);
						isTouchIn = false;
					} else if(isTouchIn == true){
						Vector2 _position = Camera.main.ScreenToWorldPoint (Input.GetTouch(i).position);
						OnTouchMove.Invoke (_position);
						OnTouchMoveWithStartingPosition.Invoke(startTouchPosition[i], _position);
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