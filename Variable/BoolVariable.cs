using UnityEngine;
using System.Collections;
using BicDB;
using System;

namespace BicDB.Variable
{
	public class BoolVariable : IVariable{
		#region AsValue
		private bool data;
		public int AsInt{ get{ return data ? 1 : 0; } set{ data = value == 0 ? false : true; OnChangedValue ();} }
		public string AsString{ get{ return data.ToString().ToLower(); } set{ data = bool.Parse (value);  OnChangedValue ();} }
		public float AsFloat{ get{ return data ? 1 : 0; } set{ data = value == 0 ? false : true;  OnChangedValue ();} }
		public bool AsBool{ get{ return data; } set{ data = value;  OnChangedValue ();} }

		public VariableType Type { get { return VariableType.Bool; }}
		#endregion

		public event Action<IVariable> OnChangedValueActions = delegate{};
		public BoolVariable(bool _value){
			data = _value;
		}

		public void OnChangedValue(){
			OnChangedValueActions (this);
		}

		public void Notify(){
			OnChangedValueActions (this);
		}

	}
}

