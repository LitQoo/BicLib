using UnityEngine;
using System.Collections;
using BicDB;
using System;

namespace BicDB.Variable
{
	public class BoolVariable : IVariable{
		#region AsValue
		private bool data;
		public int AsInt{ get{ return data ? 1 : 0; } set{ data = value == 0 ? false : true; NotifyChangedValue ();} }
		public string AsString{ get{ return data.ToString().ToLower(); } set{ data = bool.Parse (value);  NotifyChangedValue ();} }
		public float AsFloat{ get{ return data ? 1 : 0; } set{ data = value == 0 ? false : true;  NotifyChangedValue ();} }
		public bool AsBool{ get{ return data; } set{ data = value;  NotifyChangedValue ();} }

		public VariableType Type { get { return VariableType.Bool; }}
		#endregion

		public event Action<IVariable> OnChangedValueActions = delegate{};
		public BoolVariable(bool _value){
			data = _value;
		}

		public void NotifyChangedValue(){
			OnChangedValueActions (this);
		}

	}
}

