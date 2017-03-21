using System;
using BicDB;
using BicDB.Utility;

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
		public DataType Type { get { return DataType.String; }}
		#endregion

		#region LifeCycle
		public VirtualStringVariable() : base(){
		}

		public VirtualStringVariable(Func<string> _func) : base(){
			data = _func;
		}
		#endregion

		#region IDataBase
		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			throwSetException ();
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}

		public string GetFormattedString(IStringFormatter _formatter = null)
		{
			if (_formatter == null) {
				_formatter = JsonConvertor.GetInstance();
			}

			string _result = string.Empty;
			_formatter.BuildFormattedString(this, ref _result);
			return _result;
		}
		#endregion

		#region Logic
		private void throwSetException(){
			throw new SystemException ("this variable not support to write");
		}
		#endregion
	}
}