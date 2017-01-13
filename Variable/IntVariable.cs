using System;
using BicDB;

namespace BicDB.Variable
{
	public class IntVariable : IVariable{
		#region AsValue
		private int data;
		public int AsInt{ get{ return data; } set{ data = value; OnChangedValue (this);} }
		public string AsString{ get{ return data.ToString (); } set{ data = Int32.Parse (value);  OnChangedValue (this);} }
		public float AsFloat{ get{ return (float)data; } set{ data = (int)value;  OnChangedValue (this);} }
		public VariableType Type { get { return VariableType.Int; }}
		#endregion

		public event Action<IVariable> OnChangedValueActions = delegate{};
		public IntVariable(int _value){
			data = _value;
		}

		public void OnChangedValue(IVariable _variable){
			OnChangedValueActions (this);
		}
	}
}

