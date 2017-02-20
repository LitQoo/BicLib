using System;
using BicDB;

namespace BicDB.Variable
{
	public class IntVariable : VariableBase, IVariable{
		#region AsValue
		protected int data;
		public int AsInt{ get{ return data; } set{ data = value; NotifyChanged ();} }
		public string AsString{ get{ return AsInt.ToString (); } set{ AsInt = int.Parse (value);} }
		public float AsFloat{ get{ return (float)AsInt; } set{ AsInt = (int)value;} }
		public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{ AsInt = (value ? 1 : 0) ;} }
		public VariableType Type { get { return VariableType.Int; }}
		#endregion

		public IntVariable() : base(){
			
		}

		public IntVariable(int _value) : base(){
			data = _value;
		}

		public void LoadValue(string _value){
			AsString = _value;
			IsChanged = false;
		}
	}
		
	public class VirtualIntVariable : VariableBase, IVariable{
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

