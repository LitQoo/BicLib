using UnityEngine;

namespace BicUtil.SkinSystem
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class SkinSpriteColor : SkinBase {
		[SerializeField]
		private SpriteRenderer spriteRenderer;

		protected override void applySkin(SkinConfig _config){
			spriteRenderer.color = _config.SkinData.GetColor (partsName);
		}
	}
}