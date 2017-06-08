using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using BicDB.Container;
using System.Runtime.InteropServices;
using BicDB.Utility;

namespace BicDB.Variable
{
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

		static public void SetVariableProperty(ref IListContainer _member, IListContainer _value, Action<IDataBase> _addedCallback, Action _clearedCallback = null){
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
					case DataType.Table:
						return false;
					case DataType.DataStore:
						return false;
					case DataType.Record:
						return false;
					default:
						return _variable1.AsString == _variable2.AsString;
				}	
			}

			return false;
		}

		static public bool IsEqual(IDataBase _data1, IDataBase _data2){
			if (_data1 == null && _data2 == null) {
				return true;
			} else if (_data1 == null || _data2 == null) {
				return false;
			}

			string _data1FormattedString = string.Empty;
			string _data2FormattedString = string.Empty;

			_data1.BuildFormattedString(ref _data1FormattedString, JsonConvertor.GetInstance());
			_data2.BuildFormattedString(ref _data2FormattedString, JsonConvertor.GetInstance());

			return _data1FormattedString == _data2FormattedString;
		}


	}
}

