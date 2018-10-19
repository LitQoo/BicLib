using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.CameraScaler
{
	[ExecuteInEditMode]
	public class CameraScaler : MonoBehaviour {
		#region LinkingObject
		[SerializeField]
		private Camera mainCamera;
		[SerializeField]
		private RectTransform referenceTransform;
		[SerializeField]
		private Vector2 referenceResolution;
		[SerializeField]
		private VerticalAlign verticalAlign = VerticalAlign.Center;
		[SerializeField]
		private HorizonalAlign horizonalAlign = HorizonalAlign.Center;
		[SerializeField]
		private RectTransform manageFullSizeRect;
		#endregion

		#region LifeCycle
		private void Start(){
			init ();
		}
		#endregion

		#region Logic
		private void init(){
			if(referenceTransform == manageFullSizeRect){
				Debug.LogWarning("[CameraScaler] Set referenceTransform != manageFullSizeRect");
				return;
			}

			if (referenceTransform != null) {
				referenceResolution = referenceTransform.sizeDelta;
			}

			float _referenceRate = referenceResolution.x / referenceResolution.y;
			float _screenRate = (float)Screen.width / (float)Screen.height;

			if (_referenceRate < _screenRate) {
				mainCamera.orthographicSize = referenceResolution.y / 2f;

				float _xOffset = 0;
				switch (horizonalAlign) {
				case HorizonalAlign.Right:
					_xOffset = ((float)Screen.width - referenceResolution.x * (float)Screen.height / referenceResolution.y) / 2f;
					break; 
				case HorizonalAlign.Left:
					_xOffset = ((float)Screen.width - referenceResolution.x * (float)Screen.height / referenceResolution.y) / 2f * -1;
					break; 
				}

				mainCamera.transform.position = new Vector3 (_xOffset, 0, -10);


				if(manageFullSizeRect != null){
					float _rate = referenceResolution.y / (float)Screen.height;
					manageFullSizeRect.sizeDelta = new Vector3((float)Screen.width * _rate, (float)Screen.height * _rate);
				}
			} else if(_referenceRate > _screenRate){
				float _rate = referenceResolution.x / (float)Screen.width;
				float _hSize = (float)Screen.height * _rate;
				mainCamera.orthographicSize = _hSize / 2f;


				float _yOffset = 0;
				switch (verticalAlign) {
				case VerticalAlign.Top:
					_yOffset = (_hSize - referenceResolution.y) / 2f * -1;
					break; 
				case VerticalAlign.Bottom:
					_yOffset = (_hSize - referenceResolution.y) / 2f;
					break; 
				}

				mainCamera.transform.position = new Vector3 (0, _yOffset, -10);

				if(manageFullSizeRect != null){
					manageFullSizeRect.sizeDelta = new Vector3((float)Screen.width * _rate, (float)Screen.height * _rate);
				}
			}
		}
		#endregion

		#if UNITY_EDITOR
		private void Update(){
			init ();
		}
		#endif
	}

	public enum VerticalAlign
	{
		Top,
		Center,
		Bottom
	}

	public enum HorizonalAlign
	{
		Left,
		Center,
		Right
	}
}