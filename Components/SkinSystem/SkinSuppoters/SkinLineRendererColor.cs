using UnityEngine;

namespace BicUtil.Components.SkinSystem
{
	[RequireComponent(typeof(LineRenderer))]
	public class SkinLineRendererColor : SkinBase {
		[SerializeField]
		private LineRenderer lineRenderer;

		protected override void applySkin(SkinConfig _config){
			lineRenderer.startColor = _config.SkinData.GetColor (partsName);
			lineRenderer.endColor = _config.SkinData.GetColor (partsName);
		}
	}
}