using System.Collections;
using System.Collections.Generic;
using BicUtil.CustomTimeLine;
using UnityEditor;
using UnityEngine;

namespace BicUtil.Tween
{
	public partial class BicTweenEditor {

		[TweenEditorDrawHandleControl(TweenType.Sequance)]
		private void drawSequanceHandleControl(TweenModel _tween){
			drawGroupHandleControl(_tween);
		}

		[TweenEditorDrawNode(TweenType.Sequance)]
		private Rect drawSequanceNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
			var _position = _startPosition + new Vector2(0, 20);
			var _rect = new Rect();
			float _heightMax = 20;
			var _backgroundColor = selectedTweens.Contains(_tween) ? Color.yellow : Color.blue;
			float _time = 0;
			var _childList = _tween.GetChildList();
			for(int i = 0; i < _childList.Count; i++){
				_rect = DrawNode(_childList[i], _position, _timeline);
				_position = new Vector2(_rect.x + _rect.width, _rect.y);
				_heightMax = Mathf.Max(_heightMax, _rect.height);
				_time += _childList[i].Time;
			}

			_tween.Time = _time;
			_tween.editor_rect = new Rect(_startPosition.x, _startPosition.y, _position.x-_startPosition.x, 20);
			drawNode(_tween, _tween.Name, selectedTweens.Contains(_tween) ? Color.yellow : Color.blue);

			var _resultRect = new Rect(_startPosition.x, _startPosition.y, _position.x-_startPosition.x, _heightMax + 20);
			Handles.DrawSolidRectangleWithOutline(_resultRect, Color.clear, _backgroundColor);


			GUILayout.BeginArea (new Rect(Mathf.Max(_position.x - 20, _startPosition.x), _startPosition.y, 20, 20));
			if(GUILayout.Button("+")){
				openToAddTweenMenu(_tween);
			}
			GUILayout.EndArea();
			return _resultRect;
		}

		//[TweenEditorSettingNode(TweenType.Sequance)]


	}
}
