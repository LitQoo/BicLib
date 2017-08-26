using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.Components.SkinSystem
{
	[RequireComponent(typeof(UnityEngine.UI.Text))]
	public class SkinUIText : SkinBase {
		[SerializeField]
		private UnityEngine.UI.Text text;

		protected override void applySkin(SkinConfig _config){
			text.color = _config.SkinData.GetColor (partsName);
		}
	}
}