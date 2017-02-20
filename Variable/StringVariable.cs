using System;
using BicDB;

namespace BicDB.Variable
{
	public class StringVariable : VariableBase, IVariable{
		#region AsValue
		private string data;
		public int AsInt{ get{ return Int32.Parse(AsString); } set{ AsString = value.ToString();} }
		public string AsString{ get{ return data; } set{ data = value;  NotifyChanged ();} }
		public float AsFloat{ get{ return  (float)Double.Parse(AsString); } set{ AsString = value.ToString();} }
		public bool AsBool{ get{ return AsString == "true" ? true : false; } set{ AsString = (value ? "true" : "false") ;} }
		public VariableType Type { get { return VariableType.String; }}
		#endregion

		public StringVariable() : base(){
			
		}

		public StringVariable(string _value) : base(){
			data = _value;
		}

		public void LoadValue(string _value){
			AsString = _value;
			IsChanged = false;
		}
	}

	public class VirtualStringVariable : VariableBase, IVariable{
		#region AsValue
		private Func<string> data;
		public string AsString{ get{ return data(); } set{ throwSetException (); } }
		public int AsInt{ get{ return Int32.Parse(AsString); } set{ throwSetException ();} }
		public float AsFloat{ get{ return  (float)Double.Parse(AsString); } set{ throwSetException ();} }
		public bool AsBool{ get{ return AsString == "true" ? true : false; } set{ throwSetException ();} }
		public VariableType Type { get { return VariableType.String; }}
		#endregion

		public VirtualStringVariable() : base(){
		}

		public VirtualStringVariable(Func<string> _func) : base(){
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

