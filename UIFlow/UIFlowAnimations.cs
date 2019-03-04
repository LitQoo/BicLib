using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BicUtil.Tween;

namespace BicUtil.UIFlow
{
	static public class UIFlowAnimations {
		static public void OpenSwipeHorizontal(GameObject _layer, float _firstPosition = -600, float _animationTime = 0.3f, Action _finishCallback = null){
			_layer.transform.localPosition = new Vector2 (_firstPosition, 0);
			BicTween.MoveLocalX (_layer, 0, _animationTime).SetEase(EaseType.InOutQuart).SubscribeComplete(_finishCallback);
		}

		static public OnCloseUIResult CloseSwipeHorizontal(GameObject _layer, Action _finishCallback = null, float _firstPosition = -600, float _animationTime = 0.3f){
			_layer.transform.localPosition = new Vector2 (0, 0);
			BicTween.MoveLocalX (_layer, _firstPosition, _animationTime).SetEase(EaseType.InOutQuart).SubscribeComplete(_finishCallback);

			return OnCloseUIResult.WaitForFinishCallbackAndFastDisplayNext;
		}
		
		static public void OpenSwipeVertical(GameObject _layer, float _firstPosition = -1200, float _animationTime = 0.3f, Action _finishCallback = null){
			_layer.transform.localPosition = new Vector2 (0, _firstPosition);
			BicTween.MoveLocalY (_layer, 0, _animationTime).SetEase(EaseType.InOutQuart).SubscribeComplete(_finishCallback);
		}

		static public OnCloseUIResult CloseSwipeVertical(GameObject _layer, Action _finishCallback = null, float _firstPosition = -1200, float _animationTime = 0.3f){
			_layer.transform.localPosition = new Vector2 (0, 0);
			BicTween.MoveLocalY (_layer, _firstPosition, _animationTime).SetEase(EaseType.InOutQuart).SubscribeComplete(_finishCallback);

			return OnCloseUIResult.WaitForFinishCallbackAndFastDisplayNext;
		}
	}
}