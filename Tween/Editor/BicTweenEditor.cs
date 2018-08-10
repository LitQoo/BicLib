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
		private List<TweenModel> selectedTweens = new List<TweenModel>();
		private Vector2 pointsScrollPosition;
		private TweenModel selectedGroup{
			get{
				if(selectedTweenPool == null || selectedGroupIndex < 0){
					return null;
				}else{
					try{
						return selectedTweenPool.GetGroup(selectedGroupIndex);
					}catch(System.Exception){
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
			if (timeline == null) {
				timeline = new Timeline ();
			}
			
			EditorApplication.update += EditorUpdate;
			EditorApplication.update += BicTween.UpdateDeltaTime;
			timeline.onSettingsGUI = onSettings;
			timeline.onTimelineGUI = drawNods;
			timeline.onPlay = preview;
			timeline.onRecord = record;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
            selectedTweens.Clear();
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
			handleCopyPaste();

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

            if(selectedTweens != null){
                DrawHandleControl(selectedTweens);
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
				if(selectedTweenPool != null && selectedTweenPool.GroupIdList != null){
					for(int i = 0; i < selectedTweenPool.GroupIdList.Count; i++){
						int _index = i;
						toolsMenu.AddItem (new GUIContent (selectedTweenPool.GetGroup(_index).Name), false, delegate() {
							selectedGroupIndex = _index;
							selectTween(selectedTweenPool.GetGroup(_index));
							isGroupRemove = false;
						});
					}
				}
				
				toolsMenu.AddItem (new GUIContent ("[New Group]"), false, addGroup);

				GUIUtility.keyboardControl = 0;
				toolsMenu.DropDown (new Rect (3, 37, 0, 0));
				EditorGUIUtility.ExitGUI ();
			}
			GUILayout.EndHorizontal ();

            if(selectedTweens != null && selectedTweenPool != null){
				EditorGUILayout.BeginVertical();
				pointsScrollPosition = EditorGUILayout.BeginScrollView(pointsScrollPosition, false, false); 

                DrawSetting(selectedTweens);
                
                EditorUtility.SetDirty(selectedTweenPool);
				EditorGUILayout.EndScrollView();
				EditorGUILayout.EndVertical();
            }
		}

		private void drawNods(Rect position){
			if (selectedGroup == null) {
				selectedTweens.Clear();
				return;
			}

			var _rect = DrawNode(selectedGroup, Vector2.zero, timeline);
			timeline.expandView = new Vector2(_rect.width, _rect.height);
			DoEvents ();
		}

		private void DoEvents(){
			if(OnClicked(selectedGroup, Event.current) == true){
				GUI.FocusControl("");
				Repaint();
			}else{
				movingPoint = Rect.zero;
			}

			if(movingPoint != Rect.zero){
				Handles.DrawSolidRectangleWithOutline(movingPoint, Color.magenta, Color.magenta);
			}

		}

		private List<TweenModel> copiedTweens = new List<TweenModel>();
		private void handleCopyPaste()
		{
			if (Event.current.type == EventType.KeyDown && (Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.Command))
			{
				if (Event.current.keyCode == KeyCode.C)
				{
					Event.current.Use();
					copiedTweens = new List<TweenModel>(selectedTweens.ToArray());
				}
				else if (Event.current.keyCode == KeyCode.V)
				{
					if(copiedTweens.Count == 0 || selectedTweens.Count != 1){
						return;
					}
					
					if(selectedTweens[0].IsGrouped == true){
						Event.current.Use();

						for(int i = 0; i < copiedTweens.Count; i++){
							selectedTweens[0].AddChild(copiedTweens[i].Copy(selectedTweenPool));
						}
					}

				}
			}
		}
		
		private void selectTween(TweenModel _tween){
			selectedTweens.Clear();
			selectedTweens.Add(_tween);
		}

		private void unselectTween(TweenModel _tween){
			selectedTweens.Remove(_tween);
		}

		private void addToSelectTween(TweenModel _tween){
			selectedTweens.Add(_tween);
		}

		private void popupMenu(TweenModel _tween){
			GenericMenu genericMenu = new GenericMenu ();
			genericMenu.AddItem (new GUIContent ("Remove"), false,delegate() {
				selectedTweenPool.RemoveTween(selectedGroup, _tween);
				selectedTweens.Clear();
				Repaint();
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
					selectedTweens.Clear();
					isGroupRemove = false;

					if(selectedTweenPool != null && selectedTweenPool.GroupIdList != null && selectedTweenPool.GroupIdList.Count > 0){
						selectedGroupIndex = selectedTweenPool.GroupIdList.Count - 1;
					}else{
						selectedGroupIndex = -1;
					}

					Repaint ();
				}
			}
		}

		private void addTweenPool(GameObject _gameObject){
			selectedTweenPool = _gameObject.GetComponent<TweenPool> ();
			if(selectedTweenPool  == null) {
				selectedTweenPool = _gameObject.AddComponent<TweenPool>();
				selectedTweenPool.GroupIdList = new List<int>();
				selectedTweenPool.IsLocked = true;
				EditorUtility.SetDirty(_gameObject);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			}
		}

        private void addGroup(){
            selectedTweenPool.AddGroup();
            selectedGroupIndex = selectedTweenPool.GroupIdList.Count - 1;
			selectTween(selectedTweenPool.GetGroup(selectedGroupIndex));
            EditorUtility.SetDirty(selectedTweenPool);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
												Repaint();
        }

		private void preview(bool _isPlaying){
			if(selectedTweens.Count == 1 && _isPlaying == true){
				bool _applyInitialInformations = selectedTweens[0] == selectedGroup;
				selectedTweens[0].Play(_applyInitialInformations);
				UnityEditor.AnimationMode.StartAnimationMode();
			}else if(_isPlaying == false){
				UnityEditor.AnimationMode.StopAnimationMode();
			}
		}

		private void record(bool _isRecord){
			if(_isRecord == true){
				UnityEditor.AnimationMode.StartAnimationMode();
			}else{
				UnityEditor.AnimationMode.StopAnimationMode();
			}
		}

		void EditorUpdate(){
			if(timeline.isPlaying == false){
				return;
			}

			selectedTweenPool.Update();

			if(selectedTweenPool.MaxPlayingIndex < 0){
				timeline.isPlaying = false;
				Repaint();
			}
		}

		#region Setting
        private Dictionary<TweenType, MethodInfo> drawNodeCache = new Dictionary<TweenType, MethodInfo>();
        private Rect movingPoint;
		private TweenModel movingTarget;
		private bool isPreMoving = false;
		public Rect DrawNode(TweenModel _tween, Vector2 _startPosition, Timeline _timeline){
            if(_tween == null){
                Debug.LogWarning("[BicTween] tween is null");
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

			Debug.LogWarning("[BicTween] did not find draw node for type " + _tween.Type.ToString());
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
		private bool isGroupRemove = false;
		private int selectedGroupSettingTab;
		public void DrawSetting(List<TweenModel> _tweens){
			bool _isGroupSetting = false;
			if(_tweens.Count == 1 && selectedGroup == _tweens[0]){
				selectedGroupSettingTab = GUILayout.Toolbar (selectedGroupSettingTab, new string[] {"Setting", "Initial"});
				_isGroupSetting = true;
			}

			if(_isGroupSetting == true){
				if(selectedGroupSettingTab == 0){
					drawDefaultSetting(_tweens);
					drawGroupSetting();
				}else{
					if(selectedTweenPool.InitialList == null){
						selectedTweenPool.InitialList = new List<InitialInformations>();
					}
					
					drawInitial();
				}
			}else{
				drawDefaultSetting(_tweens);

				if(_tweens.Count != 1){
					return;
				}
			}

			if(drawSettingCache.ContainsKey(_tweens[0].Type)){
				drawSettingCache[_tweens[0].Type].Invoke(this, new object[]{_tweens[0]});
				return;
			}

			var _methods = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach(var _method in _methods){
				var _attributes = _method.GetCustomAttributes(typeof(TweenEditorDrawSetting), true);
				foreach(TweenEditorDrawSetting _attribute in _attributes){
					if(_attribute != null && _attribute.Type == _tweens[0].Type){
						drawSettingCache[_tweens[0].Type] = _method;
						_method.Invoke(this, new object[]{_tweens[0]});
						return;
					}
				}
			}
        }

        private Dictionary<TweenType, MethodInfo> drawHandleControlCache = new Dictionary<TweenType, MethodInfo>();
		public void DrawHandleControl(List<TweenModel> _tweens){
			
			for(int i = 0; i < _tweens.Count; i++){
				var _tween = _tweens[i];
				if(drawHandleControlCache.ContainsKey(_tween.Type)){
					drawHandleControlCache[_tween.Type].Invoke(this, new object[]{_tween});
					continue;
				}
				
				bool isContinue = false;
				var _methods = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				foreach(var _method in _methods){
					var _attributes = _method.GetCustomAttributes(typeof(TweenEditorDrawHandleControl), true);
					foreach(TweenEditorDrawHandleControl _attribute in _attributes){
						if(_attribute != null && _attribute.Type == _tween.Type){
							drawHandleControlCache[_tween.Type] = _method;
							_method.Invoke(this, new object[]{_tween});
							EditorUtility.SetDirty(selectedTweenPool);
							isContinue = true;
							break;
						}
					}

					if(isContinue == true){
						break;
					}
				}

			}
        }

		private void drawGroupSetting(){
			if(isGroupRemove == true){

				var _color = GUI.backgroundColor;
				GUI.backgroundColor = Color.red;
				if(GUILayout.Button("Remove")){
					selectedTweenPool.RemoveGroup(selectedGroup);
					selectedGroupIndex = -1;
					Repaint();
				}

				GUI.backgroundColor = _color;
			
				if(GUILayout.Button("Cancel Remove")){
					isGroupRemove = false;
				}
			}else{
				if(GUILayout.Button("Refind All Targets")){
					selectedTweenPool.RefindTargetObject(selectedGroup);
				}

				var _color = GUI.backgroundColor;
				GUI.backgroundColor = Color.red;
				if(GUILayout.Button("Remove Group")){
					isGroupRemove = true;
				}

				GUI.backgroundColor = _color;
			}
		}

        private void drawDefaultSetting(List<TweenModel> _tweens){
			if(_tweens.Count == 1){
				var _tween = _tweens[0];
				EditorGUILayout.LabelField("ID", _tween.Id.ToString());
				_tween.Name = EditorGUILayout.TextField("Name", _tween.Name);

				EditorGUILayout.BeginHorizontal();
				_tween.TargetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", _tween.TargetObject, typeof(GameObject), true);
				if(GUILayout.Button("S", GUILayout.Width(20))){
					_tween.TargetObject = Selection.activeGameObject;
				}
				EditorGUILayout.EndHorizontal();

				if(_tween.TargetObject != null){
					if(string.IsNullOrEmpty(_tween.editor_targetPath)){
						_tween.editor_targetPath = selectedTweenPool.GetTargetPath(_tween.TargetObject);
					}
					
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Target Path", _tween.editor_targetPath);
					if(GUILayout.Button("Find")){
						var _findTarget = selectedTweenPool.GetTargetObject(_tween.editor_targetPath);
						if(_findTarget != null){
							_tween.TargetObject = _findTarget;
						}
					}
					EditorGUILayout.EndHorizontal();
				}
				
				_tween.Time = EditorGUILayout.FloatField("Time", _tween.Time);
				_tween.RepeatCount = EditorGUILayout.IntField("Repeat Count", _tween.RepeatCount);
				_tween.EaseType = (EaseType)EditorGUILayout.EnumPopup("Ease Type", _tween.EaseType);
				_tween.TimeType = (TimeType)EditorGUILayout.EnumPopup("Time Type", _tween.TimeType);
				EditorGUILayout.Space();
				_tween.Type = (TweenType)EditorGUILayout.EnumPopup("Tween Type", _tween.Type);
				UpdateFuncs.SetUpdateFunc(_tween);
			}else if(_tweens.Count > 1){
				GameObject _targetObject = _tweens[0].TargetObject;
				bool _isTargetObjectSame = true;
				for(int i = 1; i < _tweens.Count; i++){
					if(_targetObject != _tweens[i].TargetObject){
						_isTargetObjectSame = false;
					}
				}

				if(_isTargetObjectSame == false){
					_targetObject = null;
				}

				_targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", _targetObject, typeof(GameObject), true);

				if(_targetObject != null){
					for(int i = 0; i < _tweens.Count; i++){
						_tweens[i].TargetObject = _targetObject;
					}
				}
			}
        }

		private void drawInitial(){
			if(selectedTweenPool.InitialList.Exists(_initial=>_initial.TargetTweenId == selectedGroup.Id)){
				if(GUILayout.Button("Apply Initial Infomations")){
					selectedTweenPool.ApplyInitialInformation(selectedGroup.Id);
				}

				var _initialInformations = selectedTweenPool.GetInitialInformations(selectedGroup.Id); 
				for(int i = 0; i < _initialInformations.List.Count; i++){
					EditorGUILayout.BeginHorizontal();
					var _initialInfo = _initialInformations.List[i];
					_initialInfo.TargetObject = (GameObject)EditorGUILayout.ObjectField(_initialInfo.TargetObject, typeof(GameObject), true);
					if(GUILayout.Button("S", GUILayout.Width(20))){
						_initialInfo.TargetObject = Selection.activeGameObject;
					}

					_initialInfo.Type = (InitialType)EditorGUILayout.EnumPopup(_initialInfo.Type);
					_initialInfo.Value = EditorGUILayout.Vector4Field("", _initialInfo.Value);
					EditorGUILayout.EndHorizontal();
				}

				if(GUILayout.Button("Add Initial Information")){
					_initialInformations.Add();
				}
			}else{
				if(GUILayout.Button("Create Initial Informations")){
					selectedTweenPool.InitialList.Add(new InitialInformations(selectedGroup.Id));
					EditorUtility.SetDirty(selectedTweenPool);
				}
			}
			
		}

		#endregion
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	sealed class TweenEditorDrawSetting : Attribute{
		public TweenType Type{get; private set;}
		public TweenEditorDrawSetting(TweenType _type){
			Type = _type;
		}
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	sealed class TweenEditorDrawHandleControl : Attribute{
		public TweenType Type{get; private set;}
		public TweenEditorDrawHandleControl(TweenType _type){
			Type = _type;
		}
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	sealed class TweenEditorDrawNode : Attribute{
		public TweenType Type{get; private set;}
		public TweenEditorDrawNode(TweenType _type){
			Type = _type;
		}
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	sealed class TweenEditorClickedNode : Attribute{
		public TweenType Type{get; private set;}
		public TweenEditorClickedNode(TweenType _type){
			Type = _type;
		}
	}
}