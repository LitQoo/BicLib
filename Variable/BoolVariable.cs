using UnityEngine;
using System.Collections;
using BicDB;
using System;

namespace BicDB.Variable
{
	public class BoolVariable : DataBase, IVariable{
		#region AsValue
		private bool data;
		public int AsInt{ get{ return AsBool ? 1 : 0; } set{ AsBool = value == 0 ? false : true;} }
		public string AsString{ get{ return AsBool.ToString().ToLower(); } set{ AsBool = bool.Parse (value);} }
		public float AsFloat{ get{ return AsBool ? 1 : 0; } set{ AsBool = value == 0 ? false : true;} }
		public bool AsBool{ get{ return data; } set{ data = value; NotifyChanged ();} }
		public DataType Type { get { return DataType.Bool; }}
		public string AsFormattedString { get; set; }
		#endregion

		public BoolVariable() : base(){
		
		}

		public BoolVariable(bool _value) : base(){
			data = _value;
		}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildNumberVariable(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}

	}
}

