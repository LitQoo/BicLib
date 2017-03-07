using System;
using System.Collections.Generic;
using BicDB;

namespace BicDB.Variable
{

	public class EnumVariable<T> : VariableBase, IEnumVariable<T> where  T : struct
	{

		new public event Action<IEnumVariable<T>> OnChangedValueActions = delegate{};

		#region AsValue
		protected T data;
		public int AsInt{ get{ return (int)Enum.ToObject(typeof(T), data); } set{ data = (T)Enum.ToObject(typeof(T), value); NotifyChanged ();} }
		public string AsString{ get{ return data.ToString (); } set{ AsInt = (int)Enum.Parse(typeof(T), value);} }
		public float AsFloat{ get{ return (float)AsInt; } set{ AsInt = (int)value;} }
		public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{ AsInt = (value ? 1 : 0) ;} }
		public VariableType Type { get { return VariableType.Int; }}

		public T AsEnum{ get{ return data; } set{ data = value; NotifyChanged ();}}
		#endregion

		public EnumVariable() : base(){

		}

		public EnumVariable(T _value) : base(){
			data = _value;
		}

		public void LoadValue(string _value){
			AsString = _value;
			IsChanged = false;
		}


		new public void NotifyChanged(){
			IsChanged = true;

			if (onSetValueActions.ContainsKey(data)) {
				onSetValueActions[data]();
			}

			OnChangedValueActions (this as IEnumVariable<T>);

		}

		#region SubscribeSetValue
		private Dictionary<T, Action> onSetValueActions = new Dictionary<T, Action>();

		public void SubscribeSetValue(T _enum, Action _callback){
			if (onSetValueActions.ContainsKey(_enum)) {
				onSetValueActions[_enum] += () => _callback();
			} else {
				onSetValueActions[_enum] = () => _callback();
			}
		}
		#endregion

	}
}

