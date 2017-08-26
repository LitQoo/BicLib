using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace BicUtil.Components.TouchNotifier
{
	public class TouchNotifier : MonoBehaviour {
		#region Event
		[System.Serializable]
		public class TouchEvent : UnityEvent <Vector2> {}

		public TouchEvent OnTouchDown;
		public TouchEvent OnTouchMove;
		public TouchEvent OnTouchUp;

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
		}
		#endregion
	}
}