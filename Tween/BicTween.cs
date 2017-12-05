using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BicUtil.Tween
{	
	public static class BicTween {
		public static LTDescr scale(GameObject gameObject, Vector3 to, float time){
			#if UNITY_EDITOR
			if(Application.isPlaying){
				return LeanTween.scale(gameObject, to, time);
			}else{
				return LeanTweenOnEditor.scale(gameObject, to, time);
			}
			#else
			return LeanTween.scale(gameObject, to, time);
			#endif
		}

		public static LTDescr rotateZ(GameObject gameObject, float to, float time){
			#if UNITY_EDITOR
			if(Application.isPlaying){
				return LeanTween.rotateZ(gameObject, to, time);
			}else{
				return LeanTweenOnEditor.rotateZ(gameObject, to, time);
			}
			#else
			return LeanTween.rotateZ(gameObject, to, time);
			#endif
		}

		public static LTDescr delayedCall( float delayTime, Action callback){
			#if UNITY_EDITOR
			if(Application.isPlaying){
				return LeanTween.delayedCall(delayTime, callback);
			}else{
				return LeanTweenOnEditor.delayedCall(delayTime, callback);
			}
			#else
			return LeanTween.delayedCall(delayTime, callback);
			#endif
		}

		public static LTDescr moveLocal(GameObject gameObject, Vector3[] to, float time){
			#if UNITY_EDITOR
			if(Application.isPlaying){
				return LeanTween.moveLocal(gameObject, to, time);
			}else{
				return LeanTweenOnEditor.moveLocal(gameObject, to, time);
			}
			#else
			return LeanTween.moveLocal(gameObject, to, time);
			#endif
		}

		public static LTDescr moveLocal(GameObject gameObject, Vector3 to, float time){
			#if UNITY_EDITOR
			if(Application.isPlaying){
				return LeanTween.moveLocal(gameObject, to, time);
			}else{
				return LeanTweenOnEditor.moveLocal(gameObject, to, time);
			}
			#else
			return LeanTween.moveLocal(gameObject, to, time);
			#endif
		}

		public static void PlayMecanimAnimation(Animator _animator, string _stateHashName){
			#if UNITY_EDITOR
			if(Application.isPlaying){
				_animator.Play(_stateHashName);
			}else{
				_animator.Play(_stateHashName, -1, 0f);
				LeanTweenOnEditor.AddUpdateCallback((_id, _dt)=>{
					_animator.Update(_dt);

					if(_animator.isActiveAndEnabled == false || _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1){
						LeanTweenOnEditor.RemoveUpdateCallback(_id);
					}
			 	});
			}
			#else
			_animator.Play(_stateHashName);
			#endif
		}

		static public LTDescr moveSlider(UnityEngine.UI.Slider _slider, float _toValue, float _time){
			#if UNITY_EDITOR
			if(Application.isPlaying){
				return LeanTween.value(_slider.gameObject, _slider.value, _toValue, _time).setOnUpdate((float _result)=>{
					_slider.value = _result;
				});
			}else{
				return LeanTweenOnEditor.value(_slider.gameObject, _slider.value, _toValue, _time).setOnUpdate((float _result)=>{
					_slider.value = _result;
				});
			}
			#else
			return LeanTween.value(_slider.gameObject, _slider.value, _toValue, _time).setOnUpdate((float _result)=>{
					_slider.value = _result;
				});
			#endif
		}

		static public LTDescr levelUp(UnityEngine.UI.Slider _slider, float _toValue, float _time, float _nextMax, float _levelUpDelay){
			if(_toValue > _slider.maxValue && _nextMax > 0){
				//levelup
				return BicTween.moveSlider(_slider, _slider.maxValue, _time).setOnComplete(()=>{
					BicTween.delayedCall(_levelUpDelay, ()=>{
						_slider.minValue = _slider.maxValue;
						_slider.maxValue = _nextMax;
						BicTween.moveSlider(_slider, _toValue, 0.5f);
					});
				});


			}else{
				return BicTween.moveSlider(_slider, _toValue, _time);
			}
		}
	}

}
