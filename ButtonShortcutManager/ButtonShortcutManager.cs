using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BicUtil.ButtonShortcutManager
{
	public class ButtonShortcutManager : MonoBehaviour {
		#region Data
		[SerializeField]
		private ButtonShortcutInfo[] buttonShortcutInfo;
		#endregion

		#region LifeCycle
		#if UNITY_EDITOR || UNITY_STANDALONE
		private void Update(){
			checkShortcut();
		}
		#endif
		#endregion

		#region Logic
		private void checkShortcut(){
			for(int i = 0; i < buttonShortcutInfo.Length; i++){
				Func<KeyCode, bool> _keyFunc = Input.GetKeyDown;
				var _info = buttonShortcutInfo[i];
				if(_info.isKeyUp == true){
					_keyFunc = Input.GetKeyUp;
				}

				if(_keyFunc(_info.KeyCode)){
					if(_info.Button.IsInteractable()){
						List<RaycastResult> _results = new List<RaycastResult>();
						PointerEventData _eventData = new PointerEventData(EventSystem.current);
						_eventData.position = Camera.main.WorldToScreenPoint(_info.Button.transform.position);
            			EventSystem.current.RaycastAll(_eventData, _results);
						
						if (_results.Count !=0 && ((_results[0].gameObject == _info.Button.gameObject) || (_results[0].gameObject.transform.parent == _info.Button.gameObject.transform))){ 
							_info.Button.onClick.Invoke();
						}
					}
				}
			}
		}
		#endregion
	}

	[System.Serializable]
	public class ButtonShortcutInfo{
		public UnityEngine.UI.Button Button;
		public KeyCode KeyCode;
		public bool isKeyUp = true;
	}
}