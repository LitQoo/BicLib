using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class VirtualStringVariable : StringVariable
	{
		override protected string data {
			get { 
				return getter ();
			}

			set { 
				setter (value);
			}
		}

		private Func<string> getter;
		private Action<string> setter;

		public VirtualStringVariable(Func<string> _getter, Action<string> _setter = null) : base(){
			getter = _getter;
			setter = _setter;
		}
	}
}