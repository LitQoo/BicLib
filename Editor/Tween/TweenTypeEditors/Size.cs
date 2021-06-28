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
		//private void onClicked?Node(Tween _tween, Event _event)

        [TweenEditorDrawNode(TweenType.Size)]
        private Rect drawSizeNode(Tween _tween, Vector2 _startPosition, Timeline _timeline){
            return drawSingleNode(_tween, _startPosition, _timeline);
        }

		[TweenEditorDrawSetting(TweenType.Size)]
		private void drawSizeSetting(Tween _tween){
			_tween.OriginValue = EditorGUILayout.Vector3Field("Origin Value", _tween.OriginValue);
			_tween.DiffValue = EditorGUILayout.Vector3Field("Diff Value", _tween.DiffValue);
		}

		[TweenEditorDrawHandleControl(TweenType.Size)]
		private void drawSizeHandleControl(Tween _tween){
			if(_tween.TargetObject == null){
				return;
			}

			var _transform = _tween.TargetObject.GetComponent<RectTransform>();
			var _originOffset = new Vector2(_tween.OriginValue.x * (_transform.pivot.x - 0.5f), _tween.OriginValue.y * (_transform.pivot.y - 0.5f));
			var _originRect = new Rect(_transform.position.x - _tween.OriginValue.x / 2f - _originOffset.x, _transform.position.y - _tween.OriginValue.y / 2f - _originOffset.y, _tween.OriginValue.x, _tween.OriginValue.y);
			Handles.DrawSolidRectangleWithOutline(_originRect, Color.clear, Color.magenta);
			drawMoveHandle(new Vector2(_originRect.x + _originRect.width, _originRect.y + _originRect.height), 0, Color.magenta, 10, _tween, setSizePosition);
			
			var _targetValue = _tween.OriginValue + _tween.DiffValue;
			var _diffOffset = new Vector2(_targetValue.x * (_transform.pivot.x - 0.5f), _targetValue.y * (_transform.pivot.y - 0.5f));
			var _diffRect = new Rect(_transform.position.x - _targetValue.x / 2f - _diffOffset.x, _transform.position.y - _targetValue.y / 2f - _diffOffset.y, _targetValue.x, _targetValue.y);
			Handles.DrawSolidRectangleWithOutline(_diffRect, Color.clear, Color.cyan);
			drawMoveHandle(new Vector2(_diffRect.x + _diffRect.width, _diffRect.y + _diffRect.height), 1, Color.cyan, 10, _tween, setSizePosition);
		}

		private void setSizePosition(Tween _tween, int _index, Vector3 _position)
        {
			var _transform = _tween.TargetObject.GetComponent<RectTransform>();
           	if(_index == 1){
				var _diffX = (_position.x -  _transform.position.x) * 2f - _tween.OriginValue.x;
				var _diffY = (_position.y -  _transform.position.y) * 2f - _tween.OriginValue.y;
				_tween.DiffValue = new Vector2(_diffX, _diffY);
			}else{
				var _originX = (_position.x - _transform.position.x) * 2f;
				var _originY = (_position.y - _transform.position.y) * 2f;
				var _targetValue = _tween.OriginValue + _tween.DiffValue;
				_tween.OriginValue = new Vector2(_originX, _originY);
				_tween.DiffValue = new Vector2(_targetValue.x - _originX, _targetValue.y - _originY);
			}
        }
    }
}
