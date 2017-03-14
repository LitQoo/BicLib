using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;

namespace BicDB.Variable
{
	public interface IVariableBase
	{
		void BuildVariable(ref string _json, ref int _counter, IStringParser _parser);
		void BuildFormattedString(ref string _json, IStringFormatter _formatter);

		string AsFormattedString{get;set;}
		VariableType Type { get; }
	}

	public interface IVariable : IVariableBase{
		event Action<IVariable, string> OnChangedValueActions;
		void NotifyChanged(string _message = "");
		bool IsEqual(IVariable _variable);

		int AsInt{ get; set; }
		string AsString{ get; set; }
		float AsFloat{ get; set; }
		bool AsBool{ get; set; }
		bool IsChanged{ get; set;}
	}

	public enum VariableType
	{
		Int,
		Float,
		String,
		Bool,
		List,
		Dictionary,
		Model
	}

	public class VariableBase{
		public event Action<IVariable, string> OnChangedValueActions = delegate{};

		public bool IsChanged{ get; set;}

		public VariableBase(){
			IsChanged = false;
		}

		public void NotifyChanged(string _message = ""){
			IsChanged = true;
			OnChangedValueActions (this as IVariable, _message);
		}

		public bool IsEqual(IVariable _variable){
			return VariableUtil.IsEqual(this as IVariable, _variable);
		}
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

		static public void SetVariableProperty<T>(ref IListVariable<T> _member, IListVariable<T> _value, Action<T> _addedCallback, Action _clearedCallback = null) where T : IVariableBase, new(){
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
			if (_variable1.Type == _variable2.Type) {
				switch (_variable1.Type) {
					case VariableType.Bool:
						return _variable1.AsBool == _variable2.AsBool;
					case VariableType.Float:
						return Math.Abs(_variable1.AsFloat - _variable2.AsFloat) < 0.00001f;
					case VariableType.Int:
						return _variable1.AsInt == _variable2.AsInt;
					case VariableType.List:
						return false;
					case VariableType.Dictionary:
						return false;
					default:
						return _variable1.AsString == _variable2.AsString;
				}	
			}

			return false;
		}
	}
}

