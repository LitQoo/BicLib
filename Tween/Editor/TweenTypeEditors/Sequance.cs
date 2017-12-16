using System.Collections;
using System.Collections.Generic;
using BicUtil.CustomTimeLine;
using UnityEditor;
using UnityEngine;

namespace BicUtil.Tween
{
	public partial class BicTweenEditor {

		//[TweenEditorDrawNode(TweenType.Sequance)]
		private Rect drawSequanceNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
			var _position = _startPosition + new Vector2(0, 20);
			var _rect = new Rect();
			float _heightMax = 20;
			var _backgroundColor = selectedTween == _tween ? Color.yellow : Color.blue;
			var _childList = _tween.GetChildList();
			for(int i = 0; i < _childList.Count; i++){
				_rect = DrawNode(_childList[i], _position, _timeline);
				_position = new Vector2(_rect.x + _rect.width, _rect.y);
				_heightMax = Mathf.Max(_heightMax, _rect.height);
			}

			_tween.editor_rect = new Rect(_startPosition.x, _startPosition.y, _position.x-_startPosition.x, 20);
			drawNode(_tween, _tween.Name, selectedTween == _tween ? Color.yellow : Color.blue);

			var _resultRect = new Rect(_startPosition.x, _startPosition.y, _position.x-_startPosition.x, _heightMax + 20);
			Handles.DrawSolidRectangleWithOutline(_resultRect, Color.clear, _backgroundColor);


			GUILayout.BeginArea (new Rect(Mathf.Max(_position.x - 20, _startPosition.x), _startPosition.y, 20, 20));
			if(GUILayout.Button("+")){
				openToAddTweenMenu(_tween);
			}
			GUILayout.EndArea();
			return _resultRect;
		}

		//[TweenEditorOnClickedNode(TweenType.Sequance)]
		private void onClickedNodeGroup(TweenModel _tween, Event _event){
			if(_tween.editor_rect.Contains(_event.mousePosition)){
				switch(_event.type){
					case EventType.mouseDown:

					break;
					case EventType.mouseUp:
						if(_event.button == 0){
							selectTween(_tween);
						}else if(_event.button == 1){
							popupMenu(_tween);
						}
					break;
					case EventType.mouseDrag:
					break;
				}
			}else{
				var _childList = _tween.GetChildList();
				for(int i = 0; i < _childList.Count; i++){
					OnClicked(_childList[i], _event);
				}
			}
		}

		//[TweenEditorHandleController(TweenType.Sequance)]
		private void drawGroupHandleControl(TweenModel _tween){
			var _childs = _tween.GetChildList();
			for(int i = 0; i < _childs.Count; i++){
				DrawHandleControl(_childs[i]);
			}
		}

		//[TweenEditorSettingNode(TweenType.Sequance)]


	}
}
