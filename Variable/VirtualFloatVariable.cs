using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class VirtualFloatVariable : FloatVariable
	{
		override protected float data {
			get { 
				return getter ();
			}

			set { 
				setter (value);
			}
		}

		private Func<float> getter;
		private Action<float> setter;

		public VirtualFloatVariable(Func<float> _getter, Action<float> _setter = null) : base(){
			getter = _getter;
			setter = _setter;
		}
	}
}