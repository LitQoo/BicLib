using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.CustomTimeLine;
using UnityEditor;
using UnityEngine;

namespace BicUtil.Tween
{
	public partial class BicTweenEditor {

		[TweenEditorDrawNode(TweenType.Alpha)]
		private Rect drawAlphaNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
			return drawSingleNode(_tween, _startPosition, _timeline);
		}

		[TweenEditorDrawSetting(TweenType.Alpha)]
		private void drawAlphaSetting(TweenModel _tween){
			var _originAlpha = EditorGUILayout.FloatField("Origin Alpha", _tween.OriginValue.w);
			_tween.OriginValue = new Vector4(0, 0, 0, _originAlpha);
			var _diffAlpha = EditorGUILayout.FloatField("Diff Alpha", _tween.DiffValue.w);
			_tween.DiffValue = new Vector4(0, 0, 0, _diffAlpha);
		}
	}

}