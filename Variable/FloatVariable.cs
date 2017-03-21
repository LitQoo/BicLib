using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class FloatVariable : VariableBase, IVariable {
		#region AsValue
		private float data;
		public int AsInt{ get{ return (int)AsFloat; } set{ AsFloat = value; } }
		public string AsString{ get{ return AsFloat.ToString (); } set{ AsFloat = float.Parse (value);} }
		public float AsFloat{ get{ return data; } set{ data = value; NotifyChanged ();} }
		public bool AsBool{ get{ return AsFloat == 0 ? false : true; } set{ AsFloat = (value ? 1 : 0) ;} }
		public DataType Type { get { return DataType.Float; }}
		#endregion

		#region LifeCycle
		public FloatVariable() : base(){
			
		}

		public FloatVariable(float _value) : base(){
			data = _value;
		}
		#endregion

		#region IDataBase
		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildNumberVariable(this, ref _json, ref _counter);
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
	}
}