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


        [TweenEditorDrawNode(TweenType.MoveLocal)]
        private Rect drawMoveNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
            return drawSingleNode(_tween, _startPosition, _timeline);
        }
		
		[TweenEditorDrawSetting(TweenType.MoveLocal)]
		private void drawMoveSetting(TweenModel _tween){
			_tween.OriginValue = EditorGUILayout.Vector3Field("Origin Value", _tween.OriginValue);
			_tween.DiffValue = EditorGUILayout.Vector3Field("Diff Value", _tween.DiffValue);
		}

        [TweenEditorDrawHandleControl(TweenType.MoveLocal)]
		private void drawMoveHandleControl(TweenModel _tween){
			if(_tween.TargetObject == null){
				return;
			}

			var _parentPos = _tween.TargetObject.transform.parent.position;
		
			Handles.color = Color.green;
			Handles.DrawLine(_parentPos + (Vector3)_tween.OriginValue, _parentPos + (Vector3)_tween.OriginValue + (Vector3)_tween.DiffValue);

			var _startPosition = _parentPos + (Vector3)_tween.OriginValue;
			var _endPosition = _parentPos + (Vector3)_tween.OriginValue + (Vector3)_tween.DiffValue;

			drawMoveHandle(_startPosition, 0, Color.red, 10, _tween, setMovePosition);
			drawMoveHandle(_endPosition, 1, Color.blue, 10, _tween, setMovePosition);

		}

		private void drawMoveHandle (Vector3 point, int index, Color _pointColor, float _size, TweenModel _tween, Action<TweenModel, int, Vector3> _callback) {
            Handles.color = _pointColor;
            
			EditorGUI.BeginChangeCheck();
			point = Handles.FreeMoveHandle(point, Quaternion.identity, _size,new Vector3(10f,10f,10f),Handles.DotHandleCap);
			if(EditorGUI.EndChangeCheck()){
				_callback(_tween, index, point);
				EditorUtility.SetDirty(selectedTweenPool);
			}
        }
		
		private void setMovePosition(TweenModel _tween, int _index, Vector3 _position){
			var _parentPos = _tween.TargetObject.transform.parent.position;
			if(_index == 0){
				//_tween.DiffValue = (Vector3)_tween.OriginValue - ((new Vector3(_position.x, _position.y)) -  (Vector3)_tween.DiffValue);
				_tween.OriginValue = _position - _parentPos;
			}else if(_index == 1){
				_tween.DiffValue = _position - _parentPos - (Vector3)_tween.OriginValue;
			}
		}
	}
}
