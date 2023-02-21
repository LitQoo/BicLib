using UnityEngine;
using System.Collections;
using BicDB;
using System;
using BicDB.Storage;

namespace BicDB.Variable
{
	public class BoolVariable : VariableBase, IVariable{
		#region AsValue
		virtual protected bool data { get; set; }
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

		public void BuildFormattedString(System.Text.StringBuilder _stringBuilder, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, _stringBuilder);
		}

		public IVariable AsVariable{ 
			get{ 
				return this;	
			} 
		}

		public D As<D>() where D : class, IDataBase{
			return this as D;
		}

		public void SetValueIfChagned(bool _value){
			if(this.AsBool != _value){
				this.AsBool = _value;
			}
		}
		#endregion
	}
}

