using System;
using BicDB;

namespace BicDB.Container
{
	public class VirtualFloatVariable : VariableBase, IVariable
	{
		#region AsValue
		private Func<float> data;
		public float AsFloat{ get{ return data(); } set{ throwSetException ();} }
		public int AsInt{ get{ return (int)AsFloat; } set{ throwSetException (); } }
		public string AsString{ get{ return AsFloat.ToString (); } set{ throwSetException (); } }
		public bool AsBool{ get{ return AsFloat == 0 ? false : true; } set{ throwSetException (); } }
		public DataType Type { get { return DataType.Float; }}
		#endregion

		public VirtualFloatVariable() : base(){
		}

		public VirtualFloatVariable(Func<float> _func) : base(){
			data = _func;
		}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			throwSetException ();
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}

		private void throwSetException(){
			throw new SystemException ("this variable not support to write");
		}
	}
}