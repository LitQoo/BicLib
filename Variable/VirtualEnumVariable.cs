using System;
using BicDB;
using BicDB.Utility;

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
		public DataType Type { get { return DataType.Int; }}
		#endregion

		#region IDataBase
		public VirtualEnumVariable() : base(){
		}

		public VirtualEnumVariable(Func<T> _func) : base(){
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