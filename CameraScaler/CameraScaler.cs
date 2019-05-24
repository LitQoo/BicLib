using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.CameraScaler
{
	[ExecuteInEditMode]
	public class CameraScaler : MonoBehaviour {
		#region static
		static public CameraScaler Instance;
		#endregion
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
			Instance = this;
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

			Vector2 calcresolution = referenceResolution + manageFullSizeOffset;
			float _referenceRate = calcresolution.x / calcresolution.y;
			float _screenRate = (float)Screen.width / (float)Screen.height;

			//설정보다 뚱뚱할때
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

				float _yOffset = 0;
				if(manageFullSizeOffset.y != 0){
					switch (verticalAlign) {
						case VerticalAlign.Top:
						_yOffset = manageFullSizeOffset.y / 2f;
						break;
						case VerticalAlign.Bottom:
						_yOffset = - manageFullSizeOffset.y / 2f;
						break;
					}
				}

				mainCamera.transform.position = new Vector3 (_xOffset, _yOffset, -10);

				if(manageFullSizeRect != null){
					float _rate = referenceResolution.y / (float)Screen.height;
					manageFullSizeRect.sizeDelta = new Vector2((float)Screen.safeArea.width * _rate, (float)Screen.safeArea.height * _rate) + manageFullSizeOffset;
				}

			//설정보다 길쭉할때
			} else {
				float _rate = referenceResolution.x / (float)Screen.width;
				float _hSize = (float)Screen.height * _rate;
				mainCamera.orthographicSize = _hSize / 2f;


				float _yOffset = 0;
				switch (verticalAlign) {
				case VerticalAlign.Top:
					if(manageFullSizeOffset.y != 0){
						_yOffset = manageFullSizeOffset.y / 2f;
					}else{
						_yOffset = (_hSize - referenceResolution.y) / 2f * -1;
					}
					break; 
				case VerticalAlign.Bottom:
					if(manageFullSizeOffset.y != 0){
						_yOffset = - manageFullSizeOffset.y / 2f;
					}else{
						_yOffset = (_hSize - referenceResolution.y) / 2f;
					}
					break; 
				}

				mainCamera.transform.position = new Vector3 (0, _yOffset, -10);

				if(manageFullSizeRect != null){
					var _rectRate = referenceResolution.x / (float)Screen.width;
					manageFullSizeRect.sizeDelta = new Vector2((float)Screen.safeArea.width * _rectRate, (float)Screen.safeArea.height * _rectRate) + manageFullSizeOffset;
				}
			}

			#if UNITY_EDITOR
			string _log = "[CameraScaler] Screen size change detected. " + Screen.safeArea.size.ToString();
			_log += "\nmainCamera position to " + mainCamera.transform.position.ToString();
			if(manageFullSizeRect != null){
				_log += "\nmanageFullSizeRect to " + manageFullSizeRect.sizeDelta.ToString();
				_log += "\noffset to " + manageFullSizeOffset.ToString();
			}

			Debug.LogWarning(_log);
			#endif
		}
		
		private Vector2 manageFullSizeOffset = Vector2.zero;

		public void SetManageFullSizeOffset(Vector2 _offset){
			manageFullSizeOffset = _offset;
			init();
		}
		#endregion

		#if UNITY_EDITOR
		Rect screenSize = Rect.zero;

		[ExecuteInEditMode]
		private void Update(){
			if(screenSize.Equals(Screen.safeArea) == false){
				screenSize = Screen.safeArea;
				init();
				return;
			}
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