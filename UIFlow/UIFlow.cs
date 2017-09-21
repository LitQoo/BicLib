using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BicUtil.SingletonBase;

namespace BicUtil.UIFlow
{
	public class UIFlow : MonoBehaviourHardBase<UIFlow> {
		private Stack<UIInfo> uiStack = new Stack<UIInfo> ();
		private UIInfo currentUiInfo { get{ return uiStack.Peek (); }}

		public void Enter(string _objectName, OpenMode _openMode, object _parameter = null){
			var _object = GameObject.Find (_objectName);
			if (_object == null) {
				throw new System.Exception ("not found object " + _objectName);
			}

			var _ui = _object.GetComponent<IUIFlowObject>();
			Enter (_ui, _openMode, _parameter);
		}

		public void Enter(IUIFlowObject _ui, OpenMode _openMode, object _parameter = null){
			
			if (uiStack.Count > 0 && _openMode == OpenMode.Change) {
				currentUiInfo.UI.Disable ();
			}

			open (_ui, _openMode, _parameter);
		}

		public void Back(CloseMode _closeMode, object _parameter = null){
			var _openMode = currentUiInfo.OpenMode;
			close (currentUiInfo.UI, _closeMode, _parameter);

			if (uiStack.Count > 0 && _openMode == OpenMode.Change) {
				currentUiInfo.UI.Enable ();
			}
		}

		public void Replace(string _objectName, OpenMode _openMode, CloseMode _closeMode, object _openParameter = null, object _closeParameter = null){
			var _ui = GameObject.Find (_objectName).GetComponent<IUIFlowObject>();
			if (_ui == null) {
				throw new System.Exception ("not found object " + _objectName);
			}

			Replace (_ui, _openMode, _closeMode, _openParameter, _closeParameter);
		}

		public void Replace(IUIFlowObject _ui, OpenMode _openMode, CloseMode _closeMode, object _openParameter = null, object _closeParameter = null){
			if (uiStack.Count > 0) {
				close (currentUiInfo.UI, _closeMode, _closeParameter);
			}

			open (_ui, _openMode, _openParameter);
		}

		private void close(IUIFlowObject _ui, CloseMode _closeMode, object _parameter){
			_ui.OnClosedUI (_parameter);

			switch (_closeMode) {
			case CloseMode.Destroy:
				_ui.Destroy ();
				break;
			case CloseMode.Disable:
				_ui.Disable ();
				break;
			}

			uiStack.Pop ();
		}

		private void open(IUIFlowObject _ui, OpenMode _openMode, object _parameter){
			uiStack.Push (new UIInfo(_ui, _openMode, _parameter));
			currentUiInfo.UI.Enable ();
			currentUiInfo.UI.OnOpenedUI (_parameter);
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
				Back (CloseMode.Disable);
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
