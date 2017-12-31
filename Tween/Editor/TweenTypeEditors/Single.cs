using System.Collections;
using System.Collections.Generic;
using BicUtil.CustomTimeLine;
using UnityEditor;
using UnityEngine;

namespace BicUtil.Tween
{
	public partial class BicTweenEditor {
		private Rect drawSingleNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
            _tween.editor_rect =new Rect(_startPosition.x,_startPosition.y, _timeline.SecondsToGUI(_tween.Time * (_tween.RepeatCount + 1)),20);
            string _title = (_tween.TargetObject != null ? _tween.TargetObject.name : "null") + "." + _tween.Type.ToString();
			var _backgroundColor = selectedTweens.Contains(_tween) ? Color.yellow : Color.white;
			
			drawNode(_tween, _title, _backgroundColor);
            return _tween.editor_rect;
        }

        private Rect drawSingleNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline, Color _backColor, string _title){
            _tween.editor_rect =new Rect(_startPosition.x,_startPosition.y, _timeline.SecondsToGUI(_tween.Time * (_tween.RepeatCount + 1)),20);
            var _backgroundColor = selectedTweens.Contains(_tween) ? Color.yellow : _backColor;
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
            
            if(rect1.width < _tween.editor_rect.width){
                GUI.Label(rect1, _text, style);
            }
            
            GUI.backgroundColor = _lastColor;
        }

        private bool isAncestor(TweenModel _child, TweenModel _parent){
            TweenModel _ancestor = _child;
            while(true){
                if(_ancestor == null){
                    return false;
                }else if(_ancestor == _parent){
                    return true;
                }

                _ancestor = _ancestor.editor_parent;
            }
        }

        private bool onClickedNodeSingle(TweenModel _tween, Event _event){
            if(_tween.editor_rect.Contains(_event.mousePosition)){
                switch(_event.type){
                    case EventType.mouseUp:
                        if(selectedTweens.Count == 1 && movingPoint != Rect.zero){
                            var _movingTween = selectedTweens[0];
                            _movingTween.editor_parent.RemoveChild(selectedTweens[0]);
                            int _index = movingTarget.editor_parent.childDataList.IndexOf(movingTarget.Id);
                            if(isPreMoving == false){
                                _index++;
                            }

                            movingTarget.editor_parent.InsertChild(_movingTween, _index);
                            Repaint();
                        }
                    break;
                    case EventType.mouseDown:
                    if(_event.button == 0){
                        if((Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.Command)){
                            if(selectedTweens.Contains(_tween)){
                                unselectTween(_tween);
                            }else{
                                addToSelectTween(_tween);
                            }
                        }else{
                            selectTween(_tween);
                        }
                    }else if(_event.button == 1){
                        popupMenu(_tween);
                    }
                    break;
                    case EventType.mouseDrag:
                        if(_event.button == 0 && _tween.editor_parent != null && selectedTweens.Contains(_tween) == false && !isAncestor(_tween, selectedTweens[0])){
                            if(_tween.editor_parent.Type == TweenType.Sequance){
                                movingTarget = _tween;
                                if(_event.mousePosition.x < _tween.editor_rect.x + _tween.editor_rect.width / 2f){
                                    movingPoint = new Rect(_tween.editor_rect.x, _tween.editor_rect.y, 1, _tween.editor_rect.height);
                                    isPreMoving = true;
                                }else{
                                    movingPoint = new Rect(_tween.editor_rect.x + _tween.editor_rect.width, _tween.editor_rect.y, 1, _tween.editor_rect.height);
                                    isPreMoving = false;
                                }
                            }else if(_tween.editor_parent.Type == TweenType.Spawn){
                                movingTarget = _tween;
                                if(_event.mousePosition.y < _tween.editor_rect.y + _tween.editor_rect.height / 2f){
                                    movingPoint = new Rect(_tween.editor_rect.x, _tween.editor_rect.y, _tween.editor_rect.width, 1);
                                    isPreMoving = true;
                                }else{
                                    movingPoint = new Rect(_tween.editor_rect.x, _tween.editor_rect.y + _tween.editor_rect.height, _tween.editor_rect.width, 1);
                                    isPreMoving = false;
                                }
                            }
                        }else{
                            movingPoint = Rect.zero;
                            return false;
                        }
                    break;
                }

                return true;
            }else{
                return false;
            }
        }
	}
}
