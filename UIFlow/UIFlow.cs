using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.SingletonBase;
using System;

namespace BicUtil.UIFlow
{
	public class UIFlow : MonoBehaviourHardBase<UIFlow> {
		private List<UIInfo> uiStack = new List<UIInfo> ();
		private UIInfo currentUiInfo { get{ return uiStack [uiStack.Count - 1]; }}
		private bool isWait = false;

		public void Enter(string _objectName, OpenMode _openMode, object _parameter = null){
			if (isWait == true) {
				return;
			}

			var _object = GameObject.Find (_objectName);
			if (_object == null) {
				throw new System.Exception ("not found object " + _objectName);
			}

			var _ui = _object.GetComponent<IUIFlowObject>();
			Enter (_ui, _openMode, _parameter);
		}

		public void Enter(IUIFlowObject _ui, OpenMode _openMode, object _parameter = null){
			if (isWait == true) {
				return;
			}

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
				if (uiStack.Count > 0 && _openMode == OpenMode.Change) {
					currentUiInfo.UI.Enable ();
					currentUiInfo.UI.OnOpenedUI(_openFromUI, currentUiInfo.Parameter);
				}
			}, _parameter);
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

		public void Replace(IUIFlowObject _ui, OpenMode _openMode, CloseMode _closeMode, object _openParameter = null, object _closeParameter = null){
			if (isWait == true) {
				return;
			}

			if (uiStack.Count > 0) {
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
				isWait = false;
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
			} else if (_result == OnCloseUIResult.WaitForFinishCallbackAndFastDisplayNext) {
				_manageStack ();
				_manageStack = null;
			}
		}

		private void closeUI(IUIFlowObject _ui, CloseMode _closeMode){
			switch (_closeMode) {
			case CloseMode.Destroy:
				_ui.Destroy ();
				break;
			case CloseMode.Disable:
				_ui.Disable ();
				break;
			}
		}

		private void open(IUIFlowObject _ui, OpenMode _openMode, object _parameter, IUIFlowObject _fromUI = null){

			IUIFlowObject _fromUIResult = _fromUI; 
			if (_fromUIResult == null && uiStack.Count > 0) {
				_fromUIResult = currentUiInfo.UI;
			}

			uiStack.Add (new UIInfo(_ui, _openMode, _parameter));
			currentUiInfo.UI.Enable ();
			currentUiInfo.UI.OnOpenedUI (_fromUIResult, _parameter);
		}

		#region LifeCycle
		#if UNITY_ANDROID
		private void Update(){
			checkBackKey ();
		}
		#endif
		private void checkBackKey(){
			if(Input.GetKey(KeyCode.Escape))
			{
				if (uiStack.Count > 0) {
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
		Disable
	}

}
