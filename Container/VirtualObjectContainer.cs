using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Container
{

	public class VirtualObjectContainer<T> : ObjectContainer<T>{
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

		public VirtualObjectContainer(Func<T> _getter, Action<T> _setter = null){
			getter = _getter;
			setter = _setter;
		}
	}
}