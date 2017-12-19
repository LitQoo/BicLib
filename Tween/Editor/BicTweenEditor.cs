using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BicUtil.CustomTimeLine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BicUtil.Tween
{
    public partial class BicTweenEditor : EditorWindow {
		private Timeline timeline;
		private GameObject selectedGameObject;
		private TweenPool selectedTweenPool;
		private int selectedGroupIndex;
		private TweenModel selectedTween;
		private Vector2 pointsScrollPosition;
		private TweenModel selectedGroup{
			get{
				if(selectedTweenPool == null || selectedGroupIndex < 0){
					return null;
				}else{
					try{
						return selectedTweenPool.GetGroup(selectedGroupIndex);
					}catch(System.Exception e){
						return null; 
					}
				}
			}
		}

		[MenuItem("Window/BicTween Editor")]
		public static void ShowWindow(){
			BicTweenEditor _window = GetWindow<BicTweenEditor>("BicTween Editor");
			UnityEngine.Object.DontDestroyOnLoad(_window);
		}

		private void OnEnable()
		{
			Debug.Log("onenable bictween");
			if (timeline == null) {
				timeline = new Timeline ();
			}
			
			EditorApplication.update += EditorUpdate;
			EditorApplication.update += BicTween.UpdateDeltaTime;
			timeline.onSettingsGUI = onSettings;
			timeline.onTimelineGUI = drawNods;
			timeline.onPlay = preview;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
			
            selectedTween = null;
            selectedGroupIndex = -1;
			if(selectedGameObject == null){
				OnSelectionChange ();
			}
		}

		private void OnDisable()
		{
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			EditorApplication.update -= EditorUpdate;
			EditorApplication.update -= BicTween.UpdateDeltaTime;
		}

		private void OnGUI(){
			bool enabled = GUI.enabled;
			GUI.enabled = selectedGameObject != null && !Application.isPlaying;
			if(selectedTweenPool == null && selectedGameObject != null){
				drawNewAnimation();
			}else{
				timeline.DoTimeline (new Rect(0,0,this.position.width,this.position.height));
			}
			GUI.enabled = enabled;
		}
        
        void OnSceneGUI( SceneView sceneView )
        {
            if( SceneView.lastActiveSceneView == null)
            {
                return;
            }

            if(selectedTween != null){
                DrawHandleControl(selectedTween);
            }
        }
		
		private void drawNewAnimation(){

			GUILayout.BeginArea (new Rect((Screen.width/2)-100, (Screen.height/2 - 30) , 200, 100));
			if(GUILayout.Button ("Create Animation to " + selectedGameObject.name)){				
				addTweenPool(selectedGameObject);
			}

			GUILayout.EndArea();
		}

		private void onSettings(float width){
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (selectedGroup != null ? selectedGroup.Name : "[None Selected]", EditorStyles.toolbarDropDown, GUILayout.Width (width))) {
				GenericMenu toolsMenu = new GenericMenu ();
				if(selectedTweenPool != null){
					for(int i = 0; i < selectedTweenPool.GroupIdList.Count; i++){
						int _index = i;
						toolsMenu.AddItem (new GUIContent (selectedTweenPool.GetGroup(_index).Name), false, delegate() {
							selectedGroupIndex = _index;
						});
					}
				}
				
				toolsMenu.AddItem (new GUIContent ("[New Group]"), false, addGroup);

				GUIUtility.keyboardControl = 0;
				toolsMenu.DropDown (new Rect (3, 37, 0, 0));
				EditorGUIUtility.ExitGUI ();
			}
			GUILayout.EndHorizontal ();

            if(selectedTween != null){
				EditorGUILayout.BeginVertical();
				pointsScrollPosition = EditorGUILayout.BeginScrollView(pointsScrollPosition, false, false); 

                DrawSetting(selectedTween);
                
                EditorUtility.SetDirty(selectedTweenPool);
				EditorGUILayout.EndScrollView();
				EditorGUILayout.EndVertical();
            }
		}

		private void drawNods(Rect position){
			if (selectedGroup == null) {
				selectedTween = null;
				return;
			}

			DrawNode(selectedGroup, Vector2.zero, timeline);
			DoEvents ();
		}

		private void DoEvents(){
			if(OnClicked(selectedGroup, Event.current) == true){
				Repaint();
			}
		}

		private void selectTween(TweenModel _tween){
			selectedTween = _tween;
		}

		private void popupMenu(TweenModel _tween){
			GenericMenu genericMenu = new GenericMenu ();
			genericMenu.AddItem (new GUIContent ("Remove"), false,delegate() {

			});
			genericMenu.ShowAsContext ();
		}

        private void openToAddTweenMenu(TweenModel _tween){
            GenericMenu genericMenu = new GenericMenu ();
            genericMenu.AddItem (new GUIContent ("Single"), false,delegate() {
				var _newTween = BicTween.MoveByBezier(null, new Vector3[]{new Vector3(0, 0, 0), new Vector3(0, 100, 0), new Vector3(100, 100, 0), new Vector3(200, 200, 0)}, 3f, selectedTweenPool);
			   
			    selectedTweenPool.AddTween(_tween, _newTween);
				EditorUtility.SetDirty(selectedTweenPool);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            });
            genericMenu.AddItem (new GUIContent ("Sequance"), false,delegate() {
                selectedTweenPool.AddTween(_tween, BicTween.Sequance(selectedTweenPool));
				EditorUtility.SetDirty(selectedTweenPool);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            });
            genericMenu.AddItem (new GUIContent ("Spawn"), false,delegate() {
                selectedTweenPool.AddTween(_tween, BicTween.Spawn(selectedTweenPool));
				EditorUtility.SetDirty(selectedTweenPool);
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            });
            genericMenu.ShowAsContext ();
        }

		private void OnSelectionChange(){
			timeline.Stop();
			selectedGameObject = Selection.activeGameObject;
			if (selectedGameObject != null) {
				var _selectedPool = selectedGameObject.GetComponent<TweenPool> ();
				if(_selectedPool != null && selectedTweenPool != _selectedPool){
					selectedTweenPool = _selectedPool;
					selectedTween = null;

					if(selectedTweenPool != null){
						selectedGroupIndex = selectedTweenPool.GroupIdList.Count - 1;
					}else{
						selectedGroupIndex = -1;
					}


					Repaint ();
				}
			}
		}

		private void addTweenPool(GameObject _gameObject){
			if (_gameObject.GetComponent<TweenPool> () == null) {
				selectedTweenPool = _gameObject.AddComponent<TweenPool>();
				selectedTweenPool.GroupIdList = new List<int>();
				EditorUtility.SetDirty(_gameObject);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}

        private void addGroup(){
            selectedTweenPool.AddGroup();
            selectedGroupIndex = selectedTweenPool.GroupIdList.Count - 1;
            EditorUtility.SetDirty(selectedTweenPool);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

		private void preview(bool _isPlaying){
			if(selectedTween != null && _isPlaying == true){
				selectedTween.Play();
			}
		}

		void EditorUpdate(){
			if(timeline.isPlaying == false){
				return;
			}

			selectedTweenPool.Update();
		}

		#region Setting
        private Dictionary<TweenType, MethodInfo> drawNodeCache = new Dictionary<TweenType, MethodInfo>();
        public Rect DrawNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
            if(_tween == null){
                Debug.LogWarning("tween is null");
                return new Rect(0, 0, 0, 0);
            }

			if(drawNodeCache.ContainsKey(_tween.Type)){
				return (Rect)drawNodeCache[_tween.Type].Invoke(this, new object[]{_tween, _startPosition, _timeline});
			}

			var _methods = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach(var _method in _methods){
				var _attributes = _method.GetCustomAttributes(typeof(TweenEditorDrawNode), true);
				foreach(TweenEditorDrawNode _attribute in _attributes){
					if(_attribute != null && _attribute.Type == _tween.Type){
						drawNodeCache[_tween.Type] = _method;
						return (Rect)_method.Invoke(this, new object[]{_tween, _startPosition, _timeline});
					}
				}
			}

			Debug.LogWarning("did not find draw node for type " + _tween.Type.ToString());
			return new Rect(0, 0, 0, 0);
        }

		public bool OnClicked(TweenModel _tween, Event _event){
            
            switch(_tween.Type){
            case TweenType.Spawn:
            case TweenType.Sequance:
                return onClickedNodeGroup(_tween, _event);
            default:
                return onClickedNodeSingle(_tween, _event);
            }
        }

        private Dictionary<TweenType, MethodInfo> drawSettingCache = new Dictionary<TweenType, MethodInfo>();
        public void DrawSetting(TweenModel _tween){
			 drawDefaultSetting(_tween);

            if(_tween.TargetObject == null){
                return;
            }

			if(drawSettingCache.ContainsKey(_tween.Type)){
				drawSettingCache[_tween.Type].Invoke(this, new object[]{_tween});
				return;
			}

			var _methods = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach(var _method in _methods){
				var _attributes = _method.GetCustomAttributes(typeof(TweenEditorDrawSetting), true);
				foreach(TweenEditorDrawSetting _attribute in _attributes){
					if(_attribute != null && _attribute.Type == _tween.Type){
						drawSettingCache[_tween.Type] = _method;
						_method.Invoke(this, new object[]{_tween});
						return;
					}
				}
			}
        }

        private Dictionary<TweenType, MethodInfo> drawHandleControlCache = new Dictionary<TweenType, MethodInfo>();
		public void DrawHandleControl(TweenModel _tween){
			
			if(drawHandleControlCache.ContainsKey(_tween.Type)){
				drawHandleControlCache[_tween.Type].Invoke(this, new object[]{_tween});
				return;
			}
			
			var _methods = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach(var _method in _methods){
				var _attributes = _method.GetCustomAttributes(typeof(TweenEditorDrawHandleControl), true);
				foreach(TweenEditorDrawHandleControl _attribute in _attributes){
					if(_attribute != null && _attribute.Type == _tween.Type){
						drawHandleControlCache[_tween.Type] = _method;
						_method.Invoke(this, new object[]{_tween});
						return;
					}
				}
			}
        }

        private void drawDefaultSetting(TweenModel _tween){
            _tween.Name = EditorGUILayout.TextField("Name", _tween.Name);
            _tween.TargetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", _tween.TargetObject, typeof(GameObject), true);
            _tween.Time = EditorGUILayout.FloatField("Time", _tween.Time);
            _tween.RepeatCount = EditorGUILayout.IntField("Repeat Count", _tween.RepeatCount);
			_tween.EaseType = (EaseType)EditorGUILayout.EnumPopup("Ease Type", _tween.EaseType);
			_tween.SetEase(EaseFuncs.GetFunc(_tween.EaseType));
			EditorGUILayout.Space();
            _tween.Type = (TweenType)EditorGUILayout.EnumPopup("Tween Type", _tween.Type);
            _tween.UpdateFunc = UpdateFuncs.GetFunc(_tween.Type);
        }

		#endregion
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	sealed class TweenEditorDrawSetting : Attribute{
		public TweenType Type{get;}
		public TweenEditorDrawSetting(TweenType _type){
			Type = _type;
		}
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	sealed class TweenEditorDrawHandleControl : Attribute{
		public TweenType Type{get;}
		public TweenEditorDrawHandleControl(TweenType _type){
			Type = _type;
		}
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	sealed class TweenEditorDrawNode : Attribute{
		public TweenType Type{get;}
		public TweenEditorDrawNode(TweenType _type){
			Type = _type;
		}
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	sealed class TweenEditorClickedNode : Attribute{
		public TweenType Type{get;}
		public TweenEditorClickedNode(TweenType _type){
			Type = _type;
		}
	}
}