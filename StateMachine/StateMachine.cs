using System.Collections;
using System.Collections.Generic;
using BicDB;
using BicDB.Variable;
using System;

namespace BicUtil.StateMachine{
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

			foreach (var _v in waitInfo){
				foreach (var _item in _v.Value) {
					var waitStat = _item.Value;
					if (waitStat.ContainsKey (_trigger)) {
						waitStat [_trigger] = true;
						_isFind = true;
						if (Load (_v.Key) == true) {
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

		private string isComplete(T _state){

			if (!waitInfo.ContainsKey (_state)) {
				return FlowNameDefault;
			}

			bool isComplete = true;
			foreach (var _v in waitInfo[_state]) {
				isComplete = true;
				foreach (var _item in _v.Value) {
					isComplete = isComplete && _item.Value;
				}

				if (isComplete == true) {
					return _v.Key;
				}
			}

			return FlowNameNone;
		}


		public void SetOnChangedStateTo(T _state, Action _callback, int _insertPosition = -1){
			if(!stateCallbacks.ContainsKey(_state)){
				stateCallbacks[_state] = new List<Action>();
			}

			if (_insertPosition == -1) {
				stateCallbacks [_state].Add (_callback);
			} else {
				stateCallbacks [_state].Insert (_insertPosition, _callback);
			}
		}

		public void RemoveOnChangedStateTo(T _state, Action _callback){
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

	}
}