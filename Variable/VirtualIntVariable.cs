using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class VirtualIntVariable : IntVariable
	{
		override protected int data {
			get { 
				return getter ();
			}

			set { 
				setter (value);
			}
		}

		private Func<int> getter;
		private Action<int> setter;

		public VirtualIntVariable(Func<int> _getter, Action<int> _setter = null) : base(){
			getter = _getter;
			setter = _setter;
		}
	}
}