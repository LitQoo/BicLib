using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.SingletonBase;
using BicDB.Variable;
using System.Linq;
using System;

namespace BicUtil.SkinSystem
{
	public class SkinManager : MonoBehaviour {
		#region static
		static private Dictionary<string, SkinController> skinControllers = new Dictionary<string, SkinController>();

		static public SkinController Get(string _groupName){
			if(!skinControllers.ContainsKey(_groupName)){
				skinControllers[_groupName] = new SkinController();	
			}

			return skinControllers[_groupName];
		}
		#endregion

		#region load
		public List<SkinConfig> SkinConfigList = new List<SkinConfig> ();

		private void Awake(){
			for (int i = 0; i < SkinConfigList.Count; i++) {
				var _skinConfig = SkinConfigList [i];
				Get (_skinConfig.GroupName).Init (_skinConfig);
			}
		}
		#endregion
	}

	[System.Serializable]
	public class SkinConfig{
		public string GroupName;
		public string SkinPath;
		public string SkinName;
		public SkinScriptableData SkinData;
	}
}
