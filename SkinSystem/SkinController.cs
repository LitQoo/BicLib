using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.SingletonBase;
using BicDB.Variable;
using System.Linq;
using System;

namespace BicUtil.SkinSystem
{
	public class SkinController{
		private string applyedSkinName;
		public SkinConfig Config { get; private set; }
		private List<SkinBase> SkinObjetList = new List<SkinBase> ();
		public void Init(SkinConfig _config){
			Config = _config;
			Load (Config.SkinName);
		}

		public void Load(string _skinName){
			if (applyedSkinName == _skinName) {
				return;
			}

			applyedSkinName = _skinName;
			Config.SkinName = applyedSkinName;

			loadSkinData ();

			for (int i = 0; i < SkinObjetList.Count; i++) {
				SkinObjetList [i].ApplySkinAndSaveLastConfig (Config);
			}
		}

		public void AddSkinObject(SkinBase _skinObject){
			SkinObjetList.Add (_skinObject);
			_skinObject.ApplySkinAndSaveLastConfig (Config);
		}

		public void RemoveSkinObject(SkinBase _skinObject){
			SkinObjetList.Remove (_skinObject);
		}

		private void loadSkinData(){
			Config.SkinData = Resources.Load<SkinScriptableData> (Config.SkinPath + "/" + Config.SkinName + "/" + "SkinData");
		}
	}
}