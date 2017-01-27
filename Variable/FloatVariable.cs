using System;
using BicDB;

namespace BicDB.Variable
{
	public class FloatVariable : VariableBase, IVariable {
		#region AsValue
		private float data;
		public int AsInt{ get{ return (int)data; } set{ data = value; NotifyChanged ();} }
		public string AsString{ get{ return data.ToString (); } set{ data = float.Parse (value);  NotifyChanged ();} }
		public float AsFloat{ get{ return data; } set{ data = value;  NotifyChanged ();} }
		public bool AsBool{ get{ return data == 0 ? false : true; } set{ data = (value ? 1 : 0) ;  NotifyChanged ();} }
		public VariableType Type { get { return VariableType.Float; }}
		#endregion

		public FloatVariable(float _value) : base(){
			data = _value;
		}

		public void LoadValue(string _value){
			data = float.Parse (_value);
			IsChanged = false;
		}
	}
}
