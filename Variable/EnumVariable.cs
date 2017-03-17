using System;
using System.Collections.Generic;
using BicDB;

namespace BicDB.Container
{
	public interface IEnumVariable<T> : IVariable where  T : struct
	{
		new event Action<IEnumVariable<T>> OnChangedValueActions;

		OnChangedValueToDelegator<T> OnSetValueActions{ get; set;}
		T AsEnum{ get; set; }
	}

	public class EnumVariable<T> : VariableBase, IEnumVariable<T> where  T : struct
	{

		new public event Action<IEnumVariable<T>> OnChangedValueActions = delegate{};
		public OnChangedValueToDelegator<T> OnSetValueActions{ get; set; } 

		#region AsValue
		protected T data;
		public int AsInt{ get{ return (int)Enum.ToObject(typeof(T), data); } set{ data = (T)Enum.ToObject(typeof(T), value); NotifyChanged ();} }
		public string AsString{ get{ return data.ToString (); } set{ AsInt = (int)Enum.Parse(typeof(T), value);} }
		public float AsFloat{ get{ return (float)AsInt; } set{ AsInt = (int)value;} }
		public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{ AsInt = (value ? 1 : 0) ;} }
		public DataType Type { get { return DataType.Int; }}

		public T AsEnum{ get{ return data; } set{ data = value; NotifyChanged ();}}
		#endregion

		public EnumVariable() : base(){
			OnSetValueActions = new OnChangedValueToDelegator<T>();
		}

		public EnumVariable(T _value) : base(){
			data = _value;
			OnSetValueActions = new OnChangedValueToDelegator<T>();
		}

		public void NotifyChanged(){
			OnSetValueActions[data]();
			OnChangedValueActions (this as IEnumVariable<T>);

		}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildNumberVariable(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}

	}
}

