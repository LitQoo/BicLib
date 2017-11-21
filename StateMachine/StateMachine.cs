using System.Collections;
using System.Collections.Generic;
using BicDB;
using BicDB.Variable;
using System;
using System.Linq;
using System.Reflection;
using System.Globalization;

namespace BicUtil.StateMachine{
	[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	sealed class StateMachineAttribute : System.Attribute
	{
		readonly string stateName;
		
		public StateMachineAttribute(object _stateEnum)
		{
			this.stateName = _stateEnum.ToString();
			this.InsertPosition = -1;
		}
		
		public string StateName
		{
			get { return stateName; }
		}

		public int InsertPosition
		{
			get; set;
		}
	}

	public class StateMachine<T> where T : struct{
		private static readonly string FlowNameDefault = "__default__";
		private static readonly string FlowNameNone = "__none__";

		public T Current{ get{ return currentState; } }
		public T Last{ get{ return lastState; } }
		public string FlowName { get { return flowName; } }
		public Dictionary<string, bool> LastTriggerList = new Dictionary<string, bool>();

		private T currentState;
		private T lastState;
		private string flowName = FlowNameNone;

		private Dictionary<T, List<Action>> stateCallbacks = new Dictionary<T, List<Action>> ();

		// [State][FlowName][trigger] = triggerValue
		private Dictionary<T, Dictionary<string, Dictionary<string, bool>>> waitInfo = new Dictionary<T, Dictionary<string, Dictionary<string, bool>>> ();

		public bool Load(T _state){
			
			var _flowName = isComplete (_state);
			if (_flowName == FlowNameNone) {
				return false;
			}

			if (waitInfo.ContainsKey (_state)) {
				LastTriggerList = waitInfo [_state][_flowName];
			} else {
				LastTriggerList = new Dictionary<string, bool> ();
			}

			flowName = _flowName;
			waitInfo.Clear ();
			lastState = currentState;
			currentState = _state;

			var _list = stateCallbacks [_state].ToArray();
			for (int i = 0; i < _list.Length; i++) {
				_list [i] ();
			}

			return true;
		}

		public void Finish(string _trigger){
			bool _isFind = false;

			foreach (var _stateInfo in waitInfo){
				foreach (var _flowInfo in _stateInfo.Value) {
					var waitStat = _flowInfo.Value;
					if (waitStat.ContainsKey (_trigger)) {
						waitStat [_trigger] = true;
						_isFind = true;
						if (Load (_stateInfo.Key) == true) {
							return;
						}
					}
				}
			}

			if (_isFind == false) {
				UnityEngine.Debug.LogWarning (_trigger + " trigger is not registered in " + currentState.ToString() + " state");
			}
		}

		public void WaitAndLoad(string _trigger, T _state, string _flowName = ""){
			if (_flowName == "") {
				_flowName = FlowNameDefault;
			}

			if (!waitInfo.ContainsKey (_state)) {
				waitInfo [_state] = new Dictionary<string, Dictionary<string, bool>> ();
			}

			if(!waitInfo[_state].ContainsKey(_flowName)){
				waitInfo [_state] [_flowName] = new Dictionary<string, bool> ();
			}

			var _waitStat = waitInfo [_state][_flowName];

			if (_waitStat.ContainsKey (_trigger)) {
				throw new SystemException (_trigger + " is already added");
			} else {
				_waitStat.Add (_trigger, false);
			}
		}

		public void RemoveAllWait(){
			waitInfo.Clear ();
		}

		public void RemoveAllFinishedTrigger(){
			var _waitInfoKeys = waitInfo.Keys.ToArray();
				foreach (T _state in _waitInfoKeys) {
				var _flowNameKeys = waitInfo [_state].Keys.ToArray();
				foreach (string _flowName in _flowNameKeys) {
					var _triggerNameKeys = waitInfo [_state] [_flowName].Keys.ToArray();
					foreach (string _triggerName in _triggerNameKeys) {
						waitInfo [_state] [_flowName] [_triggerName] = false;
					}
				}
			}
		}

		private string isComplete(T _state){

			if (!waitInfo.ContainsKey (_state)) {
				return FlowNameDefault;
			}

			bool isComplete = true;
			foreach (var _flowInfo in waitInfo[_state]) {
				isComplete = true;
				foreach (var _triggerInfo in _flowInfo.Value) {
					isComplete = isComplete && _triggerInfo.Value;
				}

				if (isComplete == true) {
					return _flowInfo.Key;
				}
			}

			return FlowNameNone;
		}

		[Obsolete("use Subscribe or SubscribeByAttribute")]
		public void SetOnChangedStateTo(T _state, Action _callback, int _insertPosition = -1){
			Subscribe(_state, _callback, _insertPosition);
		}

		public void Subscribe(T _state, Action _callback, int _insertPosition = -1){
			if(!stateCallbacks.ContainsKey(_state)){
				stateCallbacks[_state] = new List<Action>();
			}

			if (_insertPosition == -1) {
				stateCallbacks [_state].Add (_callback);
			} else {
				stateCallbacks [_state].Insert (_insertPosition, _callback);
			}
		}

		[Obsolete("use Dispoable")]
		public void RemoveOnChangedStateTo(T _state, Action _callback){
			Disposable(_state, _callback);
		}

		public void Disposable(T _state, Action _callback){
			if(!stateCallbacks.ContainsKey(_state)){
				UnityEngine.Debug.LogWarning (_state.ToString () + " State not found");
				return;
			}

			if (stateCallbacks [_state].Contains (_callback)) {
				stateCallbacks [_state].Remove (_callback);
			} else {
				UnityEngine.Debug.LogWarning (_state.ToString () + " State Callback not found");
			}
		}

		[Obsolete("use DisposableAll")]
		public void ClearAllChangedStateTo(){
			DisposableAll();
		}

		public void DisposableAll(){
			stateCallbacks.Clear();
		}

		public void SubscribeByAttribute(object _object){
			var methods = _object.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach(var _method in methods){
				StateMachineAttribute _u = (StateMachineAttribute)_method.GetCustomAttributes(typeof(StateMachineAttribute), true).FirstOrDefault();
				if(_u != null){
					Action _action = ()=>_method.Invoke(_object, BindingFlags.InvokeMethod, null, null, CultureInfo.CurrentCulture);
					this.Subscribe((T) Enum.Parse(typeof(T), _u.StateName), _action, _u.InsertPosition);
				}
			}
		}

	}
}