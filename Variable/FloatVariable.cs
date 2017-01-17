using System;
using BicDB;

namespace BicDB.Variable
{
	public class FloatVariable : IVariable {
		#region AsValue
		private float data;
		public int AsInt{ get{ return (int)data; } set{ data = value; OnChangedValue ();} }
		public string AsString{ get{ return data.ToString (); } set{ data = float.Parse (value);  OnChangedValue ();} }
		public float AsFloat{ get{ return data; } set{ data = value;  OnChangedValue ();} }
		public bool AsBool{ get{ return data == 0 ? false : true; } set{ data = (value ? 1 : 0) ;  OnChangedValue ();} }
		public VariableType Type { get { return VariableType.Float; }}
		#endregion

		public event Action<IVariable> OnChangedValueActions = delegate{};
		public FloatVariable(float _value){
			data = _value;
		}

		public void OnChangedValue(){
			OnChangedValueActions (this);
		}
	}
}
