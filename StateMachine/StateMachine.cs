using System.Collections;
using System.Collections.Generic;
using BicDB;
using BicDB.Variable;
using System;

namespace BicUtil.StateMachine{
	public class StateMachine<T> where T : struct{
		public T Current{ get{ return currentState; } }
		public T Last{ get{ return lastState; } }
		public string ChainName { get { return chainName; } }
		public Dictionary<string, bool> LastTriggerList = new Dictionary<string, bool>();

		private T currentState;
		private T lastState;
		private string chainName = "none";

		private Dictionary<T, List<Action>> stateCallbacks = new Dictionary<T, List<Action>> ();

		// [State][ChainName][trigger] = triggerValue
		private Dictionary<T, Dictionary<string, Dictionary<string, bool>>> waitInfo = new Dictionary<T, Dictionary<string, Dictionary<string, bool>>> ();

		public bool Load(T _state){

			var _chainName = isComplete (_state);
			if (_chainName == "none") {
				return false;
			}

			if (waitInfo.ContainsKey (_state)) {
				LastTriggerList = waitInfo [_state][_chainName];
			} else {
				LastTriggerList = new Dictionary<string, bool> ();
			}

			chainName = _chainName;
			waitInfo.Clear ();
			lastState = currentState;
			currentState = _state;

			var _list = stateCallbacks [_state];
			for (int i = 0; i < _list.Count; i++) {
				_list [i] ();
			}

			return true;
		}

		public void Complete(string _trigger){
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
				UnityEngine.Debug.LogWarning (_trigger + " trigger is not registered in any state");
			}
		}

		public void WaitAndLoad(string _trigger, T _state, string _chainName = "default"){
			if (!waitInfo.ContainsKey (_state)) {
				waitInfo [_state] = new Dictionary<string, Dictionary<string, bool>> ();
			}

			if(!waitInfo[_state].ContainsKey(_chainName)){
				waitInfo [_state] [_chainName] = new Dictionary<string, bool> ();
			}

			var _waitStat = waitInfo [_state][_chainName];

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
				return "default";
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

			return "none";
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
	}

	public class OnChangedStateToDelegator<T> where  T : struct{
		private Dictionary<T, Func<StateResultType>> onSetValueActions = new Dictionary<T, Func<StateResultType>>();

		public Func<StateResultType> this[T _enum]{
			get{
				if (!onSetValueActions.ContainsKey(_enum)) {
					return null;
				}

				return onSetValueActions[_enum];
			}

			set{ 
				onSetValueActions[_enum] = value;
			}
		}
	}

	public enum StateResultType 
	{
		Complete,
		WaitUserInput,
		WaitAnimation,
		WaitValue
	}

	public class StateResult<T> where T : struct{
		static public readonly StateResult<T> WaitAnimation = new StateResult<T>(StateResultType.WaitAnimation);
		static public readonly StateResult<T> WaitValue = new StateResult<T>(StateResultType.WaitValue);
		static public readonly StateResult<T> WaitUserInput = new StateResult<T>(StateResultType.WaitUserInput);
		static public readonly StateResult<T> Complete = new StateResult<T>(StateResultType.Complete);
		static public StateResult<T> CompleteAndSetNext(T _nextState){
			return new StateResult<T> (StateResultType.Complete, _nextState);
		}

		public StateResultType Result;
		public T NextState;

		private StateResult(StateResultType _result){
			Result = _result;
		}

		private StateResult(StateResultType _result, T _nextStage){
			Result = _result;
			NextState = _nextStage;
		}
	}
}