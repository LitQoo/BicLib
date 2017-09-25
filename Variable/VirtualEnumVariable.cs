using System;
using BicDB;

namespace BicDB.Variable
{
	public class VirtualEnumVariable<T> : EnumVariable<T> where  T : struct
	{
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

		public VirtualEnumVariable(Func<T> _getter, Action<T> _setter = null) : base(){
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