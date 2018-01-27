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

		public TouchEvent OnTouchDown;
		public TouchEvent OnTouchMove;
		public TouchEvent OnTouchUp;
		public TouchEvent OnTouchBackKey;
		
		private bool isTouchIn = false;

		private void Update () {
			#if UNITY_EDITOR
			if (Input.GetMouseButtonDown (0)) {
				if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject () == false) {
					OnTouchDown.Invoke (Camera.main.ScreenToWorldPoint (Input.mousePosition));
					isTouchIn = true;
				}
			} else if (Input.GetMouseButtonUp (0) && isTouchIn == true) {
				OnTouchUp.Invoke (Camera.main.ScreenToWorldPoint (Input.mousePosition));
				isTouchIn = false;
			} else if (isTouchIn == true) {
				OnTouchMove.Invoke (Camera.main.ScreenToWorldPoint (Input.mousePosition));
			}
			#else
			if(Input.touchCount > 0){
				for(int i = 0; i < Input.touchCount; i++){
					if(Input.GetTouch(i).phase == TouchPhase.Began){
						if(UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject (i) == false){
							OnTouchDown.Invoke (Camera.main.ScreenToWorldPoint (Input.mousePosition));
							isTouchIn = true;
						}
					} else if(Input.GetTouch(i).phase == TouchPhase.Ended && isTouchIn == true){
						OnTouchUp.Invoke (Camera.main.ScreenToWorldPoint (Input.mousePosition));
						isTouchIn = false;
					} else if(isTouchIn == true){
						OnTouchMove.Invoke (Camera.main.ScreenToWorldPoint (Input.mousePosition));
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