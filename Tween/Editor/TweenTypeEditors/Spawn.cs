using System.Collections;
using System.Collections.Generic;
using BicUtil.CustomTimeLine;
using UnityEditor;
using UnityEngine;

namespace BicUtil.Tween
{
	public partial class BicTweenEditor {

		[TweenEditorDrawHandleControl(TweenType.Spawn)]
		private void drawSpawnHandleControl(TweenModel _tween){
			drawGroupHandleControl(_tween);
		}

		[TweenEditorDrawNode(TweenType.Spawn)]
		private Rect drawSpawnNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
			float _xPos = _startPosition.x;
			float _height = 20;
			var _backgroundColor = selectedTween == _tween ? Color.yellow : Color.red;

			var _childList = _tween.GetChildList();
			for(int i = 0; i < _childList.Count; i++){
				var _pos = DrawNode(_childList[i], _startPosition + new Vector2(0, _height), _timeline);
				_xPos = Mathf.Max(_pos.x + _pos.width, _xPos);
				_height += _pos.height;
			}

			_tween.editor_rect = new Rect(_startPosition.x, _startPosition.y, _xPos - _startPosition.x, 20);
			drawNode(_tween, _tween.Name, _backgroundColor);

			var _resultRect2 = new Rect(_startPosition.x, _startPosition.y, _xPos - _startPosition.x ,_height);
			Handles.DrawSolidRectangleWithOutline(_resultRect2, Color.clear, _backgroundColor);


			GUILayout.BeginArea (new Rect(Mathf.Max(_xPos - 20, _startPosition.x), _startPosition.y , 20, 20));
			if(GUILayout.Button("+")){
				openToAddTweenMenu(_tween);
			}
			GUILayout.EndArea();
			return _resultRect2;
		}

		//[TweenEditorOnClickedNode(TweenType.?)]
		//private void onClicked?Node(TweenModel _tween, Event _event){
		//}

		//[TweenEditorSettingNode(TweenType.?)]
		//void drawMoveSetting(TweenModel _tween)

		//[TweenEditorHandleController(TweenType.?)]

	}
}
