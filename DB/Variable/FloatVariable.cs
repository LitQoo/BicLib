using System;
using System.Globalization;
using BicDB;
using BicDB.Storage;

namespace BicDB.Variable
{
	public class FloatVariable : VariableBase, IVariable {
		#region AsValue
		protected virtual float data{ get; set;}
		public int AsInt{ get{ return (int)AsFloat; } set{ AsFloat = value; } }
		public string AsString{ get{ return AsFloat.ToString ("G", CultureInfo.InvariantCulture.NumberFormat); } set{ AsFloat = parse (value);} }
		public float AsFloat{ get{ return data; } set{ data = value; NotifyChanged ();} }
		public bool AsBool{ get{ return AsFloat == 0 ? false : true; } set{ AsFloat = (value ? 1 : 0) ;} }
		public DataType Type { get { return DataType.Float; }}
		public float AsFloatWithoutNotify{
			set{data = value;}
			get{return data;}
		}
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
		#endregion

		#region parser
		private float parse(string _value){
			try {

				return float.Parse(_value.Trim() , CultureInfo.InvariantCulture.NumberFormat);
			} catch (Exception) {
				try{
					
					return float.Parse(System.Text.RegularExpressions.Regex.Replace(_value.Trim(), "[^0-9.+-]", ""), CultureInfo.InvariantCulture.NumberFormat);
				}catch(Exception){
					throw new SystemException("FloatVariable Parse Exception : '" + _value + "'");
				}
			}
		}
		#endregion
	}

	public class FloatVariableWithSetter : FloatVariable{
		new public float AsFloat{ get{ return data; } set{ data = Setter(value); NotifyChanged ();} }
		public Func<float, float> Setter{get;set;} = null;
		public FloatVariableWithSetter(Func<float, float> _setter, float _value = 0f) : base(_value){
			this.Setter = _setter;
			AsFloat = _value;
		}
	}
}