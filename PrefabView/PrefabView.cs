using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.PrefabView{
	public abstract class PrefabView : MonoBehaviour {
		public abstract void ApplyValues();
		public abstract string PrefabViewName{get;}
		
		#if UNITY_EDITOR
		[System.NonSerialized]
		public bool isShowChildInHierarchy = false;
		#endif

		private void Awake() {
			this.ApplyValues();
		}
	}
}