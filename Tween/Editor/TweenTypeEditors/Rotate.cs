using System;
using System.Collections;
using System.Collections.Generic;
using BicUtil.CustomTimeLine;
using UnityEditor;
using UnityEngine;

namespace BicUtil.Tween
{
	public partial class BicTweenEditor {

		//[TweenEditorDrawNode(TweenType.?)]
		//private Rect draw?Node(TweenModel _tween, Vector2 _startPosition, Timeline _timeline)


		//[TweenEditorOnClickedNode(TweenType.?)]
		//private void onClicked?Node(TweenModel _tween, Event _event)


		//[TweenEditorSettingNode(TweenType.?)]
		private void drawRotateSetting(TweenModel _tween){
            _tween.OriginValue = EditorGUILayout.Vector3Field("Origin Value", _tween.OriginValue);
            _tween.DiffValue = EditorGUILayout.Vector3Field("Diff Value", _tween.DiffValue);
        }



		//[TweenEditorHandleController(TweenType.?)]
		//private void draw?Preview(TweenModel _tween)
		private void drawRotateHandleControl(TweenModel _tween){
			if(_tween.TargetObject == null){
				return;
			}

			var _parentPos = _tween.TargetObject.transform.position;

			DrawRotateHandle(0, _tween.OriginValue.z, _tween.DiffValue.z, _parentPos, Color.red, 10, _tween, setRotate);

			// if (Handles.Button(_parentPos , Quaternion.identity, 10, 10, Handles.)) {
			// 				selectedHandleIndex = index;
			// 	Repaint();
			// }
		}

		private void setRotate(TweenModel _tween, int _index, Quaternion _rotate){
			_tween.DiffValue = _rotate.eulerAngles;
		}

		private void DrawRotateHandle (int index, float startAngle, float endAngle, Vector3 point, Color _pointColor, float _size, TweenModel _tween, Action<TweenModel, int, Quaternion> _callback) {

			EditorGUI.BeginChangeCheck();
			var _startHandle = new Vector2(40*Mathf.Cos(Mathf.Deg2Rad*(startAngle + 180)),40*Mathf.Sin(Mathf.Deg2Rad*(startAngle + 180)));
			var _startHandlePosition = (Vector2)point + _startHandle;
			var _startHandleDiffPosition = Handles.FreeMoveHandle(_startHandlePosition, Quaternion.identity, _size,new Vector3(10f,10f,10f),Handles.DotHandleCap);
			if(EditorGUI.EndChangeCheck()){
				_tween.OriginValue = new Vector3(0, 0, _tween.OriginValue.z + getAngle(_startHandlePosition - (Vector2)_tween.TargetObject.transform.position, _startHandleDiffPosition - (Vector3)_tween.TargetObject.transform.position));


				EditorUtility.SetDirty(selectedTweenPool);
				Repaint();
			}

			Handles.color = _pointColor;
			Handles.DrawWireArc(_tween.TargetObject.transform.position, Vector3.forward, _startHandle.normalized, endAngle, 40);

			int _round = (int)(Mathf.Abs(endAngle) / 360f);
			Handles.Label(_tween.TargetObject.transform.position, _round.ToString());


			EditorGUI.BeginChangeCheck();
			var _endHandle = new Vector2(40*Mathf.Cos(Mathf.Deg2Rad*(startAngle + endAngle + 180)),40*Mathf.Sin(Mathf.Deg2Rad*(startAngle + endAngle + 180)));
			var _endHandlePosition = (Vector2)point + _endHandle;
			var _endHandleDiffPosition = Handles.FreeMoveHandle(_endHandlePosition, Quaternion.identity, _size,new Vector3(10f,10f,10f),Handles.ConeHandleCap);
			if(EditorGUI.EndChangeCheck()){
				_tween.DiffValue = new Vector3(0, 0, _tween.DiffValue.z + getAngle(_endHandlePosition - (Vector2)_tween.TargetObject.transform.position, _endHandleDiffPosition - (Vector3)_tween.TargetObject.transform.position));
				EditorUtility.SetDirty(selectedTweenPool);
				Repaint();
			}
		}


		private float getAngle(Vector3 _origin, Vector3 _diff, float _offsetAngle = 0){
			//Vector2 _Dir = (Vector2)_handle - (Vector2)_center;
			var _angle = Vector2.Angle(_diff, _origin) + _offsetAngle;
			//var _angle1 = Vector2.Angle(Vector2.left, _diff);
			//var _angle2 = Vector2.Angle(Vector2.left, _origin);
			return Mathf.DeltaAngle(Mathf.Atan2(_origin.y, _origin.x) * Mathf.Rad2Deg, Mathf.Atan2(_diff.y, _diff.x) * Mathf.Rad2Deg);

		}

	}
}
