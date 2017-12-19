using System.Collections;
using System.Collections.Generic;
using BicUtil.CustomTimeLine;
using UnityEngine;

namespace BicUtil.Tween
{
	public partial class BicTweenEditor {
		private Rect drawSingleNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
            _tween.editor_rect =new Rect(_startPosition.x,_startPosition.y, _timeline.SecondsToGUI(_tween.Time),20);
            string _title = (_tween.TargetObject != null ? _tween.TargetObject.name : "null") + "." + _tween.Type.ToString();
			var _backgroundColor = selectedTween == _tween ? Color.yellow : Color.white;
			
			drawNode(_tween, _title, _backgroundColor);
            return _tween.editor_rect;
        }

        private void drawNode(TweenModel _tween, string _text, Color _color){
            var _lastColor = GUI.backgroundColor;
            GUI.backgroundColor = _color;
            GUI.Box (_tween.editor_rect,"","TL LogicBar 0");
            GUIStyle style = new GUIStyle("Label");
            Vector3 size=style.CalcSize(new GUIContent(_text));
            Rect rect1=new Rect(_tween.editor_rect.x+_tween.editor_rect.width*0.5f-size.x*0.5f,_tween.editor_rect.y+_tween.editor_rect.height*0.5f-size.y*0.5f,size.x,size.y);
            GUI.Label(rect1, _text, style);
            GUI.backgroundColor = _lastColor;
        }

        private bool onClickedNodeSingle(TweenModel _tween, Event _event){
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

                return true;
            }else{
                return false;
            }
        }
	}
}
