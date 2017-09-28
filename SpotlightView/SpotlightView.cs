using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BicUtil.UnscaledTimeTween;

namespace BicUtil
{
	public class SpotlightView : MonoBehaviour {
		#region LinkingObject
		[SerializeField]
		private UnityEngine.UI.Image maskImage;
		[SerializeField]
		private UnityEngine.UI.Image backImage;
		[SerializeField]
		private UnityEngine.UI.Button button;
		#endregion

		#region LifeCycle
		private void Awake(){
			init ();
		}
		#endregion

		#region Interface
		public Action OnClickedButtonActions;


		public void SetMask(int _maskIndex, Vector2 _position, Vector2 _size){
			if (maskList.Count <= _maskIndex) {
				var _newMask = Instantiate<UnityEngine.UI.Image> (maskImage, maskImage.transform.parent);
				maskList.Add (_newMask);
			}

			maskList [_maskIndex].gameObject.SetActive (true);
			maskList [_maskIndex].transform.position = _position;
			maskList [_maskIndex].transform.localScale = _size;
		}

		public void DisableAllMask(){
			foreach (var _mask in maskList) {
				_mask.gameObject.SetActive (false);
			}
		}

		public void Close(float _animationTime = 0.5f, Action _callback = null){
			HideSpotAnimation ();
			SetButtonEnabled (false);
			CoroutineTween.Instance.Alpha (backImage, backImage.color.a, 0f, _animationTime, () => {
				gameObject.SetActive (false);
				_callback();
			});
		}

		public void Open(Action _callback, float _dimmedAlpha = 0.7f, float _animationTime = 0.5f){
			gameObject.SetActive (true);
			DisableAllMask ();
			SetButtonEnabled (false);

			CoroutineTween.Instance.Alpha (backImage, 0, _dimmedAlpha, _animationTime, _callback);
		}

		public void SetButtonEnabled(bool _isEnabled){
			button.gameObject.SetActive (_isEnabled);
		}

		public void ShowSpotAnimation(){
			StartCoroutine("showSpotAnimation");
		}

		public void HideSpotAnimation(Action _callback = null){
			StopCoroutine ("showSpotAnimation");
			StartCoroutine("hideSpotAnimation", _callback);
		}
		#endregion

		#region Event
		public void OnClickedButton(){
			SetButtonEnabled (false);
			OnClickedButtonActions ();
		}


		#endregion

		#region logic
		private List<UnityEngine.UI.Image> maskList = new List<UnityEngine.UI.Image>();
		private List<Vector2> sizeList = new List<Vector2>();
		private void init(){
			maskList.Add (maskImage);
		}

		private IEnumerator showSpotAnimation(){

			saveMaskScale ();

			for (int r = 0; r <= 110; r+=5) {
				setMaskScale (r / 100f);
				yield return new WaitForEndOfFrame ();
			}

			while (true) {
				for (int r = 30; r >= 0; r--) {
					setMaskScale ((r/3f + 100) / 100f);
					yield return new WaitForEndOfFrame ();
				}

				for (int r = 0; r <= 30; r++) {
					setMaskScale ((r/3f + 100) / 100f);
					yield return new WaitForEndOfFrame ();
				}
			}
		}

		private IEnumerator hideSpotAnimation(Action _callback){
			saveMaskScale ();

			for (int r = 100; r >= 0; r-=10) {
				setMaskScale (r / 100f);
				yield return new WaitForEndOfFrame ();
			}

			if (_callback != null) {
				_callback ();
			}
		}


		private void saveMaskScale(){
			sizeList.Clear ();
			for (int i = 0; i < maskList.Count; i++) {
				sizeList.Add(maskList [i].transform.localScale);
			}
		}

		private void setMaskScale(float _r){
			for (int i = 0; i < maskList.Count; i++) {
				maskList [i].transform.localScale = sizeList [i] * _r;
			}
		}
		#endregion
	}
}