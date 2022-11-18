using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.SingletonBase;
using System;
using BicUtil.ClassInitializer;
using BicDB.Variable;

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

		public UIClass GetUI<UIClass>(){
			return (UIClass)GetUI(typeof(UIClass));
		}

		public void ClearRegisteredUI(){
			uiObjects.Clear();
		}

		public void UnregisterUI(IUIFlowObject _object){
			uiObjects.Remove(_object.GetType());
		}

		public IUIFlowObject CurrentUI{
			get{
				try{
					return this.currentUiInfo.UI;
				}catch{
					return null;
				}
			}
		}
		#endregion

		#region Event
		public Action OnStartChangeUI = null;
		public Action OnFinishChangeUI = null;
		#endregion

		private List<UIInfo> uiStack = new List<UIInfo> ();
		private UIInfo reservationUI = null;
		private UIInfo currentUiInfo { get{ return uiStack [uiStack.Count - 1]; }}
		private IUIFlowObject baseUi = null;
		private BoolVariable isWait = new BoolVariable(false);
		private object sceneTransitionParameter = null;
		public object GetSceneTransitionParameterAndClear(){
			var _param = this.sceneTransitionParameter;
			this.sceneTransitionParameter = null;
			return _param;
		}

		public void LoadScene(string _sceneName, object _param = null){
			cleaningValriables();
			sceneTransitionParameter = _param;
			UnityEngine.SceneManagement.SceneManager.LoadScene (_sceneName);
		}

		public void QuitApp(){
			cleaningValriables();
			Application.Quit();
		}

		public bool IsMoving{
			get{
				return isWait.AsBool;
			}
		}

		private void cleaningValriables(){
			sceneTransitionParameter = null;
			UIFlow.Instance.ClearRegisteredUI();
			backAction = null;
			uiStack.Clear();

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
			if (isWait.AsBool == true && isPassToCheckWait == false) {
				Debug.Log("[UIFLOW] Failed OPEN " + _ui.ToString() + " because closing other UI, and Reservation, Param : " + (_parameter == null?"null":_parameter.ToString()));
				
				// 예약 오픈 일단 정지?
				// if(reservationUI != null){
				// 	Debug.LogWarning("[UIFLOW] did not reservation UI " + _ui.ToString());
				// 	return;
				// }

				// reservationUI = new UIInfo(_ui, _openMode, _parameter);
				return;
			}

			Debug.Log("[UIFLOW] OPEN " + _ui.ToString() + "(mode:" + _openMode.ToString() + "stack:" +uiStack.Count.ToString() +", Param : " + (_parameter == null?"null":_parameter.ToString()) + ")");
			
			if (uiStack.Count > 0 && _openMode == OpenMode.Change) {
				close (currentUiInfo.UI, _ui, CloseMode.Disable, () => {
					open (_ui, _openMode, _parameter);
				}, _parameter, false);
				
			} else {
				isWait.AsBool = true;
				if(OnStartChangeUI != null){
					OnStartChangeUI();
				}
				open (_ui, _openMode, _parameter);
				finishWait();
			}
		}

		public void Back(CloseMode _closeMode, object _parameter = null){
			if (isWait.AsBool == true && isPassToCheckWait == false) {
				Debug.Log("[UIFLOW] Failed Back because closing other UI, and Reservation, Param : " + (_parameter == null?"null":_parameter.ToString()));
				return;
			}


			var _openMode = currentUiInfo.OpenMode;
			var _openFromUI = currentUiInfo.UI;
			IUIFlowObject _closeFromUI = null;
			if (uiStack.Count > 1) {
				_closeFromUI = uiStack [uiStack.Count - 2].UI;	
			}

			Debug.Log("[UIFLOW] Back " + currentUiInfo.UI.ToString() + "(mode:" + _openMode.ToString() + "stack:" +uiStack.Count.ToString() + ", Param : " + (_parameter == null?"null":_parameter.ToString()) + ")");

			close (currentUiInfo.UI, _closeFromUI, _closeMode, ()=>{

				if(uiStack.Count > 0){
					if(_openMode == OpenMode.Change){
						currentUiInfo.UI.gameObject.SetActive(true);
					}

					backAction = null;
					currentUiInfo.UI.OnOpenedUI(_openFromUI, currentUiInfo.Parameter);
				}

			}, _parameter);
		}

		bool isPassToCheckWait = false;
		public void MultiMove(Action _func){
			isPassToCheckWait = true;
			_func();
			isPassToCheckWait = false;
		}

		private Action onFinishedChangeUIOnceCallback = null;
		public void SubscribeOnceOnFinishedChangeUI(Action _callback){
			if(isWait.AsBool == false  && isPassToCheckWait == false){
				if(_callback != null){
					_callback();
				}
				return;
			}

			onFinishedChangeUIOnceCallback = _callback;
		}

		private void finishWait(){
			isWait.AsBool = false;
			if(onFinishedChangeUIOnceCallback != null){
				onFinishedChangeUIOnceCallback();
				onFinishedChangeUIOnceCallback = null;
			}

			if(OnFinishChangeUI != null){
				OnFinishChangeUI();
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
				while (isWait.AsBool == true) {
					yield return new WaitForSeconds (0.1f);
				}

				_warnningCounter++;
				if (_warnningCounter > 100) {
					Debug.LogWarning ("[UIFlow] warning UIFlow.BackTo");
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
			if (isWait.AsBool == true && isPassToCheckWait == false) {
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
			if (isWait.AsBool == true && isPassToCheckWait == false) {
				Debug.LogWarning("[UIFLOW] Failed Replace " + _ui.ToString() + "(mode:" + _openMode.ToString() + "stack:" +uiStack.Count.ToString() + ") Wating, OpenParam : " + (_openParameter == null?"null":_openParameter.ToString()) + ",CloseParam : " + (_closeParameter == null?"null":_closeParameter.ToString()));
				return;
			}


			Debug.Log("[UIFLOW] Replace " + _ui.ToString() + "(mode:" + _openMode.ToString() + "stack:" +uiStack.Count.ToString() + "), OpenParam : " + (_openParameter == null?"null":_openParameter.ToString()) + ",CloseParam : " + (_closeParameter == null?"null":_closeParameter.ToString()));
			
			// if (uiStack.Count > 0 && currentUiInfo.UI != baseUi) {
				var _currentUI = currentUiInfo.UI;
				close (currentUiInfo.UI, _ui,_closeMode, () => {
					open (_ui, _openMode, _openParameter, _currentUI);
				}, _closeParameter);
			// Replace 에선 이게 필요없는것 같아서 일단 주석처리
			// } else {
			// 	isWait.AsBool = true;
			// 	if(OnStartChangeUI != null){
			// 		OnStartChangeUI();
			// 	}

			// 	open (_ui, _openMode, _openParameter);
			// 	finishWait();
			// }
		}

		private void close(IUIFlowObject _ui, IUIFlowObject _fromUI, CloseMode _closeMode, Action _finishCallback, object _parameter, bool _needPop = true){
			isWait.AsBool = true;

			Action _closeUI = () => closeUI(_ui, _closeMode);

			Action _manageStack = () => {
				if (_needPop == true) {
					uiStack.RemoveAt (uiStack.Count - 1);
				}

				_finishCallback ();
			};

			Action _finishFunc = () => {
				if(_closeUI != null){
					_closeUI();	
				}

				if(_manageStack != null){
					_manageStack();
				}

				finishWait();
			};

			if(OnStartChangeUI != null){
				OnStartChangeUI();
			}

			var _result = _ui.OnClosedUI (_fromUI, _finishFunc, _parameter);

			if (_result == OnCloseUIResult.DoNotWait) {
				_finishFunc ();
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
			var _sceneParam = GetSceneTransitionParameterAndClear();
			if(_sceneParam != null){
				_param = _sceneParam;
			}

			open(_ui, _openMode, _param);
		}

		#region LifeCycle
		#if UNITY_ANDROID || UNITY_EDITOR
		private void Update(){
			checkBackKey ();
		}
		#endif


		public Func<bool> IsEnableBackKeyFunc = null;
		private void checkBackKey(){
			if(isWait.AsBool == true){
				return;
			}

			if(Input.GetKeyUp(KeyCode.Escape))
			{
				if (backAction != null) {
					if(IsEnableBackKeyFunc == null || IsEnableBackKeyFunc() == true){
						Debug.Log("[UIFlow] Touched BackKey");
						backAction ();
					}

					return;
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
