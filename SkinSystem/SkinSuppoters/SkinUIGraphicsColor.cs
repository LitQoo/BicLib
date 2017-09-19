using UnityEngine;

namespace BicUtil.SkinSystem
{
	public class SkinUIGraphicsColor : SkinBase {
		[SerializeField]
		private SkinUIGraphicInfo[] skinedObjects;

		protected override void applySkin(SkinConfig _config){
			for (int i = 0; i < skinedObjects.Length; i++) {
				skinedObjects[i].Graphic.color = _config.SkinData.GetColor (skinedObjects[i].PartName);
			}
		}
	}

	[System.Serializable]
	public class SkinUIGraphicInfo{
		public UnityEngine.UI.Graphic Graphic;
		public string PartName;
	}
}