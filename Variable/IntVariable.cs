using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class IntVariable : VariableBase, IVariable{
		#region AsValue
		protected int data;
		public int AsInt{ get{ return data; } set{ data = value; NotifyChanged ();} }

		public string AsString{ 
			get{ return AsInt.ToString (); } 
			set{
				AsInt = parse(value);
			} 
		}

		public float AsFloat{ get{ return (float)AsInt; } set{ AsInt = (int)value;} }
		public bool AsBool{ get{ return AsInt == 0 ? false : true; } set{ AsInt = (value ? 1 : 0) ;} }
		public DataType Type { get { return DataType.Int; }}
		#endregion

		#region LifeCycle
		public IntVariable() : base(){
			
		}

		public IntVariable(int _value) : base(){
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

		public IVariable AsVariable{ 
			get{ 
				return this;	
			} 
		}

		public D As<D>() where D : class, IDataBase{
			return this as D;
		}
		#endregion

		#region parser
		private int parse(string _value){
			try {
				return int.Parse(_value);
			} catch (Exception) {
				return (int)float.Parse(System.Text.RegularExpressions.Regex.Replace(_value, "[^0-9.+-]", ""));
			}
		}
		#endregion
	}
}

