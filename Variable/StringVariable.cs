using System;
using BicDB;

namespace BicDB.Variable
{
	public class StringVariable : IVariable{
		#region AsValue
		private string data;
		public int AsInt{ get{ return Int32.Parse(data); } set{ data = value.ToString(); NotifyChangedValue ();} }
		public string AsString{ get{ return data; } set{ data = value;  NotifyChangedValue ();} }
		public float AsFloat{ get{ return  (float)Double.Parse(data); } set{ data = value.ToString();  NotifyChangedValue ();} }
		public bool AsBool{ get{ return data == "true" ? true : false; } set{ data = (value ? "true" : "false") ;  NotifyChangedValue ();} }
		public VariableType Type { get { return VariableType.String; }}
		#endregion

		public event Action<IVariable> OnChangedValueActions = delegate{};
		public StringVariable(string _value){
			data = _value;
		}

		public void NotifyChangedValue(){
			OnChangedValueActions (this);
		}
	}
}

