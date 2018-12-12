//유니티 신규버전 프리팹 시스템 개선으로 인해 개발 중단
#if BICTWEEN_PREFABVIEW
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BicUtil.PrefabView
{
    [CustomEditor(typeof(PrefabViewEditorManager), true)]
	[CanEditMultipleObjects]
	[InitializeOnLoad]
	public class PrefabViewEditorManagerEditor : Editor {
		static public PrefabView SelectedPrefabView;
		static public string LastScenePath;
		static public  PrefabViewEditorManager Manager{
			get{
				return GameObject.Find("Manager").GetComponent<PrefabViewEditorManager>();
			}
		}
		
		static PrefabViewEditorManagerEditor(){
			EditorSceneManager.sceneOpened += loadPrefab;
		}

        private static void loadPrefab(Scene scene, OpenSceneMode mode)
        {
			var _activeScene = EditorSceneManager.GetActiveScene();

			if(_activeScene.name == "PrefabViewEditor"){

				GameObject _editLayer = GameObject.Find("UICanvas");
				
				if(_editLayer != null && SelectedPrefabView != null){
					Manager.SelectedPrefabView = SelectedPrefabView;
					Manager.LastScenePath = LastScenePath;
                    UnityEngine.Object _object = PrefabUtility.InstantiatePrefab(Manager.SelectedPrefabView.gameObject);
					GameObject _gameobject = GameObject.Find(_object.name);
					Manager.SelectedPrefabView = _gameobject.GetComponent<PrefabView>();

					if(_gameobject != null){
						var _rect = _gameobject.GetComponent<RectTransform>();
						if(_rect != null){
							_rect.SetParent(_editLayer.GetComponent<RectTransform>());
						}else{
							_gameobject.transform.parent = _editLayer.transform;
						}

						SetExpandedRecursive(_gameobject, true);
					}else{
						Debug.Log("fail add");
					}
				}
			}
	    }

		public static void OpenPrefabView(UnityEngine.Object _targetObject, string _scenePath){
			PrefabViewEditorManagerEditor.SelectedPrefabView = (PrefabUtility.GetCorrespondingObjectFromSource(_targetObject) as PrefabView);
			PrefabViewEditorManagerEditor.LastScenePath = _scenePath;
			EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
			openEditorScene();
		}

		static private void openEditorScene(){
			var _editorGuid = AssetDatabase.FindAssets("PrefabViewEditor");
			foreach(var _guid in _editorGuid){
				var _path = AssetDatabase.GUIDToAssetPath(_guid);
				if(_path.EndsWith("unity") == true){
					EditorSceneManager.OpenScene(_path);
					return;
				}
			}
		}

		static public void SaveAndBackToScene(UnityEngine.Object _targetPrefabObject){

			EditorUtility.DisplayProgressBar("Saving","Update Prefab",0.0f);

            // 1. save prefab; 
            UnityEngine.Object _prefab = PrefabUtility.GetCorrespondingObjectFromSource(_targetPrefabObject);

			var _editedPrefab = PrefabUtility.ReplacePrefab((_targetPrefabObject as PrefabView).gameObject, _prefab, ReplacePrefabOptions.ConnectToPrefab);

			// 2. prefab listing
			var _paths = AssetDatabase.GetAllAssetPaths();
			float _pathLength = (float)_paths.Length;
			float _pathCounter = 0f;
			foreach(var _path in _paths){
				_pathCounter++;
				
				
				if(_path.EndsWith(".prefab") == false){
					continue;
				}

				GameObject _object = AssetDatabase.LoadAssetAtPath<GameObject>(_path);
				if(_object == null){
					continue;
				}


				var _prefabView = _object.GetComponent<PrefabView>();
				if(_prefabView == null){
					continue;
				}

				EditorUtility.DisplayProgressBar("Update Nested Prefab", _path, _pathCounter / _pathLength);


				var _updateTargetPrefab = (PrefabUtility.InstantiatePrefab(_object) as GameObject);
				var _updateTargetPrefabView = _updateTargetPrefab.GetComponent<PrefabView>();
				var _updateTargetChildList = findPrefabViewInChildren(_updateTargetPrefabView, Manager.SelectedPrefabView.PrefabViewName);
				
				if(_updateTargetChildList.Count <= 0){
					GameObject.DestroyImmediate(_updateTargetPrefab);
					continue;
				}


				foreach(var _updateTargetChild in _updateTargetChildList){
					var _replacePrefab = PrefabUtility.InstantiatePrefab(_editedPrefab) as GameObject;
					
					//update transform
					if(_replacePrefab.GetComponent<RectTransform>() != null){
						_replacePrefab.GetComponent<RectTransform>().SetParent(_updateTargetChild.transform.parent);
						EditorUtility.CopySerialized(_updateTargetChild.GetComponent<RectTransform>(), _replacePrefab.GetComponent<RectTransform>());
					}else{
						_replacePrefab.transform.parent = _updateTargetChild.transform.parent;
						EditorUtility.CopySerialized(_updateTargetChild.GetComponent<Transform>(), _replacePrefab.GetComponent<Transform>());
					}
					
					_replacePrefab.transform.SetSiblingIndex(_updateTargetChild.transform.GetSiblingIndex());

					//copy parameter
					var _fields = _updateTargetChild.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
					foreach(var _field in _fields){
						var _attributes = _field.GetCustomAttributes(typeof(PrefabViewParameterAttribute), true);
						foreach(PrefabViewParameterAttribute _attribute in _attributes){
							if(_attribute != null){
								try{
									_field.SetValue(_replacePrefab.GetComponent(_updateTargetChild.GetType()), _field.GetValue(_updateTargetChild));
								}catch(Exception e){
									Debug.Log("setvalue error => " + _field.Name + " e: " + e.Message);
								}
							}
						}
					}

					//linking update
					ReplaceReferences(UnityEngine.Object.FindObjectsOfType<GameObject>(), _updateTargetChild, _replacePrefab.GetComponent<PrefabView>());
					
					GameObject.DestroyImmediate(_updateTargetChild.gameObject);

					//save prefab
					PrefabUtility.ReplacePrefab(_updateTargetPrefab, PrefabUtility.GetCorrespondingObjectFromSource(_updateTargetPrefab), ReplacePrefabOptions.ConnectToPrefab);
					
					GameObject.DestroyImmediate(_updateTargetPrefab);

				}

			}

			EditorUtility.ClearProgressBar();

			// finish. back to scene
			BackToScene();
		}

		public static void BackToScene(){
			EditorSceneManager.OpenScene(Manager.LastScenePath, OpenSceneMode.Single);
		}

		public static void ReplaceReferences(GameObject[] objects, PrefabView _origin, PrefabView _new)
		{
			foreach (var go in objects)
			{
				var components = go.GetComponents<Component> ();

				foreach (var c in components)
				{
					var so = new SerializedObject(c);
					var sp = so.GetIterator();
					bool _isModify = false;
					while (sp.NextVisible(true))
					{
						if (sp.propertyType == SerializedPropertyType.ObjectReference)
						{
							if(sp.objectReferenceValue != null && (sp.objectReferenceValue as MonoBehaviour) != null){
								var _newId = (sp.objectReferenceValue as MonoBehaviour).gameObject.GetInstanceID();
								var _originId = _origin.gameObject.GetInstanceID();

								if(_newId == _originId){
									sp.objectReferenceValue = _new.gameObject.GetComponent(sp.objectReferenceValue.GetType());
									_isModify = true;
									
								}
							}
						}
					}

					if(_isModify == true){
						so.ApplyModifiedProperties();
					}
				}
			}
		}

		private static List<PrefabView> findPrefabViewInChildren(PrefabView _parent, string _findName){
			List<PrefabView> _result = new List<PrefabView>();
			foreach(Transform _child in _parent.transform){
				var _prefabView = _child.gameObject.GetComponent<PrefabView>();
				if(_prefabView != null){
					if(_prefabView.PrefabViewName == _findName){
						_result.Add(_prefabView);
					}else{
						var _list = findPrefabViewInChildren(_prefabView, _findName);
						if(_list.Count > 0){
							_result.AddRange(_list);
						}
					}
				}
			}

			return _result;
		}


		#region hierarchy contol
		public static void SetExpandedRecursive(GameObject _gameObject, bool _isExpand)
		{
			Selection.activeGameObject = _gameObject;
			var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
			var methodInfo = type.GetMethod("SetExpandedRecursive");
			EditorApplication.ExecuteMenuItem("Window/Hierarchy");
			var window = EditorWindow.focusedWindow;
			methodInfo.Invoke(window, new object[] { _gameObject.GetInstanceID(), _isExpand });
		}
		#endregion


    }
}
#endif