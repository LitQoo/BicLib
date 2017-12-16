using System.Collections;
using System.Collections.Generic;
using BicUtil.CustomTimeLine;
using UnityEditor;
using UnityEngine;

namespace BicUtil.Tween
{
	public partial class BicTweenEditor {

		//[TweenEditorDrawNode(TweenType.?)]
        private void drawBezierHandleControl(TweenModel _tween){
			if(_tween.TargetObject == null){
				return;
			}

            float endPointSize = 10;
            var _vectors = BicTween.ChildDataToBezier(_tween.ChildDataList).ToArray();
            Vector3 p0 = _vectors[0];
            drawMoveHandle(p0, 0, Color.red, endPointSize, _tween, setBezierChild);

            for (int i = 1; i < _vectors.Length; i += 3) {
                Vector3 p1 = _vectors[i];
                Vector3 p2 = _vectors[i + 1];
                Vector3 p3 = _vectors[i + 2]; 

                Handles.color = Color.magenta;
                Handles.DrawDottedLine(p0, p1, 1f);
                Handles.DrawDottedLine(p2, p3, 1f);
                drawMoveHandle(p1, i, Color.magenta, endPointSize, _tween, setBezierChild);
                drawMoveHandle(p2, i + 1, Color.magenta, endPointSize, _tween, setBezierChild);
                drawMoveHandle(p3, i + 2, Color.blue, endPointSize, _tween, setBezierChild);

                Handles.DrawBezier(p0, p3, p1, p2, Color.cyan, null, 2f);
                p0 = p3;
            }
        }

		private void setBezierChild(TweenModel _tween, int _index, Vector3 _position){
			_tween.childDataList[_index*3] = (int)(_position.x*1000);
			_tween.childDataList[_index*3+1] = (int)(_position.y*1000);
			_tween.childDataList[_index*3+2] = (int)(_position.z*1000);
		}

		//[TweenEditorOnClickedNode(TweenType.?)]
		//private void onClicked?Node(TweenModel _tween, Event _event)


		//[TweenEditorSettingNode(TweenType.?)]
        private void drawBezierSetting(TweenModel _tween){
            var _vectors = BicTween.ChildDataToBezier(_tween.ChildDataList).ToArray();
            for(int i = 0; i < _vectors.Length; i++)
            {
                string _name = "";

                _name += i.ToString();

                if(i % 3 == 0){
                    _name += " End Point";
                }else{
                    _name += " Control Point";
                }

                EditorGUILayout.BeginHorizontal ();
				GUILayout.Label(_name);

                var _vector = EditorGUILayout.Vector3Field("",_vectors[i]);
                _tween.childDataList[i*3] = (int)(_vector.x*1000);
                _tween.childDataList[i*3+1] = (int)(_vector.y*1000);
                _tween.childDataList[i*3+2] = (int)(_vector.z*1000);


                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button("Add Last")) {
                Vector3 point = _vectors[_vectors.Length - 1];
                _tween.childDataList.Add((int)((point.x + 50) * 1000));
                _tween.childDataList.Add((int)(point.y * 1000));
                _tween.childDataList.Add((int)(point.z * 1000));
                _tween.childDataList.Add((int)((point.x + 100) * 1000));
                _tween.childDataList.Add((int)(point.y * 1000));
                _tween.childDataList.Add((int)(point.z * 1000));
                _tween.childDataList.Add((int)((point.x + 150) * 1000));
                _tween.childDataList.Add((int)(point.y * 1000));
                _tween.childDataList.Add((int)(point.z * 1000));

				EditorUtility.SetDirty(selectedTweenPool);
            }

            if (GUILayout.Button("Remove Last")) {
                //Undo.RecordObject(curveManager, "Remove Point");
                _tween.childDataList.RemoveRange(_tween.childDataList.Count - 10, 9);
               	EditorUtility.SetDirty(selectedTweenPool);
            }

            // if (GUILayout.Button("Loop :" + selectedCurve.Loop.ToString())) {
            //     //Undo.RecordObject(curveManager, "Loop");
            //     //EditorUtility.SetDirty(curveManager);
            //    // selectedCurve.Loop = !selectedCurve.Loop;
            // }

            EditorGUILayout.EndHorizontal ();

            _tween.Data = null;
        }


		//[TweenEditorHandleController(TweenType.?)]
		//private void draw?Preview(TweenModel _tween)

	}
}
