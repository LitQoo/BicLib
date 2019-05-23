using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.SingletonBase;
using System;
using BicUtil.ClassInitializer;

namespace BicUtil.UIFlow
{
	public class UIFlow : MonoBehaviourHardBase<UIFlow> {
		#region  Static
		private Dictionary<Type, IUIFlowObject> uiObjects = new Dictionary<Type, IUIFlowObject>();

		public void RegisterUI(IUIFlowObject _object){
			uiObjects[_object.GetType()] = _object;
		}

		public IUIFlowObject GetUI(Type _type){
			return uiObjects[_type];
		}

		public IUIFlowObject GetUI<UIClass>(){
			return GetUI(typeof(UIClass));
		}

		public void ClearRegisteredUI(){
			uiObjects.Clear();
		}

		public void UnregisterUI(IUIFlowObject _object){
			uiObjects.Remove(_object.GetType());
		}

		public IUIFlowObject CurrentUI{
			get{
				return this.currentUiInfo.UI;
			}
		}
		#endregion

		private List<UIInfo> uiStack = new List<UIInfo> ();
		private UIInfo reservationUI = null;
		private UIInfo currentUiInfo { get{ return uiStack [uiStack.Count - 1]; }}
		private IUIFlowObject baseUi = null;
		private bool isWait = false;

		public void LoadScene(string _sceneName){
			cleaningValriables();
			UnityEngine.SceneManagement.SceneManager.LoadScene (_sceneName);
		}

		public void QuitApp(){
			cleaningValriables();
			Application.Quit();
		}

		public bool IsMoving{
			get{
				return isWait;
			}
		}

		private void cleaningValriables(){
			UIFlow.Instance.ClearRegisteredUI();
			backAction = null;
			uiStack.Clear();
			reservationUI = null;

			if(ClassInitializer.ClassInitializer.Instance != null){
				ClassInitializer.ClassInitializer.Instance.Deinitialize();
			}
		}

		public void Enter(string _objectName, OpenMode _openMode, object _parameter = null){
			IUIFlowObject _ui = null;
			var _object = GameObject.Find (_objectName);
			if (_object == null) {
				throw new System.Exception ("not found object " + _objectName);
			}

			_ui = _object.GetComponent<IUIFlowObject>();

			Enter (_ui, _openMode, _parameter);
		}

		public void Enter<UIClass>(OpenMode _openMode, object _parameter = null){
			Type _objectType = typeof(UIClass);
			
			if(uiObjects.ContainsKey(_objectType) == false){
				throw new System.Exception ("[UIFLOW] not found object " + _objectType.ToString());
			}

			Enter (uiObjects[_objectType], _openMode, _parameter);
		}

		public void Enter(IUIFlowObject _ui, OpenMode _openMode, object _parameter = null){
			if (isWait == true) {
				#if UNITY_EDITOR
				Debug.Log("[UIFLOW] Failed OPEN " + _ui.ToString() + " because closing other UI, and Reservation");
				#endif

				if(reservationUI != null){
					throw new System.Exception("[UIFLOW] did not reservation UI " + _ui.ToString());
				}

				reservationUI = new UIInfo(_ui, _openMode, _parameter);
				return;
			}

			#if UNITY_EDITOR
			Debug.Log("[UIFLOW] OPEN " + _ui.ToString() + "(mode:" + _openMode.ToString() + "stack:" +uiStack.Count.ToString() + ")");
			#endif

			if (uiStack.Count > 0 && _openMode == OpenMode.Change) {
				close (currentUiInfo.UI, _ui, CloseMode.Disable, () => {
					open (_ui, _openMode, _parameter);
				}, _parameter, false);
			} else {
				open (_ui, _openMode, _parameter);
			}
		}

		public void Back(CloseMode _closeMode, object _parameter = null){
			if (isWait == true) {
				return;
			}


			var _openMode = currentUiInfo.OpenMode;
			var _openFromUI = currentUiInfo.UI;
			IUIFlowObject _closeFromUI = null;
			if (uiStack.Count > 1) {
				_closeFromUI = uiStack [uiStack.Count - 2].UI;	
			}


			close (currentUiInfo.UI, _closeFromUI, _closeMode, ()=>{

				#if UNITY_EDITOR
				Debug.Log("[UIFLOW] Back " + currentUiInfo.UI.ToString() + "(mode:" + _openMode.ToString() + "stack:" +uiStack.Count.ToString() + ")");
				#endif

				if (uiStack.Count > 0 && _openMode == OpenMode.Change) {
					backAction = null;
					currentUiInfo.UI.gameObject.SetActive(true);
					currentUiInfo.UI.OnOpenedUI(_openFromUI, currentUiInfo.Parameter);
				}else{
					if(uiStack.Count > 0){
						backAction = null;
						currentUiInfo.UI.OnOpenedUI(_openFromUI, currentUiInfo.Parameter);
					}
				}
			}, _parameter);
		}

		private void finishWait(){
			isWait = false;
			if(reservationUI != null){
				var _reservationUI = reservationUI;
				reservationUI = null;
				Enter(_reservationUI.UI, _reservationUI.OpenMode, _reservationUI.Parameter);
			}
		}


		private struct BackToParam
		{
			public IUIFlowObject UI;
			public Action Callback;
			public BackToParam(IUIFlowObject _ui, Action _callback){
				UI = _ui;
				Callback = _callback;
			}
		}

		public void BackTo<UIClass>(Action _callback){
			BackTo(uiObjects[typeof(UIClass)], _callback);
		}

		public void BackTo(IUIFlowObject _ui, Action _callback){
			
			StartCoroutine ("backToCourutine", new BackToParam(_ui, _callback));
		}

		private IEnumerator backToCourutine(BackToParam _param){

			int _warnningCounter = 0;

			while (currentUiInfo.UI != _param.UI) {
				Back (CloseMode.Disable);
				while (isWait == true) {
					yield return new WaitForSeconds (0.1f);
				}

				_warnningCounter++;
				if (_warnningCounter > 100) {
					Debug.LogWarning ("waring UIFlow.BackTo");
				}
			}

			if (_param.Callback != null) {
				_param.Callback ();
			}
		}

		private Action backAction = null;
		public void SetBackKeyAction(Action _action){
			backAction = _action;
		}

		public void Replace(string _objectName, OpenMode _openMode, CloseMode _closeMode, object _openParameter = null, object _closeParameter = null){
			if (isWait == true) {
				return;
			}

			var _ui = GameObject.Find (_objectName).GetComponent<IUIFlowObject>();
			if (_ui == null) {
				throw new System.Exception ("not found object " + _objectName);
			}

			Replace (_ui, _openMode, _closeMode, _openParameter, _closeParameter);
		}
		
		public void Replace<UIClass>(OpenMode _openMode, CloseMode _closeMode, object _openParameter = null, object _closeParameter = null){
			Type _objectType = typeof(UIClass);

			if(uiObjects.ContainsKey(_objectType) == false){
				throw new System.Exception ("[UIFLOW] not found object " + _objectType.ToString());
			}

			Replace (uiObjects[_objectType], _openMode, _closeMode, _openParameter, _closeParameter);
		}
		
		public void Replace(IUIFlowObject _ui, OpenMode _openMode, CloseMode _closeMode, object _openParameter = null, object _closeParameter = null){
			if (isWait == true) {
				return;
			}


			#if UNITY_EDITOR
			Debug.Log("[UIFLOW] Replace " + _ui.ToString() + "(mode:" + _openMode.ToString() + "stack:" +uiStack.Count.ToString() + ")");
			#endif

			if (uiStack.Count > 0 && uiStack[uiStack.Count - 1].UI != baseUi) {
				var _currentUI = currentUiInfo.UI;
				close (currentUiInfo.UI, _ui,_closeMode, () => {
					open (_ui, _openMode, _openParameter, _currentUI);
				}, _closeParameter);
			} else {
				open (_ui, _openMode, _openParameter);
			}
		}

		private void close(IUIFlowObject _ui, IUIFlowObject _fromUI, CloseMode _closeMode, Action _finishCallback, object _parameter, bool _needPop = true){
			isWait = true;

			Action _closeUI = () => closeUI(_ui, _closeMode);

			Action _manageStack = () => {
				if (_needPop == true) {
					uiStack.RemoveAt (uiStack.Count - 1);
				}

				_finishCallback ();
				finishWait();
			};

			Action _finishFunc = () => {
				if(_closeUI != null){
					_closeUI();	
				}

				if(_manageStack != null){
					_manageStack();
				}
			};

			var _result = _ui.OnClosedUI (_fromUI, _finishFunc, _parameter);

			if (_result == OnCloseUIResult.DoNotWait) {
				_finishFunc ();
				finishWait();
			} else if (_result == OnCloseUIResult.WaitForFinishCallbackAndFastDisplayNext) {
				_manageStack ();
				_manageStack = null;
			}
		}

		private void closeUI(IUIFlowObject _ui, CloseMode _closeMode){
			switch (_closeMode) {
			case CloseMode.Destroy:
				MonoBehaviour.Destroy(_ui.gameObject);
				break;
			case CloseMode.Disable:
				_ui.gameObject.SetActive(false);
				break;
			case CloseMode.None:
				break;
			}
		}

		private void open(IUIFlowObject _ui, OpenMode _openMode, object _parameter, IUIFlowObject _fromUI = null){

			IUIFlowObject _fromUIResult = _fromUI; 
			if (_fromUIResult == null && uiStack.Count > 0) {
				_fromUIResult = currentUiInfo.UI;
			}

			uiStack.Add (new UIInfo(_ui, _openMode, _parameter));
			backAction = null;
			currentUiInfo.UI.gameObject.SetActive(true);
			currentUiInfo.UI.OnOpenedUI (_fromUIResult, _parameter);
		}

		public void SetBase(IUIFlowObject _ui, OpenMode _openMode, object _param = null){
			baseUi = _ui;
			open(_ui, _openMode, _param);
		}

		#region LifeCycle
		#if UNITY_ANDROID || UNITY_EDITOR
		private void Update(){
			checkBackKey ();
		}
		#endif
		private void checkBackKey(){
			if(Input.GetKeyUp(KeyCode.Escape))
			{
				if (backAction != null) {
					backAction ();
					return;
				}

				if (uiStack.Count > 1) {
					Back (CloseMode.Disable);
				}
			}
		}
		#endregion
	}

	public class UIInfo{
		public IUIFlowObject UI;
		public OpenMode OpenMode;
		public object Parameter;

		public UIInfo(IUIFlowObject _ui, OpenMode _openMode, object _parameter){
			UI = _ui;
			Parameter = _parameter;
			OpenMode = _openMode;
		}
	}

	public enum OpenMode 
	{
		Change,
		Overlap
	}

	public enum CloseMode
	{
		Destroy,
		Disable,
		None
	}

}
