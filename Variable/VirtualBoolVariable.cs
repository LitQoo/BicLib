using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class VirtualBoolVariable : BoolVariable
	{
		override protected bool data {
			get { 
				return getter ();
			}

			set { 
				setter (value);
			}
		}

		private Func<bool> getter;
		private Action<bool> setter;

		public VirtualBoolVariable(Func<bool> _getter, Action<bool> _setter = null) : base(){
			getter = _getter;
			setter = _setter;
		}

	}
}