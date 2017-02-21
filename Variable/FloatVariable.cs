using System;
using BicDB;

namespace BicDB.Variable
{
	public class FloatVariable : VariableBase, IVariable {
		#region AsValue
		private float data;
		public int AsInt{ get{ return (int)AsFloat; } set{ AsFloat = value; } }
		public string AsString{ get{ return AsFloat.ToString (); } set{ AsFloat = float.Parse (value);} }
		public float AsFloat{ get{ return data; } set{ data = value; NotifyChanged ();} }
		public bool AsBool{ get{ return AsFloat == 0 ? false : true; } set{ AsFloat = (value ? 1 : 0) ;} }
		public VariableType Type { get { return VariableType.Float; }}
		#endregion

		public FloatVariable() : base(){
			
		}

		public FloatVariable(float _value) : base(){
			data = _value;
		}

		public void LoadValue(string _value){
			AsString = _value;
			IsChanged = false;
		}
	}
}