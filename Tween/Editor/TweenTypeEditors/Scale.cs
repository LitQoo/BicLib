using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.CustomTimeLine;
using UnityEditor;
using UnityEngine;

namespace BicUtil.Tween
{
	public partial class BicTweenEditor {
		//[TweenEditorOnClickedNode(TweenType.?)]
		//private void onClicked?Node(TweenModel _tween, Event _event)

        [TweenEditorDrawNode(TweenType.Scale)]
        private Rect drawScaleNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
            return drawSingleNode(_tween, _startPosition, _timeline);
        }

		[TweenEditorDrawSetting(TweenType.Scale)]
		private void drawScaleSetting(TweenModel _tween){
			_tween.OriginValue = EditorGUILayout.Vector3Field("Origin Value", _tween.OriginValue);
			_tween.DiffValue = EditorGUILayout.Vector3Field("Diff Value", _tween.DiffValue);
		}

		[TweenEditorDrawHandleControl(TweenType.Scale)]
		private void drawScaleHandleControl(TweenModel _tween){
			if(_tween.TargetObject == null){
				return;
			}

			var _transform = _tween.TargetObject.GetComponent<RectTransform>();
			var _originSize = new Vector2(_transform.sizeDelta.x * (_tween.OriginValue.x), _transform.sizeDelta.y * (_tween.OriginValue.y));
			var _originOffset = new Vector2(_originSize.x * (_transform.pivot.x - 0.5f), _originSize.y * (_transform.pivot.y - 0.5f));
			var _originRect = new Rect(_transform.position.x - _transform.sizeDelta.x * (_tween.OriginValue.x) / 2f - _originOffset.x, _transform.position.y - _transform.sizeDelta.y * (_tween.OriginValue.y) / 2f - _originOffset.y, _originSize.x, _originSize.y);
			Handles.DrawSolidRectangleWithOutline(_originRect, Color.clear, Color.magenta);
			drawMoveHandle(new Vector2(_originRect.x + _originRect.width, _originRect.y + _originRect.height), 0, Color.magenta, 10, _tween, setScalePosition);
			
			var _targetValue = _tween.OriginValue + _tween.DiffValue;
			var _diffSize = new Vector2(_transform.sizeDelta.x * (_targetValue.x), _transform.sizeDelta.y * (_targetValue.y));
			var _diffOffset = new Vector2(_diffSize.x * (_transform.pivot.x - 0.5f), _diffSize.y * (_transform.pivot.y - 0.5f));
			var _diffRect = new Rect(_transform.position.x - _transform.sizeDelta.x * (_targetValue.x) / 2f - _diffOffset.x, _transform.position.y - _transform.sizeDelta.y * (_targetValue.y) / 2f - _diffOffset.y, _diffSize.x, _diffSize.y);
			Handles.DrawSolidRectangleWithOutline(_diffRect, Color.clear, Color.cyan);
			drawMoveHandle(new Vector2(_diffRect.x + _diffRect.width, _diffRect.y + _diffRect.height), 1, Color.cyan, 10, _tween, setScalePosition);
		}

        private void setScalePosition(TweenModel _tween, int _index, Vector3 _position)
        {
			var _transform = _tween.TargetObject.GetComponent<RectTransform>();
           	if(_index == 1){
				var _diffX = (_position.x -  _transform.position.x) / (_transform.sizeDelta.x / 2f) - _tween.OriginValue.x;
				var _diffY = (_position.y -  _transform.position.y) / (_transform.sizeDelta.y / 2f) - _tween.OriginValue.y;
				_tween.DiffValue = new Vector2(_diffX, _diffY);
			}else{
				var _originX = (_position.x - _transform.position.x) / (_transform.sizeDelta.x / 2f);
				var _originY = (_position.y - _transform.position.y) / (_transform.sizeDelta.y / 2f);
				var _targetValue = _tween.OriginValue + _tween.DiffValue;
				_tween.OriginValue = new Vector2(_originX, _originY);
				_tween.DiffValue = new Vector2(_targetValue.x - _originX, _targetValue.y - _originY);
			}
        }
    }
}
