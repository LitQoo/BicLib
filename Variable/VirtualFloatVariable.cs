using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Variable
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

		#region LifeCycle
		public VirtualFloatVariable() : base(){
		}

		public VirtualFloatVariable(Func<float> _func) : base(){
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