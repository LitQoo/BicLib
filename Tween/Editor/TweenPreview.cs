using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using System.Globalization;
using BicUtil.Tween;

public class TweenPreview : EditorWindow {
	[MenuItem("Window/Tween Preview")]
	public static void ShowWindow(){
		GetWindow<TweenPreview>("Tween Preview");
	}

	private void OnEnable()
	{
		EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;
		EditorApplication.update += EditorUpdate;
		
	}

	private void OnDisable()
	{
		EditorApplication.playModeStateChanged -= HandleOnPlayModeChanged;
		EditorApplication.update -= EditorUpdate;
		LeanTweenOnEditor.ClearUpdateCallback();
		LeanTweenOnEditor.tweenEmpty = null;
	}

    private void HandleOnPlayModeChanged(PlayModeStateChange _mode)
    {
		selectedPreview = null;
        PreviewList.Clear();
    }

    void EditorUpdate(){
		if(!Application.isPlaying){
			if(AnimationMode.InAnimationMode()){
				LeanTweenOnEditor.update();
				if(!LeanTweenOnEditor.isTweening()){
					AnimationMode.StopAnimationMode();
				}
			}
		}
	}
	
	private class PreviewInfo{
		public MethodInfo methodInfo;
		public TweenPreviewAttribute attribute;
		public Component targetObject;

		public PreviewInfo(MethodInfo _methodInfo, TweenPreviewAttribute _attribute, Component _targetObject){
			methodInfo = _methodInfo;
			attribute = _attribute;
			targetObject = _targetObject;
		}
		
	}

	private List<PreviewInfo> PreviewList = new List<PreviewInfo>();
	private Vector2 previewScrollPosition = new Vector2(0, 0);
	private PreviewInfo selectedPreview = null;
	private void OnGUI() {
		if(GUILayout.Button("Find All Tween Preview")){
			GUI.FocusControl(null);
			findPreview();
		}

		EditorGUILayout.BeginVertical(GUILayout.MinHeight(100f));
		previewScrollPosition = EditorGUILayout.BeginScrollView(previewScrollPosition, false, false); 
		for(int i = 0; i < PreviewList.Count; i++){
			if(GUILayout.Button(PreviewList[i].targetObject.name + "." + PreviewList[i].attribute.Name)){
				selectedPreview = PreviewList[i];
				GUI.FocusControl(null);
			}
		}

		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();

		if(selectedPreview != null){
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField(selectedPreview.targetObject.name + "." + selectedPreview.attribute.Name);
			var _params = selectedPreview.methodInfo.GetParameters();
			for(int i = 0; i < _params.Length; i++){
				EditorGUILayout.BeginHorizontal();
				if(_params[i].ParameterType == typeof(string)){
					selectedPreview.attribute.Params[i] = EditorGUILayout.TextField(_params[i].Name, (string)selectedPreview.attribute.Params[i]);
				}else if(_params[i].ParameterType == typeof(int)){
					selectedPreview.attribute.Params[i] = EditorGUILayout.IntField(_params[i].Name, (int)selectedPreview.attribute.Params[i]);
				}else if(_params[i].ParameterType == typeof(float)){
					selectedPreview.attribute.Params[i] = EditorGUILayout.FloatField(_params[i].Name, (float)selectedPreview.attribute.Params[i]);
				}
				
				EditorGUILayout.EndHorizontal();
			}

			if(GUILayout.Button("Play")){
				GUI.FocusControl(null);
				AnimationMode.StartAnimationMode();
				Debug.Log("start animationmode");
				selectedPreview.methodInfo.Invoke(selectedPreview.targetObject, selectedPreview.attribute.Params);
			}

			if(GUILayout.Button("Edit Script")){
				var monoscript = MonoScript.FromMonoBehaviour(selectedPreview.targetObject as MonoBehaviour);
      			var scriptPath = AssetDatabase.GetAssetPath(monoscript);
				
      			UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(scriptPath, 0);
			}
		}

	}

	private void findPreview(){
		PreviewList.Clear();

		object[] _allObject = GameObject.FindSceneObjectsOfType(typeof (GameObject));
		LeanTweenOnEditor.tweenEmpty = _allObject[0] as GameObject;

		foreach (object _object in _allObject)
		{
			GameObject _targetObject = (GameObject) _object;
			var _components = _targetObject.GetComponents(typeof(MonoBehaviour));
			foreach(var _component in _components){
				var _methods = _component.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				foreach(var _method in _methods){
					TweenPreviewAttribute _attribute = (TweenPreviewAttribute)_method.GetCustomAttributes(typeof(TweenPreviewAttribute), true).FirstOrDefault();
					
					if(_attribute != null){
						PreviewList.Add(new PreviewInfo(_method, _attribute, _component));
					}
				}
			}
		}
	}
}
