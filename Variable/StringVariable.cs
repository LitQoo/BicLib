using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class StringVariable : VariableBase, IVariable
	{
		#region AsValue
		private string data;
		public int AsInt{ get{ return Int32.Parse(AsString); } set{ AsString = value.ToString();} }
		public string AsString{ get{ return data; } set{ data = value;  NotifyChanged ();} }
		public float AsFloat{ get{ return  (float)Double.Parse(AsString); } set{ AsString = value.ToString();} }
		public bool AsBool{ get{ return AsString == "true" ? true : false; } set{ AsString = (value ? "true" : "false") ;} }
		public DataType Type { get { return DataType.String; }}
		#endregion

		#region LifeCycle
		public StringVariable() : base(){
			
		}

		public StringVariable(string _value) : base(){
			data = _value;
		}
		#endregion

		#region IDataBase
		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildStringVariable(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}
		#endregion
	}
}