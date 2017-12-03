using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace BicUtil.Tween{
	[CustomEditor(typeof(CurveManager))]
	public class CurveManagerEditor : Editor {
		private CurveManager curveManager;
		private Transform handleTransform;
		private Quaternion handleRotate;
		private int selectedIndex = -1;
		Color colorCurve = Color.cyan;
		Color colorEndPoint = Color.magenta;
		Color colorControlPoint = Color.red;
		Color colorControlCurve = Color.red;
		float controlPointSize = 10f; 
		float endPointSize = 10f;
		
		bool settingFoldout = false;
		bool pointsFoldout = true;
		bool currentFoldout = true;
		bool curvetweenFoldout = true;
		bool visibleHandle = true;
		bool curveListFoldout = true;
		bool isPreviewing = false;

		Vector2 pointsScrollPosition = new Vector2(0, 0);
		GameObject positionCopytargetObject = null;
		BezierCurve selectedCurve = null;
		string newCurveName = "";

		void OnEnable()
		{
			EditorApplication.update += EditorUpdate;
		}
	
		void OnDisable()
		{
			EditorApplication.update -= EditorUpdate;
			selectedCurve = null;
			LeanTweenOnEditor.ClearUpdateCallback();
		}

		void EditorUpdate(){
			LeanTweenOnEditor.update();
		}

		public override void OnInspectorGUI () {
			curveManager = target as CurveManager;
			LeanTweenOnEditor.tweenEmpty = curveManager.gameObject;
			EditorGUI.BeginChangeCheck();

			settingFoldout = EditorGUILayout.Foldout(settingFoldout, "Setting");

			if(settingFoldout == true){
				colorCurve = EditorGUILayout.ColorField("Curve Color",colorCurve);
				colorEndPoint = EditorGUILayout.ColorField("End Point Color",colorEndPoint);
				colorControlPoint = EditorGUILayout.ColorField("Control Point Color",colorControlPoint);
				colorControlCurve = EditorGUILayout.ColorField("Control Curve Color",colorControlCurve);
				controlPointSize = EditorGUILayout.Slider("Control Point Size", controlPointSize, 1, 30);
				endPointSize = EditorGUILayout.Slider("End Point Size", endPointSize, 1, 30);
				visibleHandle = EditorGUILayout.Toggle("Visible Handle", visibleHandle);
				EditorUtility.SetDirty(curveManager);
			}

			curveListFoldout = EditorGUILayout.Foldout(curveListFoldout, "Curve List");

			if(curveListFoldout == true){
				if(curveManager.Paths == null){
					curveManager.Paths = new List<BezierCurve>();
				}

				foreach(var _path in curveManager.Paths){
					EditorGUILayout.BeginHorizontal ();
					string _name = "";
					if(_path == selectedCurve){
						_name += ">"; 
					}

					_name += _path.name;

					EditorGUILayout.LabelField(_name);

					if (GUILayout.Button("Remove")) {
						if(selectedCurve == _path){
							selectedCurve = null;
						}

						Undo.RecordObject(curveManager, "Rmove Path");
						curveManager.Paths.Remove(_path);
						EditorUtility.SetDirty(curveManager);
						break;
					}

					if (GUILayout.Button("Edit")) {
						selectedCurve = _path;
						EditorUtility.SetDirty(curveManager);
					}

					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField("New ID");
				newCurveName = EditorGUILayout.TextField(newCurveName);
				if (GUILayout.Button("Add")) {
					if(newCurveName != ""){
						Undo.RecordObject(curveManager, "Add New Curve");
						var _newCurve = new BezierCurve();
						_newCurve.name = newCurveName;
						curveManager.Paths.Add(_newCurve);
						_newCurve.Reset();
						selectedCurve = _newCurve;
						newCurveName = "";
						EditorUtility.SetDirty(curveManager);
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			if(selectedCurve == null || !curveManager.Paths.Contains(selectedCurve)){
				return;
			}
			
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(selectedCurve.name, EditorStyles.boldLabel);

			curvetweenFoldout = EditorGUILayout.Foldout(curvetweenFoldout, "Curve Tween");

			if(curvetweenFoldout == true){
				selectedCurve.easeType = (LeanTweenType)EditorGUILayout.EnumPopup("Ease Type", selectedCurve.easeType);
				selectedCurve.targetObject = (GameObject)EditorGUILayout.ObjectField("Tween Target", selectedCurve.targetObject, typeof(GameObject), true);
				selectedCurve.animationTime = EditorGUILayout.FloatField("Time", selectedCurve.animationTime);
				if (GUILayout.Button(isPreviewing == false ? "Preview" : "Playing")) {
					if(isPreviewing == false && selectedCurve.targetObject != null){
						isPreviewing = true;
						var _positionBackup = selectedCurve.targetObject.transform.localPosition;
						var _be = new LTBezierPath(selectedCurve.GetPathForLeantween(Vector3.zero).ToArray());
						LeanTweenOnEditor.moveLocal(selectedCurve.targetObject, _be, selectedCurve.animationTime).setEase(selectedCurve.easeType).setOnComplete(()=>{
							LeanTweenOnEditor.delayedCall(selectedCurve.targetObject, 0.3f, ()=>{
								selectedCurve.targetObject.transform.localPosition = _positionBackup;
								isPreviewing = false;
							});
						});
					}
				}

			}

			pointsFoldout = EditorGUILayout.Foldout(pointsFoldout, "Points");

			if(pointsFoldout == true){
				EditorGUILayout.BeginVertical(GUILayout.MinHeight(100f));
				pointsScrollPosition = EditorGUILayout.BeginScrollView(pointsScrollPosition, false, false); 
				for(int i = 0; i < selectedCurve.ControlPointCount; i++)
				{
					string _name = "";

					if(i == selectedIndex){
						_name += ">";
					}

					_name += i.ToString();

					if(i % 3 == 0){
						_name += " End Point";
					}else{
						_name += " Control Point";
					}

					EditorGUILayout.BeginHorizontal ();
					if(GUILayout.Button(_name)){
						selectedIndex = i;
						EditorUtility.SetDirty(curveManager);
					}

					selectedCurve.SetControlPoint(i, EditorGUILayout.Vector3Field("",selectedCurve.GetControlPoint(i)));
					EditorGUILayout.EndHorizontal();
				}
				
				EditorGUILayout.EndScrollView();
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginHorizontal ();
				if (GUILayout.Button("Add Last")) {
					Undo.RecordObject(curveManager, "Add Point");
					selectedCurve.AddCurveLast();
					EditorUtility.SetDirty(curveManager);
				}

				if (GUILayout.Button("Remove Last")) {
					Undo.RecordObject(curveManager, "Remove Point");
					selectedCurve.RemoveCurve(selectedCurve.ControlPointCount - 1);
					EditorUtility.SetDirty(curveManager);
				}

				if (GUILayout.Button("Loop :" + selectedCurve.Loop.ToString())) {
					Undo.RecordObject(curveManager, "Loop");
					EditorUtility.SetDirty(curveManager);
					selectedCurve.Loop = !selectedCurve.Loop;
				}

				EditorGUILayout.EndHorizontal ();
			}

			if (selectedIndex >= 0 && selectedIndex < selectedCurve.ControlPointCount) {
				currentFoldout = EditorGUILayout.Foldout(currentFoldout, "Current Point");

				if(currentFoldout == true){
				
					DrawSelectedPointInspector();

					if(selectedIndex % 3 == 0){

						EditorGUILayout.BeginHorizontal ();
						if (GUILayout.Button("Add To Next")) {
							Undo.RecordObject(curveManager, "Add Point");
							selectedCurve.AddCurveNext(selectedIndex);
							EditorUtility.SetDirty(curveManager);
						}

						if (GUILayout.Button("Remove")) {
							Undo.RecordObject(curveManager, "Remove Point");
							selectedCurve.RemoveCurve(selectedIndex);
							EditorUtility.SetDirty(curveManager);
						}
						EditorGUILayout.EndHorizontal();

						positionCopytargetObject = (GameObject)EditorGUILayout.ObjectField("Copy Position By", positionCopytargetObject, typeof(GameObject), true);
						if(positionCopytargetObject != null){
							selectedCurve.SetControlPoint(selectedIndex, handleTransform.InverseTransformPoint(positionCopytargetObject.transform.position));
							positionCopytargetObject = null;
						}
					}
				}
			}
		}

		private void DrawSelectedPointInspector() {
			EditorGUI.BeginChangeCheck();
			Vector3 point = EditorGUILayout.Vector3Field("Position", selectedCurve.GetControlPoint(selectedIndex));
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(curveManager, "Move Point");
				EditorUtility.SetDirty(curveManager);
				selectedCurve.SetControlPoint(selectedIndex, point);
			}
		}

		private void OnSceneGUI () {
			curveManager = target as CurveManager;
			handleTransform = curveManager.transform;
			handleRotate = Tools.pivotRotation == PivotRotation.Local ?
			handleTransform.rotation : Quaternion.identity;
			
			if(visibleHandle == false){
				return;
			}
			
			if(selectedCurve == null || selectedCurve.ControlPointCount == 0){
				return;
			}
	
			Vector3 p0 = GetPointPosition(0);
			DrawHandle(p0, 0, colorEndPoint, endPointSize);

			for (int i = 1; i < selectedCurve.ControlPointCount; i += 3) {
				Vector3 p1 = GetPointPosition(i);
				Vector3 p2 = GetPointPosition(i + 1);
				Vector3 p3 = GetPointPosition(i + 2); 

				Handles.color = colorControlCurve;
				Handles.DrawLine(p0, p2);
				Handles.DrawLine(p1, p3);
				DrawHandle(p1, i, colorControlPoint, controlPointSize);
				DrawHandle(p2, i + 1, colorControlPoint, controlPointSize);
				DrawHandle(p3, i + 2, colorEndPoint, endPointSize);

				Handles.DrawBezier(p0, p3, p2, p1, colorCurve, null, 2f);
				p0 = p3;
			}
		}

		private Vector3 GetPointPosition(int index){
			return handleTransform.TransformPoint(selectedCurve.GetControlPoint(index));
		}

		private void DrawHandle (Vector3 point, int index, Color _pointColor, float _size) {
			Handles.color = _pointColor;
			if (Handles.Button(point, handleRotate, _size, _size, Handles.DotCap)) {
				selectedIndex = index;
				Repaint();
			}

			if (selectedIndex == index) {
				EditorGUI.BeginChangeCheck();
				point = Handles.DoPositionHandle(point, handleRotate);
				if (EditorGUI.EndChangeCheck()) {
					Undo.RecordObject(curveManager, "Move Point");
					EditorUtility.SetDirty(curveManager);
					selectedCurve.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
				}
			}
			
			//return point;
		}
	}
}