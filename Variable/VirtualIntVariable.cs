using System;
using BicDB;

namespace BicDB.Variable
{
	public class VirtualIntVariable : DataBase, IVariable
	{
		#region AsValue
		private Func<int> data;
		public int AsInt{ get{ return data(); } set{throwSetException ();} }
		public string AsString{ get{ return AsInt.ToString (); } set{ throwSetException ();} }
		public float AsFloat{ get{ return (float)AsInt; } set{ throwSetException ();} }
		public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{throwSetException ();} }
		public DataType Type { get { return DataType.Int; }}
		public string AsFormattedString { get; set; }
		#endregion

		public VirtualIntVariable() : base(){
		}

		public VirtualIntVariable(Func<int> _func) : base(){
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