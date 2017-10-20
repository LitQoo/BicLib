using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BicUtil.UIFlow
{
	static public class UIFlowAnimations {
		static public void OpenSwipeHorizontal(GameObject _layer, float _firstPosition = -600, float _animationTime = 0.3f){
			_layer.transform.localPosition = new Vector2 (_firstPosition, 0);
			LeanTween.moveLocalX (_layer, 0, _animationTime).setEaseInOutQuart ();
		}

		static public OnCloseUIResult CloseSwipeHorizontal(GameObject _layer, Action _finishCallback = null, float _firstPosition = -600, float _animationTime = 0.3f){
			_layer.transform.localPosition = new Vector2 (0, 0);
			LeanTween.moveLocalX (_layer, _firstPosition, _animationTime).setEaseInOutQuart ().setOnComplete (_finishCallback);

			return OnCloseUIResult.WaitForFinishCallbackAndFastDisplayNext;
		}
	}
}