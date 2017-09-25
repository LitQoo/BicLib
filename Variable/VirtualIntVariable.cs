using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class VirtualIntVariable : IntVariable
	{
		private int _data;
		override protected int data {
			get { 
				_data = Getter ();
				return _data;
			}

			set { 
				_data = value;
				Setter (_data);
			}
		}

		public Func<int> Getter;
		public Action<int> Setter;

		public VirtualIntVariable(Func<int> _getter = null, Action<int> _setter = null) : base(){
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