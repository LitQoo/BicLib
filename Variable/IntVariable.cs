using System;
using BicDB;

namespace BicDB.Variable
{
	public class IntVariable : IVariable{
		#region AsValue
		private int data;
		public int AsInt{ get{ return data; } set{ data = value; OnChangedValue ();} }
		public string AsString{ get{ return data.ToString (); } set{ data = int.Parse (value);  OnChangedValue ();} }
		public float AsFloat{ get{ return (float)data; } set{ data = (int)value;  OnChangedValue ();} }
		public bool AsBool{ get{ return data == 0 ? false : true; } set{ data = (value ? 1 : 0) ;  OnChangedValue ();} }
		public VariableType Type { get { return VariableType.Int; }}
		#endregion

		public event Action<IVariable> OnChangedValueActions = delegate{};
		public IntVariable(int _value){
			data = _value;
		}

		public void OnChangedValue(){
			OnChangedValueActions (this);
		}



	}
}

