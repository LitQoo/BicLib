using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BicUtil.Tween
{	
	public static class BicTween {
		public static LTDescr scale(GameObject gameObject, Vector3 to, float time){
			if(Application.isPlaying){
				return LeanTween.scale(gameObject, to, time);
			}else{
				return LeanTweenOnEditor.scale(gameObject, to, time);
			}
		}

		public static LTDescr rotateZ(GameObject gameObject, float to, float time){
			if(Application.isPlaying){
				return LeanTween.rotateZ(gameObject, to, time);
			}else{
				return LeanTweenOnEditor.rotateZ(gameObject, to, time);
			}
		}

		public static LTDescr delayedCall( float delayTime, Action callback){
			if(Application.isPlaying){
				return LeanTween.delayedCall(delayTime, callback);
			}else{
				return LeanTweenOnEditor.delayedCall(delayTime, callback);
			}
		}

		public static LTDescr moveLocal(GameObject gameObject, Vector3[] to, float time){
			if(Application.isPlaying){
				return LeanTween.moveLocal(gameObject, to, time);
			}else{
				return LeanTweenOnEditor.moveLocal(gameObject, to, time);
			}
		}

		public static LTDescr moveLocal(GameObject gameObject, Vector3 to, float time){
			if(Application.isPlaying){
				return LeanTween.moveLocal(gameObject, to, time);
			}else{
				return LeanTweenOnEditor.moveLocal(gameObject, to, time);
			}
		}

		public static void PlayMecanimAnimation(Animator _animator, string _stateHashName){
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
		}
	}
}
