using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using BicDB.Container;
using System.Runtime.InteropServices;

namespace BicDB.Variable
{


	public interface IVariable : IDataBase{
		event Action<IVariable, string> OnChangedValueActions;
		void NotifyChanged(string _message = "");
		bool IsEqual(IVariable _variable);

		int AsInt{ get; set; }
		string AsString{ get; set; }
		float AsFloat{ get; set; }
		bool AsBool{ get; set; }
	}



	public class OnChangedValueToDelegator<T> where  T : struct{
		private Dictionary<T, Action> onSetValueActions = new Dictionary<T, Action>();

		public Action this[T _enum]{
			get{
				if (!onSetValueActions.ContainsKey(_enum)) {
					return delegate{};
				}

				return onSetValueActions[_enum];
			}

			set{ 
				onSetValueActions[_enum] = value;
			}
		}
	}


	public class OnChangedElementDelegator<T, U>{
		private Dictionary<T, Action<int, U>> onChangedElementActions = new Dictionary<T, Action<int, U>>();

		public Action<int, U> this[T _index]{
			get{
				if (!onChangedElementActions.ContainsKey(_index)) {
					onChangedElementActions[_index] = delegate {};
				}

				return onChangedElementActions[_index];
			}

			set{ 
				onChangedElementActions[_index] = value;
			}
		}
	}

	static public class VariableUtil{
		static public void SetVariableProperty(ref IVariable _member, IVariable _value, Action<IVariable, string>[] _callback){
			if (_member != null) {
				for (int i = 0; i < _callback.Length; i++) {
					_member.OnChangedValueActions -= _callback[i];
				}
			}

			_member = _value;
			for (int i = 0; i < _callback.Length; i++) {
				_member.OnChangedValueActions += _callback[i];
			}
			_member.NotifyChanged();
		}

		static public void SetVariableProperty(ref IVariable _member, IVariable _value, Action<IVariable, string> _callback){
			if (_member != null) {
				_member.OnChangedValueActions -= _callback;
			}

			_member = _value;
			_member.OnChangedValueActions += _callback;
			_member.NotifyChanged();
		}

		static public void SetVariableProperty<T>(ref IListContainer<T> _member, IListContainer<T> _value, Action<T> _addedCallback, Action _clearedCallback = null) where T : IDataBase, new(){
			if (_member != null) {
				if (_addedCallback != null) {
					_member.OnAddedValueActions -= _addedCallback;
				}

				if (_clearedCallback != null) {
					_member.OnClearedValueActions -= _clearedCallback;
				}
			}

			_member = _value;

			if (_addedCallback != null) {
				_member.OnAddedValueActions += _addedCallback;
			}

			if (_clearedCallback != null) {
				_member.OnClearedValueActions += _clearedCallback;
			}
		}

		static public bool IsEqual(IVariable _variable1, IVariable _variable2){
			if (_variable1 == null && _variable2 == null) {
				return true;
			}else if (_variable1 == null || _variable2 == null) {
				return false;
			}

			if (_variable1.Type == _variable2.Type) {
				switch (_variable1.Type) {
					case DataType.Bool:
						return _variable1.AsBool == _variable2.AsBool;
					case DataType.Float:
						return Math.Abs(_variable1.AsFloat - _variable2.AsFloat) < 0.00001f;
					case DataType.Int:
						return _variable1.AsInt == _variable2.AsInt;
					case DataType.List:
						return false;
					case DataType.Dictionary:
						return false;
					default:
						return _variable1.AsString == _variable2.AsString;
				}	
			}

			return false;
		}

		static public bool IsEqual(IDataBase _variable1, IDataBase _variable2){
			if (_variable1 == null && _variable2 == null) {
				return true;
			} else if (_variable1 == null || _variable2 == null) {
				return false;
			}

			return _variable1.GetFormattedString() == _variable2.GetFormattedString();
		}


	}
}

