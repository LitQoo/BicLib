using System;
using System.Collections.Generic;
using BicDB;
using BicDB.Storage;

namespace BicDB.Variable
{
	public class EnumVariable<T> : VariableBase, IEnumVariable<T> where  T : struct
	{


		#region AsValue
		virtual protected T data { get; set; }
		public int AsInt{ get{ return (int)Enum.ToObject(typeof(T), data); } set{ data = (T)Enum.ToObject(typeof(T), value); NotifyChanged ();} }
		public string AsString{ get{ return data.ToString (); } set{ AsEnum = (T)Enum.Parse(typeof(T), value);} }
		public float AsFloat{ get{ return (float)AsInt; } set{ AsInt = (int)value;} }
		public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{ AsInt = (value ? 1 : 0) ;} }
		public DataType Type { get { return DataType.Enum; }}
		#endregion

		#region LifeCycle
		public EnumVariable() : base(){
			OnSetValueActions = new OnChangedValueToDelegator<T>();
		}

		public EnumVariable(T _value) : base(){
			data = _value;
			OnSetValueActions = new OnChangedValueToDelegator<T>();
		}
		#endregion

		#region IEnumVariable
		new public event Action<IEnumVariable<T>> OnChangedValueActions = delegate{};
		public OnChangedValueToDelegator<T> OnSetValueActions{ get; set; } 

		public T AsEnum{ get{ return data; } set{ data = value; NotifyChanged ();}}

		public new void NotifyChanged(){
			OnSetValueActions[data]();
			OnChangedValueActions (this as IEnumVariable<T>);
		}

		#endregion


		#region IDatabase
		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			if(_json[_counter] == '"'){
				_parser.BuildStringVariable(this, ref _json, ref _counter);
			}else{
				_parser.BuildNumberVariable(this, ref _json, ref _counter);
			}
		}

		public void BuildFormattedString(System.Text.StringBuilder _stringBuilder, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, _stringBuilder);
		}

		public IVariable AsVariable{ 
			get{ 
				return this;	
			} 
		}

		public D As<D>() where D : class, IDataBase{
			return this as D;
		}
		#endregion

	}
}

