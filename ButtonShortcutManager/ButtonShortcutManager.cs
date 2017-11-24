using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

				if(buttonShortcutInfo[i].isKeyUp == true){
					_keyFunc = Input.GetKeyDown;
				}

				if(_keyFunc(buttonShortcutInfo[i].KeyCode)){
					buttonShortcutInfo[i].Button.onClick.Invoke();
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