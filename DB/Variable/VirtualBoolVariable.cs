using System;
using BicDB;

namespace BicDB.Variable
{
	public class VirtualBoolVariable : BoolVariable
	{
		private bool _data;
		override protected bool data {
			get { 
				_data = Getter ();
				return _data;
			}

			set { 
				_data = value;
				Setter (_data);
			}
		}

		public Func<bool> Getter;
		public Action<bool> Setter;

		public VirtualBoolVariable(Func<bool> _getter = null, Action<bool> _setter = null) : base(){
			Getter = _getter;
			Setter = _setter;
		}

		public new void ClearNotifyAndBinding(){
			base.UnsubscribeAll();
			Getter = null;
			Setter = null;
		}

	}
}