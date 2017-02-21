using System;
using BicDB;

namespace BicDB.Variable
{
	public class VirtualFloatVariable : VariableBase, IVariable
	{
		#region AsValue
		private Func<float> data;
		public float AsFloat{ get{ return data(); } set{ throwSetException ();} }
		public int AsInt{ get{ return (int)AsFloat; } set{ throwSetException (); } }
		public string AsString{ get{ return AsFloat.ToString (); } set{ throwSetException (); } }
		public bool AsBool{ get{ return AsFloat == 0 ? false : true; } set{ throwSetException (); } }
		public VariableType Type { get { return VariableType.Float; }}
		#endregion

		public VirtualFloatVariable() : base(){
		}

		public VirtualFloatVariable(Func<float> _func) : base(){
			data = _func;
		}

		public void LoadValue(string _value){
			throwSetException ();
		}

		private void throwSetException(){
			throw new SystemException ("this variable not support to write");
		}
	}
}