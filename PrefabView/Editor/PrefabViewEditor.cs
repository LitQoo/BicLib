//유니티 신규버전 프리팹 시스템 개선으로 인해 개발 중단
#if BICTWEEN_PREFABVIEW
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace BicUtil.PrefabView{
	[CustomEditor(typeof(PrefabView), true)]
	[CanEditMultipleObjects]
	[InitializeOnLoad]
	public class PrefabViewEditor : Editor 
	{

		#region  Hierarchy
		private static Vector2 offset = new Vector2(0, 2);

		static PrefabViewEditor()
		{
			EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
		}

		private static void HandleHierarchyWindowItemOnGUI(int _instanceID, Rect _selectionRect)
		{
			var _gameObject = EditorUtility.InstanceIDToObject(_instanceID) as GameObject;
			if (_gameObject != null)
			{
				var _prefabView = _gameObject.GetComponent<PrefabView>();
				if(_prefabView != null && _prefabView.gameObject.activeSelf == true)
				{
					Rect offsetRect = new Rect(_selectionRect.position + offset, _selectionRect.size);
					
					EditorGUI.LabelField(offsetRect, _gameObject.name, new GUIStyle()
					{
						normal = new GUIStyleState() { textColor = Color.yellow }
					}
					);

					if(EditorSceneManager.GetActiveScene().name == "PrefabViewEditor" && PrefabViewEditorManagerEditor.Manager.SelectedPrefabView != null && PrefabViewEditorManagerEditor.SelectedPrefabView.PrefabViewName == _prefabView.PrefabViewName){
						SetEnabledChildren(_prefabView, true, false);
					}else{
						SetEnabledChildren(_prefabView, false, false);
					}
				}
			}
		}
		#endregion

		public void SetEnabledChildren(bool _isEnabled){
			SetEnabledChildren(this.target, _isEnabled);
		}

		public static void SetEnabledChildren(Object _target, bool _isEnabled, bool _updateHierarchy = true){
			PrefabView _view = (_target as PrefabView);
			if(_view != null){
				foreach(Transform _child in _view.transform){
					if(_isEnabled == true){
						_child.gameObject.hideFlags = HideFlags.None;
					}else{
						_child.gameObject.hideFlags = HideFlags.HideInHierarchy;
					}
				}

				if(_updateHierarchy == true){
					EditorApplication.DirtyHierarchyWindowSorting();
				}
			}
		}

		public override void OnInspectorGUI()
		{

			base.OnInspectorGUI();
			
			if(GUILayout.Button("Reflash"))
			{
				applyValues();
			}
			
			if(EditorSceneManager.GetActiveScene().name == "PrefabViewEditor"){
				if((this.target as PrefabView).PrefabViewName == PrefabViewEditorManagerEditor.Manager.SelectedPrefabView.PrefabViewName &&  GUILayout.Button("Save And BackToScene"))
				{
					PrefabViewEditorManagerEditor.SaveAndBackToScene(this.target);
				}

				if(GUILayout.Button("Cancel And BackToScene")){
					PrefabViewEditorManagerEditor.BackToScene();
				}
			}else{
				
				if(GUILayout.Button("Edit PrefabView & Save Scene"))
				{
					PrefabViewEditorManagerEditor.OpenPrefabView(this.target, EditorSceneManager.GetActiveScene().path);
				}

				if(this.target != null){
					(this.target as PrefabView).ApplyValues();
				}
			}
		}

		
		private void applyValues(){
			(this.target as PrefabView).ApplyValues();
		}
	}
}
#endif