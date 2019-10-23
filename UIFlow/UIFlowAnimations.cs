using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BicUtil.Tween;

namespace BicUtil.UIFlow
{
	static public class UIFlowAnimations {
		static public TweenModel OpenSwipeHorizontal(GameObject _layer, float _firstPosition = -600, float _animationTime = 0.3f){
			_layer.transform.localPosition = new Vector2 (_firstPosition, 0);
			return BicTween.MoveLocalX (_layer, 0, _animationTime).SetEase(EaseType.InOutQuart);
		}

		static public OnCloseUIResult CloseSwipeHorizontal(GameObject _layer, Action _finishCallback = null, float _firstPosition = -600, float _animationTime = 0.3f){
			_layer.transform.localPosition = new Vector2 (0, 0);
			BicTween.MoveLocalX (_layer, _firstPosition, _animationTime).SetEase(EaseType.InOutQuart).SubscribeComplete(_finishCallback);

			return OnCloseUIResult.WaitForFinishCallbackAndFastDisplayNext;
		}
		
		[Obsolete]
		static public TweenModel OpenSwipeVertical(GameObject _layer, float _firstPosition = -1200, float _animationTime = 0.3f, Action _finishCallback = null, EaseType _easyType = EaseType.InOutQuart){
			_layer.transform.localPosition = new Vector2 (0, _firstPosition);
			return BicTween.MoveLocalY (_layer, 0, _animationTime).SetEase(_easyType).SubscribeComplete(_finishCallback);
		}

		static public TweenModel OpenSwipeVertical(GameObject _layer, float _firstPosition = -1200, float _animationTime = 0.3f){
			_layer.transform.localPosition = new Vector2 (0, _firstPosition);
			return BicTween.MoveLocalY (_layer, 0, _animationTime);
		}

		static public OnCloseUIResult CloseSwipeVertical(GameObject _layer, Action _finishCallback = null, float _firstPosition = -1200, float _animationTime = 0.3f){
			_layer.transform.localPosition = new Vector2 (0, 0);
			BicTween.MoveLocalY (_layer, _firstPosition, _animationTime).SetEase(EaseType.InOutQuart).SubscribeComplete(_finishCallback);

			return OnCloseUIResult.WaitForFinishCallbackAndFastDisplayNext;
		}

		static public TweenModel OpenScaleDown(GameObject _layer, float _startScale = 1.3f, float _animationTime = 0.15f){
			_layer.transform.localScale = new Vector2(1.1f, 1.1f);
			return BicTween.Scale(_layer, new Vector2(1.1f, 1.1f), Vector2.one, _animationTime);
		}
	}
}