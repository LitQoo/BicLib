using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BicUtil.UnscaledTimeTween
{
	public class CoroutineTween : SingletonBase.MonoBehaviourHardBase<CoroutineTween> {
		#region Alpha
		public void Alpha (UnityEngine.UI.Image _image, float _startAlpha, float _endAlpha, float _time, Action _callback){
			StartCoroutine("alphaAnimationCoroutine", new AlphaAnimationParam (_image, _startAlpha, _endAlpha, _time, _callback));
		}
			
		private struct AlphaAnimationParam{
			public UnityEngine.UI.Image Image;
			public float StartAlpha;
			public float EndAlpha;
			public float Time;
			public Action Callback;

			public AlphaAnimationParam(UnityEngine.UI.Image _image, float _startAlpha, float _endAlpha, float _time, Action _callback){
				Image = _image;
				StartAlpha = _startAlpha;
				EndAlpha = _endAlpha;
				Time = _time;
				Callback = _callback;
			}
		}

		private IEnumerator alphaAnimationCoroutine(AlphaAnimationParam _param){
			float _alpha = _param.StartAlpha;
			_param.Image.color = new Color (_param.Image.color.r, _param.Image.color.g, _param.Image.color.b, _alpha);

			while (_alpha != _param.EndAlpha) {
				_alpha = Mathf.MoveTowards(_alpha, _param.EndAlpha, Time.unscaledDeltaTime*(1/_param.Time));
				_param.Image.color = new Color (_param.Image.color.r, _param.Image.color.g, _param.Image.color.b, _alpha);
				yield return new WaitForEndOfFrame ();
			}

			if (_param.Callback != null) {
				_param.Callback ();
			}
		}
		#endregion
	}
}