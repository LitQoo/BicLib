using System;
using BicDB;

namespace BicDB.Container
{

	public class VirtualObjectContainer<T> : ObjectContainer<T>{
		private T _data;
		override protected T data {
			get { 
				_data = Getter ();
				return _data;
			}

			set { 
				_data = value;
				Setter (_data);
			}
		}

		public Func<T> Getter;
		public Action<T> Setter;

		public VirtualObjectContainer(Func<T> _getter = null, Action<T> _setter = null){
			Getter = _getter;
			Setter = _setter;
		}

		public new void ClearNotifyAndBinding(){
			base.ClearNotifyAndBinding ();
			Getter = null;
			Setter = null;
		}
	}
}