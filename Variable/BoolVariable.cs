using UnityEngine;
using System.Collections;
using BicDB;
using System;

namespace BicDB.Variable
{
	public class BoolVariable : VariableBase, IVariable{
		#region AsValue
		private bool data;
		public int AsInt{ get{ return data ? 1 : 0; } set{ AsBool = value == 0 ? false : true;} }
		public string AsString{ get{ return data.ToString().ToLower(); } set{ AsBool = bool.Parse (value);} }
		public float AsFloat{ get{ return data ? 1 : 0; } set{ AsBool = value == 0 ? false : true;} }
		public bool AsBool{ get{ return data; } set{ data = value; NotifyChanged ();} }
		public VariableType Type { get { return VariableType.Bool; }}
		#endregion

		public BoolVariable() : base(){
		
		}

		public BoolVariable(bool _value) : base(){
			data = _value;
		}
			
		public void LoadValue(string _value){
			AsString = _value;
			IsChanged = false;
		}
	}
}

