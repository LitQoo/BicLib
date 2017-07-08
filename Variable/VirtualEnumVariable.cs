using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class VirtualEnumVariable<T> : EnumVariable<T> where  T : struct
	{
		override protected T data {
			get { 
				return getter ();
			}

			set { 
				setter (value);
			}
		}

		private Func<T> getter;
		private Action<T> setter;

		public VirtualEnumVariable(Func<T> _getter, Action<T> _setter = null) : base(){
			getter = _getter;
			setter = _setter;
		}
	}
}