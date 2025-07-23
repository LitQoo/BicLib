using UnityEngine;

namespace BicUtil.SkinSystem
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class SkinSprite : SkinBase {
		[SerializeField]
		private SpriteRenderer spriteRenderer;

		protected override void applySkin(SkinConfig _config){
			Sprite _sprite = Resources.Load<Sprite> (_config.SkinPath + "/" + _config.SkinName + "/" + partsName);
			spriteRenderer.sprite = _sprite;
		}
	}
}