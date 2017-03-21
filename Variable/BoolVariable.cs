using UnityEngine;
using System.Collections;
using BicDB;
using System;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class BoolVariable : VariableBase, IVariable{
		#region AsValue
		private bool data;
		public int AsInt{ get{ return AsBool ? 1 : 0; } set{ AsBool = value == 0 ? false : true;} }
		public string AsString{ get{ return AsBool.ToString().ToLower(); } set{ AsBool = bool.Parse (value);} }
		public float AsFloat{ get{ return AsBool ? 1 : 0; } set{ AsBool = value == 0 ? false : true;} }
		public bool AsBool{ get{ return data; } set{ data = value; NotifyChanged ();} }
		public DataType Type { get { return DataType.Bool; }}
		#endregion

		#region LifeCycle
		public BoolVariable() : base(){
		
		}

		public BoolVariable(bool _value) : base(){
			data = _value;
		}
		#endregion

		#region IDatabase
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

