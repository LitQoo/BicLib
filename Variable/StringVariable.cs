using System;
using BicDB;

namespace BicDB.Variable
{
	public class StringVariable : VariableBase, IVariable{
		#region AsValue
		private string data;
		public int AsInt{ get{ return Int32.Parse(data); } set{ AsString = value.ToString();} }
		public string AsString{ get{ return data; } set{ data = value;  NotifyChanged ();} }
		public float AsFloat{ get{ return  (float)Double.Parse(data); } set{ AsString = value.ToString();} }
		public bool AsBool{ get{ return data == "true" ? true : false; } set{ AsString = (value ? "true" : "false") ;} }
		public VariableType Type { get { return VariableType.String; }}
		#endregion

		public StringVariable() : base(){
			
		}

		public StringVariable(string _value) : base(){
			data = _value;
		}

		public void LoadValue(string _value){
			AsString = _value;
			IsChanged = false;
		}
	}
}

