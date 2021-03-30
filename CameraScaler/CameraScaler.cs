using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BicUtil.CameraScaler
{
	[ExecuteInEditMode]
	public class CameraScaler : MonoBehaviour {
		#region static
		static public CameraScaler Instance;
		#endregion
		#region LinkingObject
		[SerializeField]
		private Camera mainCamera = null;
		[SerializeField]
		private RectTransform referenceTransform = null;
		[SerializeField]
		private Vector2 referenceResolution = Vector2.zero;
		[SerializeField]
		private VerticalAlign verticalAlign = VerticalAlign.Center;
		[SerializeField]
		private HorizonalAlign horizonalAlign = HorizonalAlign.Center;
		[SerializeField]
		private RectTransform[] manageFullSizeRect = null;
		#endregion

		#region Instant
		public Vector2 ReferenceResolution{
			get{
				return this.referenceResolution;
			}
		}

		public Vector2 FullResolution{
			get{
				return this.manageFullSizeRect[0].sizeDelta;
			}
		}

		private bool isInit = false;
		#endregion

		#region LifeCycle
		private void Awake(){
			Instance = this;
			init ();
		}
		#endregion

		#region Logic
		private void init(bool _isForcedInit = false){
			if(isInit == true && _isForcedInit == false){
				return;
			}

			isInit = true;

			if(manageFullSizeRect.Length > 0 && referenceTransform == manageFullSizeRect[0]){
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

				if(manageFullSizeRect.Length > 0){
					float _rate = referenceResolution.y / (float)Screen.height;
					for(int i = 0; i < manageFullSizeRect.Length; i++){
						manageFullSizeRect[i].sizeDelta = new Vector2((float)Screen.safeArea.width * _rate, (float)Screen.safeArea.height * _rate) + manageFullSizeOffset;
					}
				}

			//설정보다 길쭉할때
			} else {
				float _rate = referenceResolution.x / (float)Screen.width;
				float _hSize = (float)Screen.height * _rate;
				mainCamera.orthographicSize = _hSize / 2f;

				if(manageFullSizeRect.Length > 0){
					var _rectRate = referenceResolution.x / (float)Screen.width;

					for(int i = 0; i < manageFullSizeRect.Length; i++){
						manageFullSizeRect[i].sizeDelta = new Vector2((float)Screen.safeArea.width * _rectRate, (float)Screen.safeArea.height * _rectRate) + manageFullSizeOffset;
					}

					float _yOffset = 0;
					switch (verticalAlign) {
					case VerticalAlign.Top:
						_yOffset = manageFullSizeOffset.y / 2f;
						break; 
					case VerticalAlign.Bottom:
						_yOffset = - manageFullSizeOffset.y / 2f;
						break; 
					}

					mainCamera.transform.position = new Vector3 (0, _yOffset, -10);
				}else{

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
				}
			}

			#if UNITY_EDITOR
			string _log = "[CameraScaler] Screen size change detected. " + Screen.safeArea.size.ToString();
			_log += "\nmainCamera position to " + mainCamera.transform.position.ToString();
			if(manageFullSizeRect.Length > 0){
				_log += "\nmanageFullSizeRect to " + manageFullSizeRect[0].sizeDelta.ToString();
				_log += "\noffset to " + manageFullSizeOffset.ToString();
			}

			Debug.LogWarning(_log);
			#endif
		}
		
		[SerializeField]
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
				init(true);
				if(onChangedScreenSize != null){
					onChangedScreenSize();
				}
				return;
			}
		}
		#endif

		private Action onChangedScreenSize = null;

		public void SubscribeChangedScreenSize(Action _action){
			this.onChangedScreenSize += _action;
		}
		
		public void UnsubscribeChangedScreenSize(Action _action){
			this.onChangedScreenSize -= _action;
		}

		public void SetReferenceResolution(Vector2 _size){
			this.referenceResolution = _size;
			init(true);
		}
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