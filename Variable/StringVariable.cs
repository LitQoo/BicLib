using System;
using BicDB;

namespace BicDB.Variable
{
	public class StringVariable : IVariable{
		#region AsValue
		private string data;
		public int AsInt{ get{ return Int32.Parse(data); } set{ data = value.ToString(); OnChangedValue (this);} }
		public string AsString{ get{ return data; } set{ data = value;  OnChangedValue (this);} }
		public float AsFloat{ get{ return  (float)Double.Parse(data); } set{ data = value.ToString();  OnChangedValue (this);} }
		public VariableType Type { get { return VariableType.String; }}
		#endregion

		public event Action<IVariable> OnChangedValueActions = delegate{};
		public StringVariable(string _value){
			data = _value;
		}

		public void OnChangedValue(IVariable _variable){
			OnChangedValueActions (this);
		}
	}
}

