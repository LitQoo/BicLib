using System;
using BicDB;

namespace BicDB.Variable
{
	public class VirtualIntVariable : VariableBase, IVariable
	{
		#region AsValue
		private Func<int> data;
		public int AsInt{ get{ return data(); } set{throwSetException ();} }
		public string AsString{ get{ return AsInt.ToString (); } set{ throwSetException ();} }
		public float AsFloat{ get{ return (float)AsInt; } set{ throwSetException ();} }
		public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{throwSetException ();} }
		public VariableType Type { get { return VariableType.Int; }}
		#endregion

		public VirtualIntVariable() : base(){
		}

		public VirtualIntVariable(Func<int> _func) : base(){
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