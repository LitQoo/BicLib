using System;
using BicDB;

namespace BicDB.Variable
{
	public class VirtualBoolVariable : VariableBase, IVariable
	{
		#region AsValue
		private Func<bool> data;
		public int AsInt{ get{ return AsBool ? 1 : 0; } set{throwSetException ();} }
		public string AsString{ get{ return AsBool.ToString().ToLower(); } set{throwSetException ();} }
		public float AsFloat{ get{ return AsBool ? 1 : 0; } set{throwSetException ();} }
		public bool AsBool{ get{ return data(); } set{throwSetException ();} }
		public VariableType Type { get { return VariableType.Bool; }}
		#endregion

		public VirtualBoolVariable() : base(){
		}

		public VirtualBoolVariable(Func<bool> _func) : base(){
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