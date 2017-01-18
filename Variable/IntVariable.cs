using System;
using BicDB;

namespace BicDB.Variable
{
	public class IntVariable : IVariable{
		#region AsValue
		protected int data;
		public int AsInt{ get{ return data; } set{ data = value; OnChangedValue ();} }
		public string AsString{ get{ return AsInt.ToString (); } set{ AsInt = int.Parse (value);  OnChangedValue ();} }
		public float AsFloat{ get{ return (float)AsInt; } set{ AsInt = (int)value;  OnChangedValue ();} }
		public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{ AsInt = (value ? 1 : 0) ;  OnChangedValue ();} }
		public VariableType Type { get { return VariableType.Int; }}
		#endregion

		public event Action<IVariable> OnChangedValueActions = delegate{};
		public IntVariable(int _value){
			AsInt = _value;
		}

		public void OnChangedValue(){
			OnChangedValueActions (this);
		}
	}
}

