using System;
using BicDB;

namespace BicDB.Variable
{
	public class VirtualStringVariable : DataBase, IVariable
	{
		#region AsValue
		private Func<string> data;
		public string AsString{ get{ return data(); } set{ throwSetException (); } }
		public int AsInt{ get{ return Int32.Parse(AsString); } set{ throwSetException ();} }
		public float AsFloat{ get{ return  (float)Double.Parse(AsString); } set{ throwSetException ();} }
		public bool AsBool{ get{ return AsString.ToLower() == "true" ? true : false; } set{ throwSetException ();} }
		public DataType Type { get { return DataType.String; }}
		public string AsFormattedString { get; set; }
		#endregion

		public VirtualStringVariable() : base(){
		}

		public VirtualStringVariable(Func<string> _func) : base(){
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