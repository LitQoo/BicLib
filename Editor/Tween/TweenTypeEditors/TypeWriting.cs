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

        [TweenEditorDrawNode(TweenType.TypeWriting)]
        private Rect drawTypeWritingNode(Tween _tween, Vector2 _startPosition, Timeline _timeline){
            return drawSingleNode(_tween, _startPosition, _timeline);
        }

		[TweenEditorDrawSetting(TweenType.TypeWriting)]
		private void drawTypeWritingSetting(Tween _tween){
			_tween.StringData = EditorGUILayout.TextField("Text", _tween.StringData);

			if(_tween.StringData != null){
				_tween.RepeatCount = _tween.StringData.Length;
			}
		}
    }
}
