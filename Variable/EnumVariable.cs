using System;
using System.Collections.Generic;
using BicDB;

namespace BicDB.Variable
{

	public class EnumVariable<T> : VariableBase, IEnumVariable<T> where  T : struct
	{

		new public event Action<IEnumVariable<T>> OnChangedValueActions = delegate{};
		public OnSetValueDelegator<T> OnSetValueActions{ get; set; } 

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
			OnSetValueActions = new OnSetValueDelegator<T>();
		}

		public EnumVariable(T _value) : base(){
			data = _value;
			OnSetValueActions = new OnSetValueDelegator<T>();
		}

		public void LoadValue(string _value){
			AsString = _value;
			IsChanged = false;
		}


		new public void NotifyChanged(){
			IsChanged = true;

			OnSetValueActions[data]();
			OnChangedValueActions (this as IEnumVariable<T>);

		}

	}

	public class OnSetValueDelegator<T> where  T : struct{
		private Dictionary<T, Action> onSetValueActions = new Dictionary<T, Action>();

		public Action this[T _enum]{
			get{
				if (!onSetValueActions.ContainsKey(_enum)) {
					onSetValueActions[_enum] = delegate{};
				}

				return onSetValueActions[_enum];
			}

			set{ 
				if (!onSetValueActions.ContainsKey(_enum)) {
					onSetValueActions[_enum] = delegate{};
				}

				onSetValueActions[_enum] = value;
			}
		}
	}

	public interface IEnumVariable<T> : IVariable where  T : struct
	{
		new event Action<IEnumVariable<T>> OnChangedValueActions;

		OnSetValueDelegator<T> OnSetValueActions{ get; set;}
		T AsEnum{ get; set; }
	}
}

