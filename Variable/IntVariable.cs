using System;
using BicDB;

namespace BicDB.Variable
{
	public class IntVariable : VariableBase, IVariable{
		#region AsValue
		protected int data;
		public int AsInt{ get{ return data; } set{ data = value; NotifyChanged ();} }
		public string AsString{ get{ return AsInt.ToString (); } set{ AsInt = int.Parse (value);  NotifyChanged ();} }
		public float AsFloat{ get{ return (float)AsInt; } set{ AsInt = (int)value;  NotifyChanged ();} }
		public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{ AsInt = (value ? 1 : 0) ;  NotifyChanged ();} }
		public VariableType Type { get { return VariableType.Int; }}
		#endregion


		public IntVariable(int _value) : base(){
			data = _value;
		}

		public void LoadValue(string _value){
			data = int.Parse (_value);
			IsChanged = false;
		}
	}
}

