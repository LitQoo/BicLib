using System.Collections;
using System.Collections.Generic;
using BicDB;
using BicDB.Variable;
using System;

namespace BicUtil.StateMachine{
	public class StateMachine<T> where T : struct{
		public T Current{ get{ return currentState; } }
		public T Last{ get{ return lastState; } }

		private T currentState;
		private T lastState;

		private Dictionary<T, List<Action>> stateCallbacks = new Dictionary<T, List<Action>> ();
		//private Dictionary<string, bool> waitStat = new Dictionary<string, bool>();
		private Dictionary<T, Dictionary<string, bool>> waitInfo = new Dictionary<T, Dictionary<string, bool>>();

		public bool Load(T _state){

			if (isComplete(_state) == false) {
				return false;
			}

			var _list = stateCallbacks [_state];
			waitInfo.Clear ();
			lastState = currentState;
			currentState = _state;
			for (int i = 0; i < _list.Count; i++) {
				_list [i] ();
			}

			return true;
		}

		public void Complete(string _tag){
			foreach (var _item in waitInfo) {
				var waitStat = _item.Value;
				if (!waitStat.ContainsKey (_tag)) {
					
				} else {
					waitStat [_tag] = true;
					if (Load (_item.Key) == true) {
						return;
					}
				}
			}
		}

		public void WaitAndLoad(string _tag, T _state){
			if (!waitInfo.ContainsKey (_state)) {
				waitInfo [_state] = new Dictionary<string, bool> ();
			}

			var waitStat = waitInfo [_state];

			if (waitStat.ContainsKey (_tag)) {
				throw new SystemException (_tag + " is already added");
			} else {
				waitStat.Add (_tag, false);
			}
		}

		public void RemoveAllWait(){
			waitInfo.Clear ();
		}

		private bool isComplete(T _state){
			bool isComplete = true;

			if (!waitInfo.ContainsKey (_state)) {
				return isComplete;
			}
				
			foreach (var _item in waitInfo[_state]) {
				isComplete = isComplete && _item.Value;
			}

			return isComplete;
		}


		public void SetOnChangedStateTo(T _state, Action _callback){
			if(!stateCallbacks.ContainsKey(_state)){
				stateCallbacks[_state] = new List<Action>();
			}

			stateCallbacks [_state].Add (_callback);
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