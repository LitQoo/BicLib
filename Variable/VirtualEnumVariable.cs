using System;
using BicDB;

namespace BicDB.Variable
{
	public class VirtualEnumVariable<T> : VariableBase, IVariable where  T : struct
	{
		#region AsValue
		private Func<T> data;
		public int AsInt{ get{ return (int)Enum.ToObject(typeof(T), data()); } set{ throwSetException(); } }
		public string AsString{ get{ return data().ToString (); } set{ throwSetException(); } }
		public float AsFloat{ get{ return (float)AsInt; } set{ throwSetException(); } }
		public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{ throwSetException(); } }
		public VariableType Type { get { return VariableType.Int; }}
		#endregion

		public VirtualEnumVariable() : base(){
		}

		public VirtualEnumVariable(Func<T> _func) : base(){
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