using System;
using BicDB;

namespace BicDB.Variable
{
	public class FloatVariable : IVariable {
		#region AsValue
		private float data;
		public int AsInt{ get{ return (int)data; } set{ data = value; NotifyChangedValue ();} }
		public string AsString{ get{ return data.ToString (); } set{ data = float.Parse (value);  NotifyChangedValue ();} }
		public float AsFloat{ get{ return data; } set{ data = value;  NotifyChangedValue ();} }
		public bool AsBool{ get{ return data == 0 ? false : true; } set{ data = (value ? 1 : 0) ;  NotifyChangedValue ();} }
		public VariableType Type { get { return VariableType.Float; }}
		#endregion

		public event Action<IVariable> OnChangedValueActions = delegate{};
		public FloatVariable(float _value){
			data = _value;
		}

		public void NotifyChangedValue(){
			OnChangedValueActions (this);
		}
	}
}
