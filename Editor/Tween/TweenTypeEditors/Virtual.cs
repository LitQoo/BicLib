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


        [TweenEditorDrawNode(TweenType.Virtual)]
        private Rect drawVirtualNode(Tween _tween, Vector2 _startPosition, Timeline _timeline){
			if(_tween.ChildDataList != null && _tween.ChildDataList.Count > 0){
		    	var _virtual = selectedTweenPool.GetTween(_tween.ChildDataList[0]);
				_tween.Time = _virtual.Time;
			}

			return drawSingleNode(_tween, _startPosition, _timeline, Color.green, "Virtual("+_tween.Name+")");
        }
		
		[TweenEditorDrawSetting(TweenType.Virtual)]
		private void drawVirtualSetting(Tween _tween){
			if(selectedTweenPool != null){
				var _titles = new List<string>();
				var _groups = new List<Tween>();
				for(int i = 0; i < selectedTweenPool.GroupIdList.Count; i++){
					var _group = selectedTweenPool.GetGroup(i);
					_titles.Add(_group.Name);
					_groups.Add(_group);
				}

				int _selectedChildIndex = 0;
				
				if(_tween.ChildDataList != null && _tween.ChildDataList.Count > 0){
					foreach(var _groupId in selectedTweenPool.GroupIdList){
						if(_groupId == _tween.childDataList[0]){
							break;
						}

						_selectedChildIndex++;
					}
				}

				_selectedChildIndex = EditorGUILayout.Popup("PlayGroup", _selectedChildIndex, _titles.ToArray());

				_tween.childDataList = new List<int>();
				_tween.childDataList.Add(selectedTweenPool.GroupIdList[_selectedChildIndex]);
				
			}

			// if (GUILayout.Button ("[select group]", EditorStyles.toolbarDropDown)) {
			// 	GenericMenu toolsMenu = new GenericMenu ();
			// 	if(selectedTweenPool != null){
			// 		for(int i = 0; i < selectedTweenPool.GroupIdList.Count; i++){
			// 			int _index = i;
			// 			toolsMenu.AddItem (new GUIContent (selectedTweenPool.GetGroup(_index).Name), false, delegate() {
			// 				//selectedGroupIndex = _index;
			// 			});
			// 		}
			// 	}

			// 	toolsMenu.DropDown (new Rect (3, 37, 0, 0));
			// 	EditorGUIUtility.ExitGUI ();
			// }
		}
	}
}
