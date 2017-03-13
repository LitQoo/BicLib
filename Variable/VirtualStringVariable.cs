using System;
using BicDB;

namespace BicDB.Variable
{
	public class VirtualStringVariable : VariableBase, IVariable
	{
		#region AsValue
		private Func<string> data;
		public string AsString{ get{ return data(); } set{ throwSetException (); } }
		public int AsInt{ get{ return Int32.Parse(AsString); } set{ throwSetException ();} }
		public float AsFloat{ get{ return  (float)Double.Parse(AsString); } set{ throwSetException ();} }
		public bool AsBool{ get{ return AsString.ToLower() == "true" ? true : false; } set{ throwSetException ();} }
		public VariableType Type { get { return VariableType.String; }}
		#endregion

		public VirtualStringVariable() : base(){
		}

		public VirtualStringVariable(Func<string> _func) : base(){
			data = _func;
		}

		public void LoadFormatString(ref string _json, ref int _counter, IStringParser _parser)
		{
			throwSetException ();
		}

		public void GetFormatString(ref string _json, IStringFormatter _formatter){
			_formatter.ToFormattedString(this, ref _json);
		}

		private void throwSetException(){
			throw new SystemException ("this variable not support to write");
		}
	}
}