using System.Collections;
using System.Collections.Generic;
using BicUtil.CustomTimeLine;
using UnityEditor;
using UnityEngine;

namespace BicUtil.Tween
{
	public partial class BicTweenEditor {

		//[TweenEditorDrawNode(TweenType.?)]
		//private Rect draw?Node(TweenModel _tween, Vector2 _startPosition, Timeline _timeline)
		

		//[TweenEditorOnClickedNode(TweenType.?)]
		//private void onClicked?Node(TweenModel _tween, Event _event)


		//[TweenEditorSettingNode(TweenType.?)]
		private void drawScaleSetting(TweenModel _tween){
			_tween.OriginValue = EditorGUILayout.Vector3Field("Origin Value", _tween.OriginValue);
			_tween.DiffValue = EditorGUILayout.Vector3Field("Diff Value", _tween.DiffValue);
		}


		//[TweenEditorHandleController(TweenType.?)]
		private void drawScaleHandleControl(TweenModel _tween){
			if(_tween.TargetObject == null){
				return;
			}

			var _parentPos = _tween.TargetObject.transform.position;

		}

	}
}
