using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.SkinSystem
{
	public abstract class SkinBase : MonoBehaviour{
		[SerializeField]
		protected string groupName;
		[SerializeField]
		protected string partsName;
		[SerializeField]
		private bool isAutoInitSkin = true;
		protected SkinConfig lastConfig;
		private bool isInit = false;

		protected void Start () {
			if (isAutoInitSkin == true) {
				initSkin ();
			}
		}

		private void initSkin(){
			if (isInit == false) {
				SkinManager.Get (groupName).AddSkinObject (this);
				isInit = true;
			}
		}

		public void ApplySkinAndSaveLastConfig (SkinConfig _config)
		{	
			applySkin(_config);
			lastConfig = _config;
		}

		abstract protected void applySkin (SkinConfig _config);

		public void SetPartsName(string _partsName){
			initSkin ();
			partsName = _partsName;
			applySkin (lastConfig);
		}

		private void OnDestory(){
			SkinManager.Get(groupName).RemoveSkinObject (this);
		}
	}
}
