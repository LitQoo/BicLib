using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.CustomTimeLine;
using UnityEditor;
using UnityEngine;

namespace BicUtil.Tween
{
	public partial class BicTweenEditor {
		[TweenEditorDrawNode(TweenType.Active)]
        private Rect drawActiveNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
            return drawSingleNode(_tween, _startPosition, _timeline);
        }

		[TweenEditorDrawSetting(TweenType.Active)]
		private void drawActiveSetting(TweenModel _tween){
			var _value1 = EditorGUILayout.Toggle("IsActive On Start" ,_tween.DiffValue.x >= 1f);
			var _value2 = EditorGUILayout.Toggle("IsActive On End" ,_tween.DiffValue.y >= 1f);
            _tween.DiffValue = new Vector2(_value1 ? 1f : 0f, _value2 ? 1f : 0f);
        }
	}
}