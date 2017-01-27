using System;
using BicDB;

namespace BicDB.Variable
{
	public class StringVariable : VariableBase, IVariable{
		#region AsValue
		private string data;
		public int AsInt{ get{ return Int32.Parse(data); } set{ data = value.ToString(); NotifyChanged ();} }
		public string AsString{ get{ return data; } set{ data = value;  NotifyChanged ();} }
		public float AsFloat{ get{ return  (float)Double.Parse(data); } set{ data = value.ToString();  NotifyChanged ();} }
		public bool AsBool{ get{ return data == "true" ? true : false; } set{ data = (value ? "true" : "false") ;  NotifyChanged ();} }
		public VariableType Type { get { return VariableType.String; }}
		#endregion

		public StringVariable(string _value) : base(){
			data = _value;
		}

		public void LoadValue(string _value){
			data = _value;
			IsChanged = false;
		}
	}
}

