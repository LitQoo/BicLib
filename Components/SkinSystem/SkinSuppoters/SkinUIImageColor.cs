using UnityEngine;

namespace BicUtil.Components.SkinSystem
{
	[RequireComponent(typeof(UnityEngine.UI.Image))]
	public class SkinUIImageColor : SkinBase {
		[SerializeField]
		private UnityEngine.UI.Image image;

		protected override void applySkin(SkinConfig _config){
			image.color = _config.SkinData.GetColor (partsName);
		}
	}
}