using System;
using BicDB;

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
		public VariableType Type { get { return VariableType.String; }}
		#endregion

		public StringVariable() : base(){
			
		}

		public StringVariable(string _value) : base(){
			data = _value;
		}

		public void LoadFormatString(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.ToString(this, ref _json, ref _counter);
			IsChanged = false;
		}

		public void GetFormatString(ref string _json, IStringFormatter _formatter){
			_formatter.ToFormattedString(this, ref _json);
		}
	}
}