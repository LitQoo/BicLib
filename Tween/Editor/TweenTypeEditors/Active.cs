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
			var _value = EditorGUILayout.Toggle("IsActive" ,_tween.DiffValue.x >= 1f);
            _tween.DiffValue = new Vector2(_value ? 1f : 0f, 0);
        }
	}
}