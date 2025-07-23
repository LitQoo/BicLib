using System;
using System.Collections.Generic;
using BicDB.Container;
using BicUtil.Json;

namespace BicDB.Variable
{

	public interface IVariable : IDataBase, IBindRmover, IVariableReadOnly{
		bool IsEqual(IVariable _variable);

		new int AsInt{ get; set; }
		new string AsString{ get; set; }
		new float AsFloat{ get; set; }
		new bool AsBool{ get; set; }
	}

	public interface IVariableReadOnly{
		[Obsolete("use Subscribe")]
		event Action<IVariableReadOnly> OnChangedValueActions;

		void Subscribe(Action<IVariableReadOnly> _callback, bool _needFirstCall = false);
		void Unsubscribe(Action<IVariableReadOnly> _callback);
		void UnsubscribeAll();
		void NotifyChanged();
		void NotifyChanged(IVariableReadOnly _value);

		int AsInt{ get;}
		string AsString{ get;}
		float AsFloat{ get;}
		bool AsBool{ get;}
	}

	public interface IEnumVariable<T> : IVariable where  T : struct
	{
		void Subscribe(Action<IEnumVariable<T>> _callback, bool _needFirstCall = false);
		void Unsubscribe(Action<IEnumVariable<T>> _callback);

		// new event Action<IEnumVariable<T>> OnChangedValueActions;

		OnChangedValueToDelegator<T> OnSetValueActions{ get; set;}
		T AsEnum{ get; set; }
	}

	public class VariableBase{
		[Obsolete("use Subscribe")]
		public event Action<IVariableReadOnly> OnChangedValueActions{
			add{
				onChangedValueActions += value;
			}

			remove{
				onChangedValueActions -= value;
			}
		}
		
		private event Action<IVariableReadOnly> onChangedValueActions = delegate{};

		public void Subscribe(Action<IVariableReadOnly> _callback, bool _needFirstCall = false){
			onChangedValueActions += _callback;
			if(_needFirstCall == true){
				_callback(this as IVariableReadOnly);
			}
		}
		
		public void Unsubscribe(Action<IVariableReadOnly> _callback){
			onChangedValueActions -= _callback;
		}

		public void UnsubscribeAll(){
			onChangedValueActions = delegate{};
		}

		public void NotifyChanged(){
			onChangedValueActions (this as IVariableReadOnly);
		}

		public void NotifyChanged(IVariableReadOnly _value){
			onChangedValueActions (this as IVariableReadOnly);
		}

		public bool IsEqual(IVariable _variable){
			return VariableUtil.IsEqual(this as IVariable, _variable);
		}

		[Obsolete("use UnsubscribeALL")]
		public void ClearNotifyAndBinding(){
			onChangedValueActions = delegate{};
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


	public class OnChangedElementDelegator<KEYTYPE, VALUETYPE>{
		private Dictionary<KEYTYPE, Action<KEYTYPE, VALUETYPE>> onChangedElementActions = new Dictionary<KEYTYPE, Action<KEYTYPE, VALUETYPE>>();
		public Action<KEYTYPE, VALUETYPE> OnChangedElementActionsAll = null;

		public Action<KEYTYPE, VALUETYPE> this[KEYTYPE _index]{
			get{
				if(OnChangedElementActionsAll != null){
					return OnChangedElementActionsAll;
				}

				if (!onChangedElementActions.ContainsKey(_index)) {
					onChangedElementActions[_index] = delegate {};
				}

				return onChangedElementActions[_index];
			}

			set{ 
				onChangedElementActions[_index] = value;
			}
		}
		
		public OnChangedElementDelegator(){

		}
		
		public OnChangedElementDelegator(Action<KEYTYPE, VALUETYPE> _callbackForAll){
			OnChangedElementActionsAll = _callbackForAll;
		}
	}

	static public class VariableUtil{
		static public bool IsVariableType(DataType _type){
			if (_type == DataType.Bool || _type == DataType.Float || _type == DataType.Int || _type == DataType.String) {
				return true;
			} else {
				return false;
			}
		}

		static public void SetVariableProperty(ref IVariable _member, IVariable _value, Action<IVariableReadOnly>[] _callback){
			if (_member != null) {
				for (int i = 0; i < _callback.Length; i++) {
					_member.Unsubscribe(_callback[i]);
				}
			}

			_member = _value;
			for (int i = 0; i < _callback.Length; i++) {
				_member.Subscribe(_callback[i]);
			}
			_member.NotifyChanged();
		}

		static public void SetVariableProperty(ref IVariable _member, IVariable _value, Action<IVariableReadOnly> _callback){
			if (_member != null) {
				_member.Unsubscribe(_callback);
			}

			_member = _value;
			_member.Subscribe(_callback);
			_member.NotifyChanged();
		}

		static public void SetVariableProperty<T>(ref IListContainer<T> _member, IListContainer<T> _value, Action<T> _addedCallback, Action _clearedCallback = null) where T : IDataBase{
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
					case DataType.Enum:
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

			System.Text.StringBuilder _stringBuilder1 = new System.Text.StringBuilder();
			System.Text.StringBuilder _stringBuilder2 = new System.Text.StringBuilder();
			_data1.BuildFormattedString(_stringBuilder1, JsonConvertor.GetInstance());
			_data2.BuildFormattedString(_stringBuilder2, JsonConvertor.GetInstance());

			return _stringBuilder1.ToString() == _stringBuilder2.ToString();
		}


	}
}

