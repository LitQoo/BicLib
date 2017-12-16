// using System;
// using System.Collections.Generic;
// using BicUtil.CustomTimeLine;
// using UnityEditor;
// using UnityEngine;

// namespace BicUtil.Tween
// {

//     public static class TweenModelEditorExtension{
//         public static Action<TweenModel> OnSelected;
//         public static Action<TweenModel> OnClickedRightButton;
//         public static Action<TweenModel> OnAddTween;
        
//         public static Rect DrawNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
//             if(_tween == null){
//                 Debug.LogWarning("tween is null");
//                 return new Rect(0, 0, 0, 0);
//             }
//             switch(_tween.Type){
//                 case TweenType.Sequance:
//                    return drawSequanceNode(_tween, _startPosition, _timeline);
//                 case TweenType.Spawn:
//                     return drawSpawnNode(_tween, _startPosition, _timeline);
//                 default:
//                     return drawSingleNode(_tween, _startPosition, _timeline);

//             }
//         }

//         private static void drawRect(Rect _resultRect, Color _color){
//             Handles.BeginGUI();
//             Handles.color = _color;
//             Handles.DrawLine(new Vector3(_resultRect.x, _resultRect.y), new Vector3(_resultRect.x, _resultRect.y + _resultRect.height));
//              Handles.DrawLine(new Vector3(_resultRect.x, _resultRect.y), new Vector3(_resultRect.x + _resultRect.width, _resultRect.y));
//              Handles.DrawLine(new Vector3(_resultRect.x+ _resultRect.width, _resultRect.y), new Vector3(_resultRect.x + _resultRect.width, _resultRect.y+ _resultRect.height));
//              Handles.DrawLine(new Vector3(_resultRect.x, _resultRect.y+ _resultRect.height), new Vector3(_resultRect.x + _resultRect.width, _resultRect.y+ _resultRect.height));

//             Handles.EndGUI();
//         }

//         private static Rect drawSingleNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
//             _tween.editor_rect =new Rect(_startPosition.x,_startPosition.y, _timeline.SecondsToGUI(_tween.Time),20);
//             string _title = (_tween.TargetObject != null ? _tween.TargetObject.name : "null") + "." + _tween.Type.ToString();
//             _tween.drawNode(_title, Color.white);
//             return _tween.editor_rect;
//         }

//         private static void drawNode(this TweenModel _tween, string _text, Color _color){
//             var _lastColor = GUI.backgroundColor;
//             GUI.backgroundColor = _color;
//             GUI.Box (_tween.editor_rect,"","TL LogicBar 0");
//             GUIStyle style = new GUIStyle("Label");
//             Vector3 size=style.CalcSize(new GUIContent(_text));
//             Rect rect1=new Rect(_tween.editor_rect.x+_tween.editor_rect.width*0.5f-size.x*0.5f,_tween.editor_rect.y+_tween.editor_rect.height*0.5f-size.y*0.5f,size.x,size.y);
//             GUI.Label(rect1, _text, style);
//             GUI.backgroundColor = _lastColor;
//         }

//         private static Rect drawSequanceNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
//             var _position = _startPosition + new Vector2(0, 20);
//             var _rect = new Rect();
//             float _heightMax = 20;
//             var _childList = _tween.GetChildList();
//             for(int i = 0; i < _childList.Count; i++){
//                 _rect = DrawNode(_childList[i], _position, _timeline);
//                 _position = new Vector2(_rect.x + _rect.width, _rect.y);
//                 _heightMax = Mathf.Max(_heightMax, _rect.height);
//             }


//             _tween.editor_rect = new Rect(_startPosition.x, _startPosition.y, _position.x-_startPosition.x, 20);
//             _tween.drawNode("Sequance", Color.blue);

//             var _resultRect = new Rect(_startPosition.x, _startPosition.y, _position.x-_startPosition.x, _heightMax + 20);
//             drawRect(_resultRect, Color.blue);

//             GUILayout.BeginArea (new Rect(Mathf.Max(_position.x - 20, _startPosition.x), _startPosition.y, 20, 20));
//             if(GUILayout.Button("+")){
//                 OnAddTween(_tween);
//             }
//             GUILayout.EndArea();
//             return _resultRect;
//         }

//         private static Rect drawSpawnNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
//             float _xPos = _startPosition.x;
//             float _height = 20;

//             var _childList = _tween.GetChildList();
//             for(int i = 0; i < _childList.Count; i++){
//                 var _pos = DrawNode(_childList[i], _startPosition + new Vector2(0, _height), _timeline);
//                 _xPos = Mathf.Max(_pos.x + _pos.width, _xPos);
//                 _height += _pos.height;
//             }

//             _tween.editor_rect = new Rect(_startPosition.x, _startPosition.y, _xPos - _startPosition.x, 20);
//             _tween.drawNode("Spawn", Color.red);

//             var _resultRect2 = new Rect(_startPosition.x, _startPosition.y, _xPos - _startPosition.x ,_height);
//             drawRect(_resultRect2, Color.red);

//             GUILayout.BeginArea (new Rect(Mathf.Max(_xPos - 20, _startPosition.x), _startPosition.y , 20, 20));
//             if(GUILayout.Button("+")){
//                 OnAddTween(_tween);
//             }
//             GUILayout.EndArea();
//             return _resultRect2;
//         }

//         public static void OnClicked(TweenModel _tween, Event _event){
            
//             switch(_tween.Type){
//             case TweenType.Spawn:
//             case TweenType.Sequance:
//                 if(_tween.editor_rect.Contains(_event.mousePosition)){
//                     switch(_event.type){
//                         case EventType.mouseDown:

//                         break;
//                         case EventType.mouseUp:
//                             if(_event.button == 0){
//                                 OnSelected(_tween);
//                             }else if(_event.button == 1){
//                                 OnClickedRightButton(_tween);
//                             }
//                         break;
//                         case EventType.mouseDrag:
//                         break;
//                     }
//                 }else{
//                     var _childList = _tween.GetChildList();
//                     for(int i = 0; i < _childList.Count; i++){
//                         OnClicked(_childList[i], _event);
//                     }
//                 }
//             break;
//             default:
//                 if(_tween.editor_rect.Contains(_event.mousePosition)){
//                        switch(_event.type){
//                            case EventType.mouseDown:

//                            break;
//                            case EventType.mouseUp:
//                             if(_event.button == 0){
//                                 OnSelected(_tween);
//                             }else if(_event.button == 1){
//                                 OnClickedRightButton(_tween);
//                             }
//                            break;
//                            case EventType.mouseDrag:
//                            break;
//                        }
//                     }
//                 break;
//             }
//         }

//         public static void DrawSetting(TweenModel _tween){
//             drawDefaultSetting(_tween);

//             if(_tween.TargetObject == null){
//                 return;
//             }

//             switch(_tween.Type){
//                 case TweenType.Move:
//                     drawMoveSetting(_tween);
//                 break;
//                 case TweenType.Bezier:
//                     drawBezierSetting(_tween);
//                 break; 
//             }
//         }

//         public static void DrawPreview(TweenModel _tween){
//             if(_tween.TargetObject == null){
//                 return;
//             }

//             switch(_tween.Type){
//                 case TweenType.Move:
//                     drawMovePreview(_tween);
//                 break;
//                 case TweenType.Bezier:
//                     drawBezierPreview(_tween);
//                 break;
//             }
//         }

//         private static void drawDefaultSetting(TweenModel _tween){
//             _tween.Name = EditorGUILayout.TextField("Name", _tween.Name);
//             _tween.TargetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", _tween.TargetObject, typeof(GameObject), true);
//             _tween.Time = EditorGUILayout.FloatField("Time", _tween.Time);
//             _tween.RepeatCount = EditorGUILayout.IntField("Repeat Count", _tween.RepeatCount);
//             _tween.Type = (TweenType)EditorGUILayout.EnumPopup("Tween Type", _tween.Type);
//             _tween.UpdateFunc = UpdateFuncs.GetFunc(_tween.Type);
//             _tween.EaseType = (EaseType)EditorGUILayout.EnumPopup("Ease Type", _tween.EaseType);
//             _tween.SetEase(EaseFuncs.GetFunc(_tween.EaseType));
//         }

//         private static void drawMoveSetting(TweenModel _tween){
//             _tween.OriginValue = EditorGUILayout.Vector3Field("Origin Value", _tween.OriginValue);
//             _tween.DiffValue = EditorGUILayout.Vector3Field("Diff Value", _tween.DiffValue);
//         }

        
//         private static void drawMovePreview(TweenModel _tween){
//             var _parentPos = _tween.TargetObject.transform.parent.position;

//             Handles.color = Color.green;
//             Handles.DrawLine(_parentPos + (Vector3)_tween.OriginValue, _parentPos + (Vector3)_tween.OriginValue + (Vector3)_tween.DiffValue);

//             Handles.color = Color.red;
//             if (Handles.Button(_parentPos + (Vector3)_tween.OriginValue, _tween.TargetObject.transform.rotation, 10, 10, Handles.DotCap)) {
                
//             }

//             Handles.color = Color.blue;
//             if (Handles.Button(_parentPos + (Vector3)_tween.OriginValue + (Vector3)_tween.DiffValue, _tween.TargetObject.transform.rotation, 10, 10, Handles.DotCap)) {

//             }


//         }

//         private static void drawBezierSetting(TweenModel _tween){
//             var _vectors = BicTween.ChildDataToBezier(_tween.ChildDataList).ToArray();
//             for(int i = 0; i < _vectors.Length; i++)
//             {
//                 string _name = "";

//                 _name += i.ToString();

//                 if(i % 3 == 0){
//                     _name += " End Point";
//                 }else{
//                     _name += " Control Point";
//                 }

//                 EditorGUILayout.BeginHorizontal ();
//                 if(GUILayout.Button(_name)){
//                    // selectedIndex = i;
//                    // EditorUtility.SetDirty(curveManager);
//                 }

//                 var _vector = EditorGUILayout.Vector3Field("",_vectors[i]);
//                 _tween.childDataList[i*3] = (int)(_vector.x*1000);
//                 _tween.childDataList[i*3+1] = (int)(_vector.y*1000);
//                 _tween.childDataList[i*3+2] = (int)(_vector.z*1000);


//                 EditorGUILayout.EndHorizontal();
//             }

//             EditorGUILayout.BeginHorizontal ();
//             if (GUILayout.Button("Add Last")) {
//                 //Undo.RecordObject(curveManager, "Add Point");
//                 Vector3 point = _vectors[_vectors.Length - 1];
//                 _tween.childDataList.Add((int)((point.x + 50) * 1000));
//                 _tween.childDataList.Add((int)(point.y * 1000));
//                 _tween.childDataList.Add((int)(point.z * 1000));
//                 _tween.childDataList.Add((int)((point.x + 100) * 1000));
//                 _tween.childDataList.Add((int)(point.y * 1000));
//                 _tween.childDataList.Add((int)(point.z * 1000));
//                 _tween.childDataList.Add((int)((point.x + 150) * 1000));
//                 _tween.childDataList.Add((int)(point.y * 1000));
//                 _tween.childDataList.Add((int)(point.z * 1000));

//                // EditorUtility.SetDirty(curveManager);
//             }

//             if (GUILayout.Button("Remove Last")) {
//                 //Undo.RecordObject(curveManager, "Remove Point");
//                  _tween.childDataList.RemoveRange(_tween.childDataList.Count - 10, 9);
//                // EditorUtility.SetDirty(curveManager);
//             }

//             // if (GUILayout.Button("Loop :" + selectedCurve.Loop.ToString())) {
//             //     //Undo.RecordObject(curveManager, "Loop");
//             //     //EditorUtility.SetDirty(curveManager);
//             //    // selectedCurve.Loop = !selectedCurve.Loop;
//             // }

//             EditorGUILayout.EndHorizontal ();

//             _tween.Data = null;
//         }

//         private static int selectedHandleIndex = -1;
//         private static void drawBezierPreview(TweenModel _tween){
//             float endPointSize = 10;
//             var _vectors = BicTween.ChildDataToBezier(_tween.ChildDataList).ToArray();
//             Vector3 p0 = _vectors[0];
//             DrawHandle(p0, 0, Color.red, endPointSize, _tween);

//             for (int i = 1; i < _vectors.Length; i += 3) {
//                 Vector3 p1 = _vectors[i];
//                 Vector3 p2 = _vectors[i + 1];
//                 Vector3 p3 = _vectors[i + 2]; 

//                 Handles.color = Color.magenta;
//                 Handles.DrawLine(p0, p1);
//                 Handles.DrawLine(p2, p3);
//                 DrawHandle(p1, i, Color.magenta, endPointSize, _tween);
//                 DrawHandle(p2, i + 1, Color.magenta, endPointSize, _tween);
//                 DrawHandle(p3, i + 2, Color.blue, endPointSize, _tween);

//                 Handles.DrawBezier(p0, p3, p1, p2, Color.cyan, null, 2f);
//                 p0 = p3;
//             }
//         }

//         private static void DrawHandle (Vector3 point, int index, Color _pointColor, float _size, TweenModel _tween) {
//             Handles.color = _pointColor;
//             var _handleRotate = Tools.pivotRotation == PivotRotation.Local ? _tween.TargetObject.transform.rotation : Quaternion.identity;
//             if (Handles.Button(point, _handleRotate, _size, _size, Handles.DotCap)) {
//                  selectedHandleIndex = index;
//                     //Repaint();
//             }

//             if (selectedHandleIndex == index) {
//                 EditorGUI.BeginChangeCheck();
//                 point = Handles.DoPositionHandle(point, _handleRotate);
//                 if (EditorGUI.EndChangeCheck()) {

//                     _tween.childDataList[index*3] = (int)(point.x*1000);
//                     _tween.childDataList[index*3+1] = (int)(point.y*1000);
//                     _tween.childDataList[index*3+2] = (int)(point.z*1000);
//                     //Undo.RecordObject(curveManager, "Move Point");
//                     //EditorUtility.SetDirty(curveManager);
//                     //selectedCurve.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
//                 }
//             }
//         }
//     }

// }